using LanguageApp.API.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageApp.API.Services
{
    public interface IUserInfoRepository
    {

        User GetUser(string Email);
        IEnumerable<User> GetUsers();

        bool UserExists(string Email);

        void CreateUser(User UserToCreate);

        bool Save();
    }
}
