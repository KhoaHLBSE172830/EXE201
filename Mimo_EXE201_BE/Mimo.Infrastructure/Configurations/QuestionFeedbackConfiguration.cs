using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Configurations
{
    public class QuestionFeedbackConfiguration : IEntityTypeConfiguration<QuestionFeedback>
    {
        public void Configure(EntityTypeBuilder<QuestionFeedback> builder)
        {
            builder.ToTable("QuestionFeedbacks");

            builder.HasKey(qf => qf.Id);

            builder.Property(qf => qf.ReviewerName).HasMaxLength(100).IsRequired();
            builder.Property(qf => qf.Kind).HasMaxLength(20).IsRequired();
            builder.Property(qf => qf.Note).HasColumnType("text").IsRequired();
            builder.Property(qf => qf.Status).HasMaxLength(20).IsRequired();

            builder.HasIndex(qf => qf.QuestionId);

            builder.HasOne(qf => qf.Question)
                .WithMany(q => q.QuestionFeedbacks)
                .HasForeignKey(qf => qf.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qf => qf.Reviewer)
                .WithMany()
                .HasForeignKey(qf => qf.ReviewerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
