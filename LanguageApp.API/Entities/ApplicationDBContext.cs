using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LanguageApp.API.Entities
{
    public class ApplicationDBContext : IdentityDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Startup.Configuration["ConnectionString:LanguageAppDB"];
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<User> Users { get; set; }
    }
}
