using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JWT_token_api.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JWT_token_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        private readonly IConfiguration _config;
        private List<User> appUsers = new List<User>
        {
            new User { FullName = "John Dow", UserName = "admin", Password = "1234", UserRole = "Admin" },
            new User { FullName = "Test User", UserName = "admin", Password = "1234", UserRole = "User" },
        };

        public SignInController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] User login)
        {
            IActionResult response = Unauthorized();
            User user = AuthenticateUser(login);
            if (user != null)
            {
                var tokenString = GenerateJWTToken(user);
                response = Ok(new
                {
                    token = tokenString,
                    userDetails = new
                    {
                        userName = user.UserName,
                        fullName = user.FullName,
                        userRole = user.UserRole
                    },
                });
            }
            return response;
        }
        User AuthenticateUser(User loginCredentials)
        {
            User user = appUsers.FirstOrDefault(x => x.UserName == loginCredentials.UserName && x.Password == loginCredentials.Password);
            return user;
        }
        string GenerateJWTToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim("fullName", userInfo.FullName.ToString()),
                new Claim("role",userInfo.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             };

            var token = new JwtSecurityToken(
            issuer: _config["Jwt: Issuer"],
            audience: _config["Jwt: Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
