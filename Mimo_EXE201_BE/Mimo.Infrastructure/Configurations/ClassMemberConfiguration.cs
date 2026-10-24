using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class ClassMemberConfiguration : IEntityTypeConfiguration<ClassMember>
    {
        public void Configure(EntityTypeBuilder<ClassMember> builder)
        {
            builder.ToTable("ClassMembers");

            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Status).HasMaxLength(20).IsRequired();

            builder.HasIndex(cm => new { cm.ClassId, cm.StudentId }).IsUnique();
            builder.HasIndex(cm => cm.StudentId);

            builder.HasOne(cm => cm.Class)
                .WithMany(c => c.ClassMembers)
                .HasForeignKey(cm => cm.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Student)
                .WithMany(u => u.ClassMembers)
                .HasForeignKey(cm => cm.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
