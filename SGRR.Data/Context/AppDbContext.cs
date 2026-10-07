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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(user => user.Restaurant)
                .WithOne()
                .HasForeignKey<User>("RestaurantId");
        }
        
        #region DbSets

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }

        #endregion
    }
}
