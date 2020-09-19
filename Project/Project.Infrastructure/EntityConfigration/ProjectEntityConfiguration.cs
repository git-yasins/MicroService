using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfigration {
    public class ProjectEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.Project> {
        public void Configure (EntityTypeBuilder<Domain.AggregatesModel.Project> builder) {
            builder.ToTable ("Projects").HasKey (x => x.Id);
        }
    }
}