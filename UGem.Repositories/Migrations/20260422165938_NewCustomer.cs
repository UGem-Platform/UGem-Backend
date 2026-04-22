using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class NewCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("c1aa1e3e-2cd9-46da-b385-ad3a17d94177"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("35d0b22c-793f-4109-8757-09204c9ef96e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f742e71b-cc7d-4ac0-a544-e9be4ab9f6ee"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("6a8494a1-37b4-4353-badc-bfd322cf133f"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "hungsui@gmail.com", "Trần Văn Hùng", true, false, "123456", "902222222", "Customer", null },
                    { new Guid("bc89b823-be06-4d56-8b8e-d85b2d6d587e"), null, new DateTimeOffset(new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "staff.ngoc@ugem.com", "Lê Bảo Ngọc", true, false, "123456", "901111111", "Staff", null }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "TotalCheckIns", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("36223781-8f27-4af4-8e68-a5a7ea7eb371"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, 0, null, new Guid("bc89b823-be06-4d56-8b8e-d85b2d6d587e") });

            migrationBuilder.InsertData(
                table: "Staffs",
                columns: new[] { "Id", "CreatedAt", "HiredAt", "IsDeleted", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("c59a23f0-d3ee-4de8-8a88-011202a497f9"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, new Guid("bc89b823-be06-4d56-8b8e-d85b2d6d587e") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("36223781-8f27-4af4-8e68-a5a7ea7eb371"));

            migrationBuilder.DeleteData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("c59a23f0-d3ee-4de8-8a88-011202a497f9"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6a8494a1-37b4-4353-badc-bfd322cf133f"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bc89b823-be06-4d56-8b8e-d85b2d6d587e"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("35d0b22c-793f-4109-8757-09204c9ef96e"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "hungsui@gmail.com", "Trần Văn Hùng", true, false, "123456", "902222222", "Customer", null },
                    { new Guid("f742e71b-cc7d-4ac0-a544-e9be4ab9f6ee"), null, new DateTimeOffset(new DateTime(2026, 4, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "staff.ngoc@ugem.com", "Lê Bảo Ngọc", true, false, "123456", "901111111", "Staff", null }
                });

            migrationBuilder.InsertData(
                table: "Staffs",
                columns: new[] { "Id", "CreatedAt", "HiredAt", "IsDeleted", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("c1aa1e3e-2cd9-46da-b385-ad3a17d94177"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, new Guid("f742e71b-cc7d-4ac0-a544-e9be4ab9f6ee") });
        }
    }
}
