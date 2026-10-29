using CarsInfo.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CarsInfo.Api.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public partial class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = ValidateUserCredentials(authenticationRequestBody.Username, authenticationRequestBody.Password);

            if (user == null)
            {
                return Unauthorized();
            }
            var secretForKey = _configuration["Authentication:SecretForKey"];
            if (string.IsNullOrWhiteSpace(secretForKey))
            {
                throw new InvalidOperationException("Missing configuration value 'Authentication:SecretForKey'.");
            }
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(secretForKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("car", user.Car));

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials
                );

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            
            return Ok(tokenToReturn);

        }

        private CarInfoUser ValidateUserCredentials(string? username, string? password)
        {
            // Demo only: every username/password is accepted and gets access to "Car 1".
            // Replace with a real user store (hashed passwords) before production use.
            return new CarInfoUser(
                1,
                username ?? "",
                "Demo",
                "User",
                "Car 1"
            );
        }
    }
}
