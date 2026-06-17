using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ProcureFlow.Application.Auth;
using ProcureFlow.Domain;
using ProcureFlow.Infrastructure;
using ProcureFlow.Infrastructure.Persistence;
using ProcureFlow.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var configuredOrigins = builder.Configuration["Cors:AllowedOrigins"];
        var origins = string.IsNullOrWhiteSpace(configuredOrigins)
            ? ["http://localhost:4200"]
            : configuredOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var authenticationBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (!string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    authenticationBuilder.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "ProcureFlow.Api",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck");

app.MapGet("/health/database", async (IServiceProvider serviceProvider) =>
{
    var dbContext = serviceProvider.GetService<ProcureFlowDbContext>();

    if (dbContext is null)
    {
        return Results.Problem("Database connection string is not configured.");
    }

    var canConnect = await dbContext.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { status = "ok", database = "reachable" })
        : Results.Problem("Database is not reachable.");
})
.WithName("DatabaseHealthCheck");

app.MapPost("/api/setup/seed-demo-data", async (
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken) =>
{
    var seeder = serviceProvider.GetService<DemoDataSeeder>();

    if (seeder is null)
    {
        return Results.Problem("Database connection string is not configured.");
    }

    await seeder.SeedAsync(cancellationToken);

    return Results.Ok(new
    {
        message = "Demo data seeded.",
        users = new[]
        {
            new { email = "employee@demo.com", password = "employee", role = UserRole.Employee.ToString() },
            new { email = "manager@demo.com", password = "manager", role = UserRole.Manager.ToString() },
            new { email = "finance@demo.com", password = "finance", role = UserRole.Finance.ToString() }
        }
    });
})
.WithName("SeedDemoData");

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    ProcureFlowDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var currentUser = new CurrentUserDto(
        user.Id,
        user.Name,
        user.Email,
        user.Role,
        user.DepartmentId);

    var token = tokenService.CreateToken(currentUser);

    return Results.Ok(new LoginResponse(token, currentUser));
})
.WithName("Login");

var api = app.MapGroup("/api").RequireAuthorization();

api.MapGet("/me", (ClaimsPrincipal principal) =>
{
    return Results.Ok(EndpointHelpers.GetCurrentUser(principal));
});

api.MapGet("/vendors", async (ProcureFlowDbContext dbContext, CancellationToken cancellationToken) =>
{
    var vendors = await dbContext.Vendors
        .Where(x => x.IsActive)
        .OrderBy(x => x.Name)
        .Select(x => new LookupDto(x.Id, x.Name))
        .ToListAsync(cancellationToken);

    return Results.Ok(vendors);
});

api.MapGet("/requests", async (
    string? status,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var currentUser = EndpointHelpers.GetCurrentUser(principal);
    var query = dbContext.ProcurementRequests.AsNoTracking();

    if (currentUser.Role == UserRole.Employee)
    {
        query = query.Where(x => x.RequestedById == currentUser.Id);
    }

    if (Enum.TryParse<RequestStatus>(status, true, out var requestedStatus))
    {
        query = query.Where(x => x.Status == requestedStatus);
    }

    var requests = await (
        from request in query
        join vendor in dbContext.Vendors.AsNoTracking() on request.VendorId equals vendor.Id
        join requester in dbContext.Users.AsNoTracking() on request.RequestedById equals requester.Id
        orderby request.CreatedAt descending
        select new RequestListItemDto(
            request.Id,
            request.RequestNo,
            requester.Name,
            vendor.Name,
            request.Status.ToString(),
            request.EstimatedTotal,
            request.CreatedAt,
            request.SubmittedAt))
        .ToListAsync(cancellationToken);

    return Results.Ok(requests);
});

