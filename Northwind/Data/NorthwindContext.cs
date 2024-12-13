using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Northwind.Models;

namespace Northwind.Data
{
    public class NorthwindContext : IdentityDbContext
    {
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }

        public NorthwindContext(DbContextOptions<NorthwindContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().ToTable("Categories", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Supplier>().ToTable("Suppliers", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Product>().ToTable("Products", t => t.ExcludeFromMigrations());
        }
    }
}
