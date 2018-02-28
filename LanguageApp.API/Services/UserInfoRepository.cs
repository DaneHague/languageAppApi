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

        public User GetUser(string Email)
        {
            return _context.Users.Where(u => u.Email == Email).FirstOrDefault();
        }
        public IEnumerable<User> GetUsers()
        {
            return _context.Users.OrderBy(u => u.LastName).ToList();
        }

        public bool UserExists(string Email)
        {
            return _context.Users.Any(u => u.Email == Email);
        }

        public void CreateUser(User UserToCreate)
        {
            _context.Users.Add(UserToCreate);
            _context.SaveChanges();
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
