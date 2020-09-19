using System;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.AggregatesModel;

namespace Project.Infrastructure.EntityConfigration {
    public class ProjectVisibleRuleEntityConfiguration : IEntityTypeConfiguration<ProjectVisibleRule> {
        public void Configure (EntityTypeBuilder<ProjectVisibleRule> builder) {
            builder.ToTable ("ProjectVisibleRules").HasKey (x => x.Id);
        }
    }
}