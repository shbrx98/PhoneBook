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

        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<ContactImage> ContactImages => Set<ContactImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureContact(modelBuilder);
            ConfigureContactImage(modelBuilder);
        }

        private void ConfigureContact(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>(entity =>
            {
                // Table Name
                entity.ToTable("Contacts");

                // Primary Key
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                // FullName
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                // MobileNumber
                entity.Property(e => e.MobileNumber)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .HasColumnType("varchar(11)");

                // Unique Index for MobileNumber
                entity.HasIndex(e => e.MobileNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Contacts_MobileNumber_Unique");

                // BirthDate (nullable)
                entity.Property(e => e.BirthDate)
                    .HasColumnType("date");

                // CreatedAt
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()")
                    .HasColumnType("datetime2");

                // UpdatedAt
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2");

                // Relationship: Contact -> ContactImage (One-to-One)
                entity.HasOne(e => e.ContactImage)
                    .WithOne(i => i.Contact)
                    .HasForeignKey<ContactImage>(i => i.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureContactImage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactImage>(entity =>
            {
                // Table Name
                entity.ToTable("ContactImages");

                // Primary Key
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                // ContactId (Foreign Key)
                entity.Property(e => e.ContactId)
                    .IsRequired();

                // Unique Index for ContactId
                entity.HasIndex(e => e.ContactId)
                    .IsUnique()
                    .HasDatabaseName("IX_ContactImages_ContactId_Unique");

                // ImageData
                entity.Property(e => e.ImageData)
                    .IsRequired()
                    .HasColumnType("varbinary(max)");

                // FileName
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("nvarchar(255)");

                // ContentType
                entity.Property(e => e.ContentType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnType("varchar(50)");

                // FileSize
                entity.Property(e => e.FileSize)
                    .IsRequired();

                // CreatedAt
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()")
                    .HasColumnType("datetime2");

                // UpdatedAt
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime2");
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically set UpdatedAt
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set UpdatedAt
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update CreatedAt and UpdatedAt timestamps
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    var createdAtValue = entry.Property("CreatedAt").CurrentValue as DateTime?;
                    if (createdAtValue == null || createdAtValue.Value == default)
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}