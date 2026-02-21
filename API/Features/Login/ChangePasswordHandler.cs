using API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Login
{
    public class ChangePasswordHandler
    {
        private readonly AppDbContext _context;

        public ChangePasswordHandler(AppDbContext context) => _context = context;

        public async Task Handle(ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync();
            if (user is null)
                throw new InternalServerException("No hay usuario configurado.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedException("La contraseña actual no es correcta.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
        }
    }
}
