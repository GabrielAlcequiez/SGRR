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


            modelBuilder.Entity<Review>()
                .HasOne<Restaurant>()
                .WithMany()
                .HasForeignKey(review => review.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pago>()
.               HasOne(pago => pago.OrdenAnticipada)
.               WithMany()
.               HasForeignKey(pago => pago.OrdenAnticipadaId)
.               OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenAnticipada>()
                .HasMany(orden => orden.Productos)
                .WithOne()
                .HasForeignKey(producto => producto.OrdenAnticipadaId)
                .OnDelete(DeleteBehavior.Cascade);

        }
        
        #region DbSets

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<RestaurantSchedule> RestaurantSchedules { get; set; }

        
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<OrdenAnticipada> OrdenesAnticipadas { get; set; }


        #endregion
        public DbSet<Order> Orders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
