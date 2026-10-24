using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable("Classes");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
            builder.Property(c => c.ClassCode).HasMaxLength(10).IsRequired();
            builder.Property(c => c.Grade).HasMaxLength(50);
            builder.Property(c => c.Room).HasMaxLength(50);
            builder.Property(c => c.FocusTopic).HasMaxLength(150);
            builder.Property(c => c.Status).HasMaxLength(20).IsRequired();

            builder.HasIndex(c => c.ClassCode).IsUnique();
            builder.HasIndex(c => c.OrganizationId);
            builder.HasIndex(c => c.EducatorId);

            builder.HasOne(c => c.Organization)
                .WithMany(o => o.Classes)
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Educator)
                .WithMany(u => u.EducatorClasses)
                .HasForeignKey(c => c.EducatorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