api.MapGet("/requests/{id:guid}", async (
    Guid id,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var currentUser = EndpointHelpers.GetCurrentUser(principal);
    var request = await dbContext.ProcurementRequests
        .AsNoTracking()
        .Include(x => x.Items)
        .Include(x => x.ApprovalLogs)
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    if (request is null)
    {
        return Results.NotFound();
    }

    if (currentUser.Role == UserRole.Employee && request.RequestedById != currentUser.Id)
    {
        return Results.Forbid();
    }

    var vendor = await dbContext.Vendors.AsNoTracking()
        .FirstAsync(x => x.Id == request.VendorId, cancellationToken);
    var requester = await dbContext.Users.AsNoTracking()
        .FirstAsync(x => x.Id == request.RequestedById, cancellationToken);
    var actorIds = request.ApprovalLogs.Select(x => x.ActorId).Distinct().ToList();
    var actors = await dbContext.Users.AsNoTracking()
        .Where(x => actorIds.Contains(x.Id))
        .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

    return Results.Ok(new RequestDetailDto(
        request.Id,
        request.RequestNo,
        requester.Name,
        vendor.Name,
        request.Status.ToString(),
        request.EstimatedTotal,
        request.CreatedAt,
        request.SubmittedAt,
        request.Items
            .OrderBy(x => x.Description)
            .Select(x => new RequestItemDto(x.Description, x.Quantity, x.UnitCost, x.Category, x.LineTotal))
            .ToList(),
        request.ApprovalLogs
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ApprovalLogDto(
                actors.GetValueOrDefault(x.ActorId, "Unknown"),
                x.FromStatus.ToString(),
                x.ToStatus.ToString(),
                x.Remarks,
                x.CreatedAt))
            .ToList()));
});

