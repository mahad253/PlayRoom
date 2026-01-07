using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GamingPlatform.Models;

namespace GamingPlatform.Data
{
    public class GamingPlatformContext : DbContext
    {
        public GamingPlatformContext (DbContextOptions<GamingPlatformContext> options)
            : base(options)
        {
        }

        public DbSet<GamingPlatform.Models.User> User { get; set; } = default!;
    }
}
