using Microsoft.EntityFrameworkCore;
using Northwind.Models;

namespace Northwind.Data
{
    public class NorthwindContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }

        public NorthwindContext(DbContextOptions<NorthwindContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
    }
}
