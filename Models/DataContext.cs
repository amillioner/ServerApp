using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ServerApp.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opts)
            : base(opts) { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasMany(p => p.Ratings)
                .WithOne(r => r.Product).OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>().HasOne<Supplier>(s => s.Supplier)
                .WithMany(p => p.Products).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
