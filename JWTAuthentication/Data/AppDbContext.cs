using JWTAuthentication.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JWTAuthentication.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public virtual DbSet<Todo> Todos { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }
    }
}
