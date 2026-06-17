using ProcureFlow.Infrastructure.Security;

namespace ProcureFlow.Tests;

public class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Verify_accepts_the_original_password_because_demo_login_uses_stored_hashes()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var passwordHash = hasher.Hash("employee");

        Assert.True(hasher.Verify("employee", passwordHash));
        Assert.False(hasher.Verify("manager", passwordHash));
    }
}
