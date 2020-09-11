using Microsoft.EntityFrameworkCore;
using User.API.Models;
namespace User.API.Data {
    public class UserContext : DbContext {
        public UserContext (DbContextOptions<UserContext> options) : base (options) { }
        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            //设置主键
            modelBuilder.Entity<AppUser> ().ToTable ("Users").HasKey (u => u.Id);
            modelBuilder.Entity<UserProperty> ().ToTable ("UserProperties").HasKey (u => new { u.Key, u.AppUserId, u.Value });
            modelBuilder.Entity<UserTag> ().ToTable ("UserTags").HasKey (u => new { u.UserId, u.Tag });
            modelBuilder.Entity<BPFile> ().ToTable ("BPFiles").HasKey (b => b.Id);
            //字段约束
            modelBuilder.Entity<UserTag> ().Property (u => u.Tag).HasMaxLength (100);
            modelBuilder.Entity<UserProperty> ().Property (u => u.Key).HasMaxLength (100);
            modelBuilder.Entity<UserProperty> ().Property (u => u.Value).HasMaxLength (100);

            base.OnModelCreating (modelBuilder);

        }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserProperty> UserPropertys { get; set; }
        public DbSet<UserTag> UserTags { get; set; }
    }
}