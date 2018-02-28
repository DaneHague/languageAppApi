using AutoMapper;
using AutoMapper.Configuration;
using LanguageApp.API.Entities;
using LanguageApp.API.Models;
using LanguageApp.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace LanguageApp.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {

        private const string AuthSchemes =
       GoogleDefaults.AuthenticationScheme + "," +
       FacebookDefaults.AuthenticationScheme;

        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IUserInfoRepository _userInfoRepository;

        public UsersController(IUserInfoRepository userInfoRepository,
            Microsoft.Extensions.Configuration.IConfiguration configuration
            )
        {
            _userInfoRepository = userInfoRepository;
            _configuration = configuration;
        }
        
        [HttpGet()]
        public IActionResult GetUsers()
        {
            var userEntities = _userInfoRepository.GetUsers();

            var results = Mapper.Map<IEnumerable<UserDto>>(userEntities);

            return Ok(results);
        }
        
        [HttpGet("login/google")]
        public IActionResult LoginOauth2()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = "http://localhost:58411/api/users/oauth2connect"
            };

            return(Challenge(authenticationProperties, "Google"));

        }

        [HttpPost("profile/test")]
        public JsonResult UpdateProfile([FromBody] User info)
        {
            var updatedUserProfile = Mapper.Map<UserProfileDto>(info);
            return Json(updatedUserProfile);
        }

        [HttpGet("login/facebook")]
        public IActionResult LoginFacebook()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = "http://localhost:58411/api/users/oauth2connect"
            };

            return (Challenge(authenticationProperties, "Facebook"));
        }

        [Authorize(Roles = "User")]
        [HttpGet("profile")]
        public JsonResult GetUserProfile()
        {
            var email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var user = _userInfoRepository.GetUser(User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value);
            var result = Mapper.Map<UserDto>(user);
            return Json(result);
        }



        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [HttpGet("oauth2connect")]
        public object Oauth2ConnectUser()
        {
            var userEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;

            if (_userInfoRepository.UserExists(userEmail))
            {
                User UserInfo = new User
                {
                    FirstName = User.FindFirst(c => c.Type == ClaimTypes.GivenName)?.Value,
                    Email = userEmail
                };
                var generateToken = GenerateJwtToken(UserInfo.Email, UserInfo);
                return Redirect($"http://localhost:3000?token={generateToken}");
            }

            var user = new User
            {
                FirstName = User.FindFirst(c => c.Type == ClaimTypes.GivenName)?.Value,
                Email = userEmail
            };

            _userInfoRepository.CreateUser(user);

            var token = GenerateJwtToken(user.Email, user);
            return Redirect($"http://localhost:3000?token={token}");

        }

        private object GenerateJwtToken(string email, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Email, User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value)

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
