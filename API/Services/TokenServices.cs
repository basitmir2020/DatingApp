using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenServices : ITokenServices
    {
        private readonly SymmetricSecurityKey _securityKey;
        public TokenServices(IConfiguration config) => _securityKey = new
           SymmetricSecurityKey(Encoding.Default.GetBytes(config["TokenKey"]));

        public string CreateToken(AppUser user)
        {
            var claim = new List<Claim> 
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName),
            };

            var cred = new 
            SigningCredentials(
                _securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claim),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = cred
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}