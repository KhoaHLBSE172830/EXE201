using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class AssignmentProgressConfiguration : IEntityTypeConfiguration<AssignmentProgress>
    {
        public void Configure(EntityTypeBuilder<AssignmentProgress> builder)
        {
            builder.ToTable("AssignmentProgress");

            builder.HasKey(ap => ap.Id);

            builder.HasIndex(ap => new { ap.AssignmentId, ap.StudentId }).IsUnique();
            builder.HasIndex(ap => ap.StudentId);

            builder.HasOne(ap => ap.Assignment)
                .WithMany(a => a.AssignmentProgresses)
                .HasForeignKey(ap => ap.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ap => ap.Student)
                .WithMany(u => u.AssignmentProgresses)
                .HasForeignKey(ap => ap.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ap => ap.Session)
                .WithMany()
                .HasForeignKey(ap => ap.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
