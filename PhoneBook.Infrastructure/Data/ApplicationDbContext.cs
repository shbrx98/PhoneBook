using Microsoft.EntityFrameworkCore;
using PhoneBook.Domain.Entities;

namespace PhoneBook.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactImage> ContactImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Contact Configuration
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MobileNumber).IsRequired().HasMaxLength(11);
                entity.HasIndex(e => e.MobileNumber).IsUnique();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(e => e.ContactImage)
                    .WithOne(e => e.Contact)
                    .HasForeignKey<ContactImage>(e => e.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ContactImage Configuration
            modelBuilder.Entity<ContactImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageData).IsRequired();
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.ContactId).IsUnique();
            });
        }
    }
}
