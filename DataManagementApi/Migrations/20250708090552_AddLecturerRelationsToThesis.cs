using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerRelationsToThesis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Theses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Theses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExaminerId",
                table: "Theses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Theses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Theses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Theses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Theses_ExaminerId",
                table: "Theses",
                column: "ExaminerId");

            migrationBuilder.CreateIndex(
                name: "IX_Theses_SupervisorId",
                table: "Theses",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Theses_Lecturers_ExaminerId",
                table: "Theses",
                column: "ExaminerId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Theses_Lecturers_SupervisorId",
                table: "Theses",
                column: "SupervisorId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Theses_Lecturers_ExaminerId",
                table: "Theses");

            migrationBuilder.DropForeignKey(
                name: "FK_Theses_Lecturers_SupervisorId",
                table: "Theses");

            migrationBuilder.DropIndex(
                name: "IX_Theses_ExaminerId",
                table: "Theses");

            migrationBuilder.DropIndex(
                name: "IX_Theses_SupervisorId",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "ExaminerId",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Theses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Theses");
        }
    }
}
