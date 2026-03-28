namespace API.Services.Interfaces
{
    public interface ILogin
    {
        Task<string> LoginUser(string password);
        Task<bool> isTokenValid(string token);
        Task<bool> ChangePassword(string previouPassword,string newPassword);
    }
}
