using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using JwtWebApiTemplate.Data;
using JwtWebApiTemplate.Models;
using System.Text;
using JwtWebApiTemplate.Services.JWTService;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace JwtWebApiTemplate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;
        private readonly JwtWebApiTemplateContext _context;


        public AuthController(IConfiguration configuration, IUserService userService, IJWTService jwtService, JwtWebApiTemplateContext context)
        {
            _userService = userService;
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserLogin login)
        {
            if (_userService.UserExists(login.UserEmail))
            {
                return BadRequest("User already exists");
            }

            CreatePasswordHash(login.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User();

            user.UserEmail = login.UserEmail;
            user.UserName = login.UserName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok( new {message = "registered" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLogin login)
        {
            User user = _userService.GetUserByEmail(login.UserEmail);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            string token = _jwtService.CreateToken(user);

            var currentRefreshToken = Request.Cookies["refreshToken"];
            //create new refresh token if existing doesn't exists or expired
            if (String.IsNullOrEmpty(currentRefreshToken) || (!String.IsNullOrEmpty(currentRefreshToken) && currentRefreshToken == user.RefreshToken && user.TokenExpires < DateTime.Now))
            {
                var refreshToken = _jwtService.GenerateRefreshToken();
                _jwtService.SetRefreshToken(refreshToken, Response);

                user.RefreshToken = refreshToken.Token;
                user.TokenCreated = refreshToken.Created;
                user.TokenExpires = refreshToken.Expires;

                _context.Entry(user).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }

            return Ok(new { user, token});
        }

        [HttpGet("refresh")]
        public ActionResult Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if(!String.IsNullOrEmpty(refreshToken)) 
            {
                User user = _userService.GetUserByRefreshToken(refreshToken);

                if (user == null)
                {
                    _jwtService.DeleteRefreshToken("refreshToken", Response);
                    return Unauthorized("user not found");
                };

                if (user != null && user.TokenExpires < DateTime.Now) {
                    _jwtService.DeleteRefreshToken("refreshToken", Response);
                    return Unauthorized("refresh token expired");
                };

                string accessToken = _jwtService.CreateToken(user);

                return Ok(new { accessToken });
            }

            return Unauthorized("no refresh token found");
        }


        [HttpPost("logout")]
        public ActionResult Logout()
        {
            _jwtService.DeleteRefreshToken("refreshToken", Response);

            return Ok();
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

    }
}
