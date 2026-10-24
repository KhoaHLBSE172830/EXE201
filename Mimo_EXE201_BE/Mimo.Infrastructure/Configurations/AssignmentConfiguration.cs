using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.ToTable("Assignments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title).HasMaxLength(150).IsRequired();
            builder.Property(a => a.Description).HasColumnType("text");

            builder.HasIndex(a => new { a.ClassId, a.Deadline });
            builder.HasIndex(a => a.NodeId);

            builder.HasOne(a => a.Class)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Node)
                .WithMany(n => n.Assignments)
                .HasForeignKey(a => a.NodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
