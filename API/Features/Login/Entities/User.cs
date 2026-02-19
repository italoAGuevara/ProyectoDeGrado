namespace API.Features.Login.Entities
{
    public class User
    {
        public int Id { get; set; }        
        public string PasswordHash { get; set; }
    }
}
