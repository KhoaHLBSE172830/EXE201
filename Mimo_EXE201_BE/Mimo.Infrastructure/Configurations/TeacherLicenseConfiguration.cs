using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class TeacherLicenseConfiguration : IEntityTypeConfiguration<TeacherLicense>
    {
        public void Configure(EntityTypeBuilder<TeacherLicense> builder)
        {
            builder.ToTable("TeacherLicenses");

            builder.HasKey(tl => tl.Id);

            builder.Property(tl => tl.ActivationCode).HasMaxLength(32).IsRequired();
            builder.Property(tl => tl.Note).HasMaxLength(150);
            builder.Property(tl => tl.AssignedClassesJson).HasColumnType("jsonb").IsRequired();

            builder.HasIndex(tl => tl.ActivationCode).IsUnique();
            builder.HasIndex(tl => tl.OrganizationId);

            builder.HasOne(tl => tl.Organization)
                .WithMany(o => o.TeacherLicenses)
                .HasForeignKey(tl => tl.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tl => tl.ClaimedByEducator)
                .WithMany(u => u.ClaimedTeacherLicenses)
                .HasForeignKey(tl => tl.ClaimedByEducatorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
