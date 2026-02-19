using API.Exceptions;
using API.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace API.Features.Login
{
    public class IsTokenValidHandler
    {
        private readonly AppDbContext _context;
        public IsTokenValidHandler(AppDbContext context) => _context = context;

        public async Task<bool> Handle(string token)
        {            
             return IsTokenValid(token, "Tu_Llave_Super_Secreta_De_Al_Menos_32_Chars!");
        }

        private bool IsTokenValid(string token, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Ajustar según necesidad
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false; // El token falló alguna validación
            }
        }
    }
}