api.MapPost("/requests", async (
    CreateRequestDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var currentUser = EndpointHelpers.GetCurrentUser(principal);

    if (currentUser.Role != UserRole.Employee)
    {
        return Results.Forbid();
    }

    if (dto.Items.Count == 0)
    {
        return Results.BadRequest(new { message = "At least one item is required." });
    }

    var vendorExists = await dbContext.Vendors.AnyAsync(x => x.Id == dto.VendorId && x.IsActive, cancellationToken);

    if (!vendorExists)
    {
        return Results.BadRequest(new { message = "Vendor was not found." });
    }

    try
    {
        var request = new ProcurementRequest(currentUser.Id, currentUser.DepartmentId, dto.VendorId);

        foreach (var item in dto.Items)
        {
            request.AddItem(item.Description, item.Quantity, item.UnitCost, item.Category);
        }

        dbContext.ProcurementRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/requests/{request.Id}", new { request.Id, request.RequestNo });
    }
    catch (DomainException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

api.MapPost("/requests/{id:guid}/submit", async (
    Guid id,
    ActionDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    return await EndpointHelpers.UpdateRequestAsync(
        id,
        principal,
        dbContext,
        cancellationToken,
        [UserRole.Employee],
        request => request.RequestedById == EndpointHelpers.GetCurrentUser(principal).Id,
        request => request.Submit(EndpointHelpers.GetCurrentUser(principal).Id, dto.Remarks));
});

api.MapPost("/requests/{id:guid}/approve", async (
    Guid id,
    ActionDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    return await EndpointHelpers.UpdateRequestAsync(
        id,
        principal,
        dbContext,
        cancellationToken,
        [UserRole.Manager],
        _ => true,
        request => request.Approve(EndpointHelpers.GetCurrentUser(principal).Id, dto.Remarks));
});

api.MapPost("/requests/{id:guid}/reject", async (
    Guid id,
    ActionDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    return await EndpointHelpers.UpdateRequestAsync(
        id,
        principal,
        dbContext,
        cancellationToken,
        [UserRole.Manager],
        _ => true,
        request => request.Reject(EndpointHelpers.GetCurrentUser(principal).Id, dto.Remarks ?? string.Empty));
});

api.MapPost("/requests/{id:guid}/order", async (
    Guid id,
    ActionDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    return await EndpointHelpers.UpdateRequestAsync(
        id,
        principal,
        dbContext,
        cancellationToken,
        [UserRole.Finance],
        _ => true,
        request => request.MarkOrdered(EndpointHelpers.GetCurrentUser(principal).Id, dto.Remarks));
});

api.MapPost("/requests/{id:guid}/complete", async (
    Guid id,
    ActionDto dto,
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    return await EndpointHelpers.UpdateRequestAsync(
        id,
        principal,
        dbContext,
        cancellationToken,
        [UserRole.Finance],
        _ => true,
        request => request.Complete(EndpointHelpers.GetCurrentUser(principal).Id, dto.Remarks));
});

api.MapGet("/dashboard/summary", async (
    ClaimsPrincipal principal,
    ProcureFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var currentUser = EndpointHelpers.GetCurrentUser(principal);
    var query = dbContext.ProcurementRequests.AsNoTracking();

    if (currentUser.Role == UserRole.Employee)
    {
        query = query.Where(x => x.RequestedById == currentUser.Id);
    }

    var statusCounts = await query
        .GroupBy(x => x.Status)
        .Select(x => new StatusCountDto(x.Key.ToString(), x.Count()))
        .ToListAsync(cancellationToken);

    var recent = await (
        from request in query
        join vendor in dbContext.Vendors.AsNoTracking() on request.VendorId equals vendor.Id
        orderby request.CreatedAt descending
        select new RequestListItemDto(
            request.Id,
            request.RequestNo,
            string.Empty,
            vendor.Name,
            request.Status.ToString(),
            request.EstimatedTotal,
            request.CreatedAt,
            request.SubmittedAt))
        .Take(5)
        .ToListAsync(cancellationToken);

    return Results.Ok(new DashboardSummaryDto(statusCounts, recent));
});

app.Run();

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, CurrentUserDto User);

public sealed record LookupDto(Guid Id, string Name);

public sealed record CreateRequestDto(Guid VendorId, List<CreateRequestItemDto> Items);

public sealed record CreateRequestItemDto(string Description, int Quantity, decimal UnitCost, string Category);

public sealed record ActionDto(string? Remarks);

public sealed record RequestListItemDto(
    Guid Id,
    string RequestNo,
    string RequestedBy,
    string Vendor,
    string Status,
    decimal EstimatedTotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt);

public sealed record RequestDetailDto(
    Guid Id,
    string RequestNo,
    string RequestedBy,
    string Vendor,
    string Status,
    decimal EstimatedTotal,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt,
    List<RequestItemDto> Items,
    List<ApprovalLogDto> ApprovalLogs);

public sealed record RequestItemDto(
    string Description,
    int Quantity,
    decimal UnitCost,
    string Category,
    decimal LineTotal);

public sealed record ApprovalLogDto(
    string Actor,
    string FromStatus,
    string ToStatus,
    string? Remarks,
    DateTimeOffset CreatedAt);

public sealed record StatusCountDto(string Status, int Count);

public sealed record DashboardSummaryDto(List<StatusCountDto> StatusCounts, List<RequestListItemDto> RecentRequests);

public static class EndpointHelpers
{
    public static CurrentUserDto GetCurrentUser(ClaimsPrincipal principal)
    {
        var id = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var email = principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email) ?? string.Empty;
        var role = Enum.Parse<UserRole>(principal.FindFirstValue(ClaimTypes.Role)!);
        var departmentId = Guid.Parse(principal.FindFirstValue("departmentId")!);

        return new CurrentUserDto(id, name, email, role, departmentId);
    }

    public static async Task<IResult> UpdateRequestAsync(
        Guid id,
        ClaimsPrincipal principal,
        ProcureFlowDbContext dbContext,
        CancellationToken cancellationToken,
        UserRole[] allowedRoles,
        Func<ProcurementRequest, bool> canAccessRequest,
        Action<ProcurementRequest> update)
    {
        var currentUser = GetCurrentUser(principal);

        if (!allowedRoles.Contains(currentUser.Role))
        {
            return Results.Forbid();
        }

        var request = await dbContext.ProcurementRequests
            .Include(x => x.Items)
            .Include(x => x.ApprovalLogs)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return Results.NotFound();
        }

        if (!canAccessRequest(request))
        {
            return Results.Forbid();
        }

        try
        {
            update(request);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { request.Id, status = request.Status.ToString() });
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
