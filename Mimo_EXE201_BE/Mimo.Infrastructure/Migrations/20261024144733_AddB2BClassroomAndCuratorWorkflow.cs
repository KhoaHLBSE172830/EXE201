using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddB2BClassroomAndCuratorWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsedSeats",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CuratorId",
                table: "Nodes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClassCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FocusTopic = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MaxStudents = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Users_EducatorId",
                        column: x => x.EducatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QuestionFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionFeedbacks_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionFeedbacks_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TeacherLicenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivationCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AssignedClassesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ClaimedByEducatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherLicenses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherLicenses_Users_ClaimedByEducatorId",
                        column: x => x.ClaimedByEducatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PassScoreThreshold = table.Column<int>(type: "integer", nullable: false),
                    RewardXp = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignments_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assignments_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassMembers_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassMembers_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    BestScore = table.Column<int>(type: "integer", nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentProgress_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentProgress_LessonSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "LessonSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssignmentProgress_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CuratorId",
                table: "Nodes",
                column: "CuratorId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSessions_AssignmentId",
                table: "LessonSessions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProgress_AssignmentId_StudentId",
                table: "AssignmentProgress",
                columns: new[] { "AssignmentId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProgress_SessionId",
                table: "AssignmentProgress",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProgress_StudentId",
                table: "AssignmentProgress",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ClassId_Deadline",
                table: "Assignments",
                columns: new[] { "ClassId", "Deadline" });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CreatedBy",
                table: "Assignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_NodeId",
                table: "Assignments",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ClassCode",
                table: "Classes",
                column: "ClassCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_EducatorId",
                table: "Classes",
                column: "EducatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_OrganizationId",
                table: "Classes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassMembers_ClassId_StudentId",
                table: "ClassMembers",
                columns: new[] { "ClassId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassMembers_StudentId",
                table: "ClassMembers",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionFeedbacks_QuestionId",
                table: "QuestionFeedbacks",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionFeedbacks_ReviewerId",
                table: "QuestionFeedbacks",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherLicenses_ActivationCode",
                table: "TeacherLicenses",
                column: "ActivationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherLicenses_ClaimedByEducatorId",
                table: "TeacherLicenses",
                column: "ClaimedByEducatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherLicenses_OrganizationId",
                table: "TeacherLicenses",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonSessions_Assignments_AssignmentId",
                table: "LessonSessions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Users_CuratorId",
                table: "Nodes",
                column: "CuratorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonSessions_Assignments_AssignmentId",
                table: "LessonSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Users_CuratorId",
                table: "Nodes");

            migrationBuilder.DropTable(
                name: "AssignmentProgress");

            migrationBuilder.DropTable(
                name: "ClassMembers");

            migrationBuilder.DropTable(
                name: "QuestionFeedbacks");

            migrationBuilder.DropTable(
                name: "TeacherLicenses");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_CuratorId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_LessonSessions_AssignmentId",
                table: "LessonSessions");

            migrationBuilder.DropColumn(
                name: "UsedSeats",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CuratorId",
                table: "Nodes");
        }
    }
}
