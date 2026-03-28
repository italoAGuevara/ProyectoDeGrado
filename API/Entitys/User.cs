namespace API.Entitys
{
    public class User
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        /// <summary>Si true, la aplicación exige contraseña para entrar.</summary>
        public bool RequirePassword { get; set; } = true;
    }
}
