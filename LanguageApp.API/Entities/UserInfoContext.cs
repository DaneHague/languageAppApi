using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageApp.API.Entities
{
    public class UserInfoContext : IdentityDbContext<User>
    {
        public UserInfoContext(DbContextOptions<UserInfoContext> options)
            : base(options)
        {

        }
    }
}
