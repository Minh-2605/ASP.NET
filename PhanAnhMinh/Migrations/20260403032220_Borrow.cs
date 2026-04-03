using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhanAnhMinh.Migrations
{
    /// <inheritdoc />
    public partial class Borrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Borrows",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Borrows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Borrows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Borrows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borrows_UserId1",
                table: "Borrows",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrows_Users_UserId1",
                table: "Borrows",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrows_Users_UserId1",
                table: "Borrows");

            migrationBuilder.DropIndex(
                name: "IX_Borrows_UserId1",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Borrows");
        }
    }
}
