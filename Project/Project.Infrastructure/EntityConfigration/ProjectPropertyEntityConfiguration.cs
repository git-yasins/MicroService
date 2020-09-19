using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;

namespace Project.Infrastructure.EntityConfigration {
    public class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<ProjectProperty> {
        public void Configure (EntityTypeBuilder<ProjectProperty> builder) {
            builder.ToTable ("ProjectPropertys")
                .Property (x => x.Key).HasMaxLength (100);

            builder.ToTable ("ProjectPropertys")
                .Property (x => x.Value).HasMaxLength (100);

            builder.ToTable ("ProjectPropertys")
                .HasKey (x => new { x.ProjectId, x.Key, x.Value });
        }
    }
}