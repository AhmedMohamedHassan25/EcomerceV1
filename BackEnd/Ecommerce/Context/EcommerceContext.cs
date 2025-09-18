using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Context
{
    public class ECommerceContext : DbContext
    {
        public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(user =>
            {
                user.HasIndex(u=>u.UserName).IsUnique();
                user.HasIndex(u=>u.Email).IsUnique();
                user.Property(u => u.Password)
                   .HasMaxLength(255);

            });
            modelBuilder.Entity<Product>(product =>
            {
                product.HasIndex(p => p.ProductCode).IsUnique();


            });

        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateDate();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateDate();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        private void UpdateDate()
        {
            var entities = ChangeTracker.Entries<BaseEntity<int>>();
            foreach (var entity in entities)
            {
                if
                (entity.State == EntityState.Added)
                {
                    entity.Entity.CreatedAt = DateTime.Now;
                }
                else if (entity.State == EntityState.Modified)
                {
                    entity.Entity.UpdatedAt = DateTime.Now;
                }
            }
        }
    }
 
}
