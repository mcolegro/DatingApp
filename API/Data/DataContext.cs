using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Data
{
   public class DataContext : DbContext
   {
      public DataContext(DbContextOptions options) : base(options) { }

      public DbSet<AppUser> Users { get; set; }
      public DbSet<UserLike> Likes { get; set; }
      protected override void OnModelCreating(ModelBuilder builder){
         base.OnModelCreating(builder);

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
      }
   }
}