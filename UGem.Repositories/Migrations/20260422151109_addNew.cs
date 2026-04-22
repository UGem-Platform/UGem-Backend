using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("fc8a15cd-a6a9-4cb6-92cf-b820714056f4"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0f90d3a4-1acf-4875-ac69-44e9b464f3d9"), null, new DateTimeOffset(new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "staff.ngoc@ugem.com", "Lê Bảo Ngọc", true, false, "123456", "901111111", "Staff", null },
                    { new Guid("25a76e7a-2f37-4fdd-a8c8-48b9e0872c5d"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "hungsui@gmail.com", "Trần Văn Hùng", true, false, "123456", "902222222", "Customer", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0f90d3a4-1acf-4875-ac69-44e9b464f3d9"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("25a76e7a-2f37-4fdd-a8c8-48b9e0872c5d"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[] { new Guid("fc8a15cd-a6a9-4cb6-92cf-b820714056f4"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "hungsui@gmail.com", "Trần Văn Hùng", true, false, "123456", "902222222", "Customer", null });
        }
    }
}
