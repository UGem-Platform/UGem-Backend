using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Updatereviewer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Reviewers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "Reviewers",
                columns: new[] { "Id", "Balance", "CommissionRate", "CreatedAt", "CustomerId", "IsDeleted", "Points", "Rank", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 0m, 0m, new DateTimeOffset(new DateTime(2026, 5, 13, 11, 57, 2, 410, DateTimeKind.Unspecified).AddTicks(3747), new TimeSpan(0, 0, 0, 0, 0)), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), false, 55, "Gold", new DateTimeOffset(new DateTime(2026, 5, 13, 11, 57, 2, 410, DateTimeKind.Unspecified).AddTicks(3750), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$sodbTyYIBAoMdq/ODQ.wP.Yo5VvBbq0R1nIYxckybGuZLHAWlhr2q");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$DNp.9p9XVtKnDk2FjYBwjuX5AENaZ7tz/saQCBvOwrgtTc7SbVz/2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$6DoWP3q8T/9UxM/TNijFSelqSvQ7PI31.gbqJRQrWmWWHSmQJGseC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$.IJuejpBPkh86H7ByhNO6eu73hoqz7cCXH9ydcLWID77mpKmJHJVm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$I7IL8Ro8K72ZVmJxxMf0iejeVRPZYeWlEQ6u2pea8T5aupI1bzQNC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$.JqtSkllUmSF8Xwq17v5G.Ffd8RpuTp8wvla5ZbMItpqlCi7OhPD.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "$2a$11$uOsYAKo4raF3QJxD/D/J6eXO3dfATfRcr7AtJfBUKaft1e6IFbqHW", "Reviewer" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reviewers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Reviewers");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$77OHL9C.2WjH1HOrIxBYCOE5H9ezfQTNwLysOiLHiIGyIW6JlPMIC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$f/LKoxwBBv0LIiwEkdXMJuP2zcpm3g1DJqWVCDetQSUZ8M.WpX3KS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$NlZN.xacm2s.WvmYX3cyHe9MBNJqWhRiLi8TCvNBhK2MFFL6jEk..");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$V9fqJVL/YX8KILYYq2lofOS/ALxgUfl1imUgguyn5AYyF4/kSzgY2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$CldXW8KazSyksTW7NnV6Y.HT4tImUKktRIiB.m6MDMUmcJyxTRszq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$BeSnDD4vb/RMsTO1rwBHdeHmRsrqxo70oAbDlxkWRifiLV8eoNKO6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "PasswordHash", "Role" },
                values: new object[] { "$2a$11$LS3mH10Ry4v.XMrlIsom1ejWzjHRkWIuwZVfxlSvfYic5NqXgPru6", "Customer" });
        }
    }
}
