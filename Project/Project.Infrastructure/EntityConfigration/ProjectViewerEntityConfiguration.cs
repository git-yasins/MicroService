using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;

namespace Project.Infrastructure.EntityConfigration
{
    public class ProjectViewerEntityConfiguration : IEntityTypeConfiguration<ProjectViewer>
    {
        public void Configure(EntityTypeBuilder<ProjectViewer> builder)
        {
           builder.ToTable ("ProjectViewers").HasKey (x => x.Id);
        }
    }
}