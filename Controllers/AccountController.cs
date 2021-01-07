using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Delivery.Contracts.Account;
using Delivery.Installers;
using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Delivery.Controllers
{
    /// <summary>
    /// Запросы, связанные с авторизацией
    /// </summary>
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly deliveryContext db;
        public AccountController(deliveryContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///
        ///     POST /api/account/login
        ///     {
        ///         "username": "username",
        ///         "password": "password"
        ///     }
        /// </remarks>
        /// <param name="loginModel">Параметры авторизации</param>
        /// <response code="200">Авторизация успешна прошла</response>
        /// <response code="400">Некорректные логин и/или пароль</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == loginModel.username);
            if (user == null)
            {
                return BadRequest(new { error = "Некорректные логин и/или пароль" });
            }

            var salt = user.Salt;
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: loginModel.password,
                salt: salt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            var loggedUser = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == loginModel.username && u.Password == hashedPassword);

            if (loggedUser == null)
            {
                return BadRequest(new {error = "Некорректные логин и/или пароль"});
            }
            
            var token = GetToken(loggedUser);

            return Ok(new { loginModel.username, token });
        }

        /// <summary>
        /// Регистрация
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///
        ///     POST /api/account/register
        ///     {
        ///         "username": "username",
        ///         "password": "password",
        ///         "confirm_password": "password"
        ///     }
        /// </remarks>
        /// <param name="registerModel">Параметры регистрации</param>
        /// <response code="200">Успешная регистрация</response>
        /// <response code="400">Пароли не совпадают и/или пользователь с таким логином уже существует</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == registerModel.username);
            if (user != null)
            {
                return BadRequest(new { error = "Пользователь с таким логином уже существует" });
            }

            var salt = Guid.NewGuid();

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: registerModel.password,
                salt: salt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            
            var newUser = new User
            {
                Login = registerModel.username,
                FirstName = "",
                LastName = "",
                Role = db.Roles.FirstOrDefault(r => r.Name == "user"),
                Password = hashedPassword,
                Salt = salt,
            };

            await db.Users.AddAsync(newUser);
            await db.SaveChangesAsync();

            var token = GetToken(newUser);

            return Ok(new { registerModel.username, token });
        }

        /// <summary>
        /// Сброс пароля
        /// </summary>
        /// <remarks>
        ///     Sample request:
        ///
        ///     POST /api/account/reset
        ///     {
        ///         "username": "username",
        ///         "old_password": "password",
        ///         "new_password": "new_password",
        ///         "confirm_new_password": "new_password"
        ///     }
        /// </remarks>
        /// <param name="resetModel">Параметры сброса</param>
        /// <response code="200">Успешный сброс</response>
        /// <response code="400">Пароли не совпадают или некорректные логин и/или пароль</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Не является пользователем с введённым логином</response>
        [Authorize]
        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromBody] ResetModel resetModel)
        {
            if (resetModel.username != User.Identity?.Name)
            {
                return Forbid();
            }
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.Login == resetModel.username);
            if (user == null)
            {
                return BadRequest(new { error = "Некорректные логин и/или пароль" });
            }
            
            var salt = user.Salt;
            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: resetModel.old_password,
                salt: salt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            
            var loggedUser = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == resetModel.username && u.Password == hashedPassword);
            
            if (loggedUser == null)
            {
                return BadRequest(new {error = "Некорректные логин и/или пароль"});
            }
            
            var newSalt = Guid.NewGuid();
            var newHashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: resetModel.new_password,
                salt: newSalt.ToByteArray(),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            loggedUser.Salt = newSalt;
            loggedUser.Password = newHashedPassword;

            await db.SaveChangesAsync();
            return Ok();
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