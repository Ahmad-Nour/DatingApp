using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Dtos;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _config = config;
            _authRepository = authRepository;
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            if (!ModelState.IsValid) return NotFound();
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            var userExist = await _authRepository.UserExists(userForRegisterDto.Username);
            if (userExist) return BadRequest("Username is Exist!");

            var user = new User
            {
                Username = userForRegisterDto.Username
            };

            var registerUser = await _authRepository.Register(user, userForRegisterDto.Password);
            if (registerUser == null)
                return BadRequest();
            return Ok();
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login(UserForLoginDto userForLoginDto)
        {
            if (!ModelState.IsValid) return NotFound();
            var user = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (user == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier , user.Id.ToString()),
                new Claim(ClaimTypes.Name , user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSetting:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescripter = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescripter);
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }

        [HttpGet("UsernameExist")]
        public async Task<ActionResult<bool>> UsernameExist(string username)
        {
            var userExist = await _authRepository.UserExists(username);
            if (userExist) return true;
            return false;
        }

    }
}