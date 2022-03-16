using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoMinimalAPI.Data
{
    public class MinimalContextDb : DbContext
    {
        public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }

        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Supplier>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Supplier>()
                .Property(t => t.Name)
                .IsRequired()
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Supplier>()
                .Property(t => t.Document)
                .IsRequired()
                .HasColumnType("varchar(14)");

            modelBuilder.Entity<Supplier>()
                .ToTable("Suppliers");

            base.OnModelCreating(modelBuilder);
        }
    }
}
