using LanguageApp.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageApp.API.Services
{
    public interface IUserInfoRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(string Oauth2UserID);

        bool UserExists(string Oauth2UserId);

        void CreateUser(User UserToCreate);

        bool Save();
    }
}
