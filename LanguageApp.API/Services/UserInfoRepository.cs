using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageApp.API.Entities;

namespace LanguageApp.API.Services
{
    public class UserInfoRepository : IUserInfoRepository
    {
        private UserInfoContext _context;

        public UserInfoRepository(UserInfoContext context)
        {
            _context = context;
        }
        public User GetUser(string Oauth2UserId)
        {
            return _context.Users.Where(u => u.Oauth2Id == Oauth2UserId).FirstOrDefault();
        }

        public IEnumerable<User> GetUsers()
        {
            return _context.Users.OrderBy(u => u.LastName).ToList();
        }

        public bool UserExists(string Oauth2UserId)
        {
            return _context.Users.Any(u => u.Oauth2Id == Oauth2UserId);
        }

        public void CreateUser(User UserToCreate)
        {
            _context.Users.Add(UserToCreate);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
