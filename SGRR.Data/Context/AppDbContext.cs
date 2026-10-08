using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using SGRR.Data.Entities;

namespace SGRR.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
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
        public DbSet<Dish> Dishes { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<RestaurantSchedule> RestaurantSchedules { get; set; }


        #endregion
    }
}
