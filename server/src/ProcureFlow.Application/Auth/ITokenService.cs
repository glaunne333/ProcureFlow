namespace ProcureFlow.Application.Auth;

public interface ITokenService
{
    string CreateToken(CurrentUserDto user);
}
