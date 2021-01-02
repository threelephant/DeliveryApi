using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Installers;
using Delivery.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Delivery.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly deliveryContext db;
        public AccountController(deliveryContext db)
        {
            this.db = db;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == username);
            if (user == null)
            {
                return BadRequest(new { error = "Некорректные логин и/или пароль" });
            }

            var salt = user.Salt;
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            var loggedUser = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == username && u.Password == hashedPassword);

            if (loggedUser == null)
            {
                return BadRequest(new {error = "Некорректные логин и/или пароль"});
            }
            
            var token = GetToken(loggedUser);

            return Ok(new { username = username, token = token });
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                return BadRequest(new { error = "Пароли не совпадают" });
            }
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == username);
            if (user != null)
            {
                return BadRequest(new { error = "Пользователь с таким логином уже существует" });
            }

            var salt = Guid.NewGuid();

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            
            var newUser = new User
            {
                Login = username,
                FirstName = "",
                LastName = "",
                Role = db.Roles.FirstOrDefault(r => r.Name == "user"),
                Password = hashedPassword,
                Salt = salt,
            };

            await db.Users.AddAsync(newUser);
            await db.SaveChangesAsync();

            var token = GetToken(newUser);

            return Ok(new { username, token });
        }

        private string GetToken(User user)
        {
            var identity = GetIdentity(user);
            var now = DateTime.Now;
            
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: identity.Claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private static ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name),
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, "Token",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }
    }
}