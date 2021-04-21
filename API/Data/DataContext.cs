using Microsoft.EntityFrameworkCore;
using API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                     .HasMany(ur => ur.UserRoles)
                     .WithOne(u => u.User)
                     .HasForeignKey(fk => fk.UserId)
                     .IsRequired();

            builder.Entity<AppRole>()
                     .HasMany(ur => ur.UserRole)
                     .WithOne(u => u.Role)
                     .HasForeignKey(fk => fk.RoleId)
                     .IsRequired();

            builder.Entity<UserLike>().HasKey(key => new { key.SourceUserId, key.LikedUserId });

            builder.Entity<UserLike>()
                     .HasOne(s => s.SourceUser)
                     .WithMany(l => l.LikedUsers)
                     .HasForeignKey(fk => fk.SourceUserId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                     .HasOne(s => s.LikedUser)
                     .WithMany(l => l.LikedByUsers)
                     .HasForeignKey(fk => fk.LikedUserId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                     .HasOne(u => u.Recipient)
                     .WithMany(m => m.MessagesReceived)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                     .HasOne(u => u.Sender)
                     .WithMany(m => m.MessagesSent)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.ApplyUtcDateTimeConverter();
        }
    }

    /// <summary>
    /// Comes from https://github.com/dotnet/efcore/issues/4711
    /// </summary>
    public static class UtcDateAnnotation
    {
        private const string IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
          new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
          new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
          builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
          ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}