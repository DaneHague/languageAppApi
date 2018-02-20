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

namespace LanguageApp.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IUserInfoRepository _userInfoRepository;

        public UsersController(IUserInfoRepository userInfoRepository,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            Microsoft.Extensions.Configuration.IConfiguration configuration
            )
        {
            _userInfoRepository = userInfoRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet()]
        public IActionResult GetUsers()
        {
            var userEntities = _userInfoRepository.GetUsers();

            var results = Mapper.Map<IEnumerable<UserDto>>(userEntities);

            return Ok(results);
        }
        
        [HttpGet("{Oauth2UserId}")]
        public IActionResult GetUser(string Oauth2UserId)
        {
            var userToReturn = _userInfoRepository.GetUser(Oauth2UserId);

            if(userToReturn == null)
            {
                return NotFound();
            }

            return Ok(userToReturn);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult test()
        {
            return Ok();
        }

        [HttpPost("oauth2connect")]
        public async Task<object> Oauth2ConnectUserAsync([FromBody] User Oauth2User)
        {
            //var data = "";
            //string url = $"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={Oauth2User.AccessToken}";

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //using (Stream stream = response.GetResponseStream())
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    data = reader.ReadToEnd();
            //}

            //dynamic jObj = JsonConvert.DeserializeObject(data);

            //var issued_to = jObj.issued_to;

            //if (_userInfoRepository.UserExists(Oauth2User.Oauth2Id))
            //{
            //    var UserInfo = Mapper.Map<UserDto>(_userInfoRepository.GetUser(Oauth2User.Oauth2Id));
            //    return Ok(UserInfo);
            //}

            var user = new IdentityUser
            {
                UserName = Oauth2User.FirstName,
                Email = Oauth2User.Email
            };

            var result = await _userManager.CreateAsync(user, "Test123@");

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return GenerateJwtToken(Oauth2User.Email, user);
            }

            //_userInfoRepository.CreateUser(Oauth2User);

            //if (!_userInfoRepository.Save())
            //{
            //    return StatusCode(500, "A problem happened when handling your request");
            //}
            //var createdUser = Mapper.Map<Models.UserDto>(Oauth2User);

            return Ok();
        }

        private object GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
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
