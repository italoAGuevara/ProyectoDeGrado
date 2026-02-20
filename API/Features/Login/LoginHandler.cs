using API.Exceptions;
using API.Utility;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Login
{
    public class LoginHandler
    {
        private readonly AppDbContext _context;
        public LoginHandler(AppDbContext context) => _context = context;

        public async Task<string> Handle(ValiadateJwtRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync();

            if (user is null)
            {
                throw new InternalServerException("No hay usuario configurado.");
            }

            bool verified = BCrypt.Net.BCrypt.Verify(request.password, user.PasswordHash);

            if (verified)
            {
                return JWTHelpers.GenerateToken("admin");
            }

            throw new UnauthorizedException("Contraseña incorrecta");
        }
    }
}
