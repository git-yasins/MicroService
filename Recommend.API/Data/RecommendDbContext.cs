using Microsoft.EntityFrameworkCore;
using Recommend.API.Models;

namespace Recommend.API.Data {
    public class RecommendDbContext : DbContext {
        public RecommendDbContext (DbContextOptions<RecommendDbContext> options) : base (options) { }
        protected override void OnModelCreating (ModelBuilder modelBuilder) {
            modelBuilder.Entity<ProjectRecommend> ().ToTable ("ProjectRecommends").HasKey (u => u.Id);
            base.OnModelCreating (modelBuilder);
        }

        public DbSet<ProjectRecommend> Recommends { get; set; }
    }
}