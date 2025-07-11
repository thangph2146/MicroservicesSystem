using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManagementApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToInternships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Internships",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Internships");
        }
    }
}
