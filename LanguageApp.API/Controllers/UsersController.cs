using AutoMapper;
using LanguageApp.API.Entities;
using LanguageApp.API.Models;
using LanguageApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LanguageApp.API.Controllers
{
    [Route("api/users")]
    public class UsersController : Controller
    {
        private IUserInfoRepository _userInfoRepository;

        public UsersController(IUserInfoRepository userInfoRepository)
        {
            _userInfoRepository = userInfoRepository;
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

        [HttpPost("oauth2connect")]
        public IActionResult Oauth2ConnectUser([FromBody] User Oauth2User)
        {
            var data = "";
            string url = $"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={Oauth2User.AccessToken}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }

            dynamic jObj = JsonConvert.DeserializeObject(data);

            var issued_to = jObj.issued_to;

            if (_userInfoRepository.UserExists(Oauth2User.Oauth2Id))
            {
                var UserInfo = Mapper.Map<UserDto>(_userInfoRepository.GetUser(Oauth2User.Oauth2Id));
                return Ok(UserInfo);
            }

            _userInfoRepository.CreateUser(Oauth2User);

            if (!_userInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened when handling your request");
            }
            var createdUser = Mapper.Map<Models.UserDto>(Oauth2User);

            return Ok();
        }
    }
}
