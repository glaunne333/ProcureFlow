using Microsoft.EntityFrameworkCore;
using ProcureFlow.Application.Auth;
using ProcureFlow.Domain;

namespace ProcureFlow.Infrastructure.Persistence;

public sealed class DemoDataSeeder(ProcureFlowDbContext dbContext, IPasswordHasher passwordHasher)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var financeDepartment = await EnsureDepartmentAsync("Finance", cancellationToken);
        var operationsDepartment = await EnsureDepartmentAsync("Operations", cancellationToken);
        var itDepartment = await EnsureDepartmentAsync("IT", cancellationToken);

        await EnsureVendorAsync("Acme Office Supplies", "sales@acme-demo.test", cancellationToken);
        await EnsureVendorAsync("Northwind Hardware", "orders@northwind-demo.test", cancellationToken);
        await EnsureVendorAsync("Contoso IT Services", "hello@contoso-demo.test", cancellationToken);

        await EnsureUserAsync(
            "Employee Demo",
            "employee@demo.com",
            "employee",
            UserRole.Employee,
            operationsDepartment.Id,
            cancellationToken);

        await EnsureUserAsync(
            "Manager Demo",
            "manager@demo.com",
            "manager",
            UserRole.Manager,
            operationsDepartment.Id,
            cancellationToken);

        await EnsureUserAsync(
            "Finance Demo",
            "finance@demo.com",
            "finance",
            UserRole.Finance,
            financeDepartment.Id,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Department> EnsureDepartmentAsync(string name, CancellationToken cancellationToken)
    {
        var department = await dbContext.Departments.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        if (department is not null)
        {
            return department;
        }

        department = new Department(name);
        dbContext.Departments.Add(department);
        return department;
    }

    private async Task EnsureVendorAsync(string name, string contactEmail, CancellationToken cancellationToken)
    {
        if (await dbContext.Vendors.AnyAsync(x => x.Name == name, cancellationToken))
        {
            return;
        }

        dbContext.Vendors.Add(new Vendor(name, contactEmail));
    }

    private async Task EnsureUserAsync(
        string name,
        string email,
        string password,
        UserRole role,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return;
        }

        dbContext.Users.Add(new User(name, email, passwordHasher.Hash(password), role, departmentId));
    }
}
