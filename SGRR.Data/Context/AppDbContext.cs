using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using SGRR.Data.Entities;

namespace SGRR.Data.Context
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AppDbContext");
        }
        
        #region DbSets

        public DbSet<User> Users { get; set; }

        #endregion
    }
}