using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mimo.DomainLayer.Entities;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Infrastructure.Database;

public class MimoDbContext : DbContext
{
    public MimoDbContext(DbContextOptions<MimoDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Module> Modules { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<LessonProgress> LessonProgresses { get; set; } = null!;
    public DbSet<Certificate> Certificates { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<UserStats> UserStats { get; set; } = null!;

    // Gamification & Gameplay Engine (AGENT.md Section 4.2 & 4.3)
    public DbSet<Domain> Domains { get; set; } = null!;
    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Node> Nodes { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<NodeQuestion> NodeQuestions { get; set; } = null!;
    public DbSet<UserNodeProgress> UserNodeProgresses { get; set; } = null!;
    public DbSet<LessonSession> LessonSessions { get; set; } = null!;
    public DbSet<SessionAnswer> SessionAnswers { get; set; } = null!;
    public DbSet<WrongQuestion> WrongQuestions { get; set; } = null!;
    public DbSet<XpEvent> XpEvents { get; set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt ??= DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MimoDbContext).Assembly);

        // 1. Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.RoleName).IsUnique();
            entity.Property(r => r.Status).HasConversion<string>();
        });

        // Seed default roles
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var studentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var curatorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var orgRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var educatorRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, RoleName = "Admin", Description = "Quản trị viên toàn hệ thống", Status = RoleStatusEnum.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = studentRoleId, RoleName = "Student", Description = "Học sinh / Người học kỹ năng", Status = RoleStatusEnum.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = curatorRoleId, RoleName = "Curator", Description = "Giảng viên / Biên tập kịch bản bài giảng", Status = RoleStatusEnum.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = orgRoleId, RoleName = "Organization", Description = "Đại diện tổ chức / Trường học B2B", Status = RoleStatusEnum.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = educatorRoleId, RoleName = "Educator", Description = "Giáo viên / Giảng viên thuộc trường học / tổ chức", Status = RoleStatusEnum.Active, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // 2. Organizations
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(o => o.Code).IsUnique();
            entity.Property(o => o.Status).HasConversion<string>();
            entity.Property(o => o.SubscriptionTier).HasConversion<string>();
        });

        // 3. Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.Property(u => u.Status).HasConversion<string>();

            // Enforce strictly ONE Admin user across the entire database (Partial Unique Index)
            entity.HasIndex(u => u.RoleId)
                  .IsUnique()
                  .HasFilter($"\"RoleId\" = '{adminRoleId}'");

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Organization)
                  .WithMany(o => o.Users)
                  .HasForeignKey(u => u.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Profile)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Stats)
                  .WithOne(s => s.User)
                  .HasForeignKey<UserStats>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Courses
        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(c => c.DifficultyLevel).HasConversion<string>();
            entity.Property(c => c.Status).HasConversion<string>();

            entity.HasOne(c => c.Instructor)
                  .WithMany(u => u.InstructedCourses)
                  .HasForeignKey(c => c.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. Modules
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasOne(m => m.Course)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(m => m.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Lessons
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.Property(l => l.ContentType).HasConversion<string>();

            entity.HasOne(l => l.Module)
                  .WithMany(m => m.Lessons)
                  .HasForeignKey(l => l.ModuleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. Enrollments
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. LessonProgresses
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.Property(lp => lp.Status).HasConversion<string>();

            entity.HasIndex(lp => new { lp.EnrollmentId, lp.LessonId }).IsUnique();

            entity.HasOne(lp => lp.Enrollment)
                  .WithMany(e => e.LessonProgresses)
                  .HasForeignKey(lp => lp.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lp => lp.Lesson)
                  .WithMany(l => l.LessonProgresses)
                  .HasForeignKey(lp => lp.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 9. Certificates
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasIndex(c => c.EnrollmentId).IsUnique();

            entity.HasOne(c => c.Enrollment)
                  .WithOne(e => e.Certificate)
                  .HasForeignKey<Certificate>(c => c.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 10. Reviews
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.CourseId }).IsUnique();

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Course)
                  .WithMany(c => c.Reviews)
                  .HasForeignKey(r => r.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 11. Transactions
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.PaymentMethod).HasConversion<string>();
            entity.Property(t => t.Status).HasConversion<string>();

            entity.HasOne(t => t.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Course)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 12. Domains
        modelBuilder.Entity<Domain>(entity =>
        {
            entity.HasIndex(d => d.Slug).IsUnique();
        });

        // 13. Units
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasOne(u => u.Domain)
                  .WithMany(d => d.Units)
                  .HasForeignKey(u => u.DomainId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 14. Nodes
        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasOne(n => n.Unit)
                  .WithMany(u => u.Nodes)
                  .HasForeignKey(n => n.UnitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 15. Questions
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasOne(q => q.Curator)
                  .WithMany()
                  .HasForeignKey(q => q.CuratorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 16. NodeQuestions (Composite Key)
        modelBuilder.Entity<NodeQuestion>(entity =>
        {
            entity.HasKey(nq => new { nq.NodeId, nq.QuestionId });

            entity.HasOne(nq => nq.Node)
                  .WithMany(n => n.NodeQuestions)
                  .HasForeignKey(nq => nq.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(nq => nq.Question)
                  .WithMany(q => q.NodeQuestions)
                  .HasForeignKey(nq => nq.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 17. UserNodeProgresses
        modelBuilder.Entity<UserNodeProgress>(entity =>
        {
            entity.HasIndex(unp => new { unp.UserId, unp.NodeId }).IsUnique();

            entity.HasOne(unp => unp.User)
                  .WithMany(u => u.UserNodeProgresses)
                  .HasForeignKey(unp => unp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(unp => unp.Node)
                  .WithMany(n => n.UserNodeProgresses)
                  .HasForeignKey(unp => unp.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 18. LessonSessions
        modelBuilder.Entity<LessonSession>(entity =>
        {
            entity.HasOne(ls => ls.User)
                  .WithMany(u => u.LessonSessions)
                  .HasForeignKey(ls => ls.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ls => ls.Node)
                  .WithMany(n => n.LessonSessions)
                  .HasForeignKey(ls => ls.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 19. SessionAnswers
        modelBuilder.Entity<SessionAnswer>(entity =>
        {
            entity.HasIndex(sa => new { sa.SessionId, sa.QuestionId }).IsUnique();

            entity.HasOne(sa => sa.Session)
                  .WithMany(s => s.Answers)
                  .HasForeignKey(sa => sa.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.Question)
                  .WithMany(q => q.SessionAnswers)
                  .HasForeignKey(sa => sa.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 20. WrongQuestions
        modelBuilder.Entity<WrongQuestion>(entity =>
        {
            entity.HasIndex(wq => new { wq.UserId, wq.QuestionId }).IsUnique();

            entity.HasOne(wq => wq.User)
                  .WithMany(u => u.WrongQuestions)
                  .HasForeignKey(wq => wq.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wq => wq.Question)
                  .WithMany(q => q.WrongQuestions)
                  .HasForeignKey(wq => wq.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 21. XpEvents (Ledger)
        modelBuilder.Entity<XpEvent>(entity =>
        {
            entity.HasIndex(xe => xe.UserId);
            entity.HasIndex(xe => xe.CreatedAt);

            entity.HasOne(xe => xe.User)
                  .WithMany(u => u.XpEvents)
                  .HasForeignKey(xe => xe.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
