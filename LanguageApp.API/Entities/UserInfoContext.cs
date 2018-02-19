using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageApp.API.Entities
{
    public class UserInfoContext : DbContext
    {
        public UserInfoContext(DbContextOptions<UserInfoContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
