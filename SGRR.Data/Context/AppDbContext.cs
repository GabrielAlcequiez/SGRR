using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace SGRR.Data.Context
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AppDbContext");

            #region DbSets

            #endregion
        }
    }
}