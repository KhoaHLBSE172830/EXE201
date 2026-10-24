using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mimo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Transaction_For_VNPay_Payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Courses_CourseId",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Transactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderInfo",
                table: "Transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanCode",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TxnRef",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VnpayResponseCode",
                table: "Transactions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VnpayTransactionNo",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TxnRef",
                table: "Transactions",
                column: "TxnRef");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Courses_CourseId",
                table: "Transactions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Courses_CourseId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TxnRef",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "OrderInfo",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PlanCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TxnRef",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "VnpayResponseCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "VnpayTransactionNo",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Courses_CourseId",
                table: "Transactions",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
