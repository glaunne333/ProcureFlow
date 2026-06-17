namespace ProcureFlow.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "portfolio-demo";

    public string Audience { get; set; } = "portfolio-demo";

    public int ExpirationMinutes { get; set; } = 120;
}
