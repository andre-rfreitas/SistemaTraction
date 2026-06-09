namespace SistemaTraction.Application.Common.Interfaces;

public interface IAuthSettings
{
    string PasswordHash { get; }
    string JwtSecret { get; }
}
