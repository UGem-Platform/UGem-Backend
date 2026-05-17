using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ahehe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Reviewers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 5, 13, 11, 59, 32, 905, DateTimeKind.Unspecified).AddTicks(1017), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 5, 13, 11, 59, 32, 905, DateTimeKind.Unspecified).AddTicks(1020), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$khZuQ0Zb5SSTwJnrNBJeTOseBklea9ufy5.42W9aMB4n8BRDEDOUm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$Z2IMT5l6iLAhNZUHeuevg.KjyRxRqtUOsyTQGWAzbCxz4TUSo1ZfW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$u0qDiVvbG8RNERjPYbG1VOxAfmpOPffpOd5QAj2c0ce7m/BixaRc6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$7QGPEYVkvMVhRdAmHioDluGCIZ74ntE.AIk72wrq/cj3YXKcxCgi2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$hAdBEDdfs.mSD0ZejpAowOKjmSiJyYM5gV5fxzhvfQmjfADqlaV.e");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$46GoT8AzFzGiuL6dnwVu1uBOAyXMejOYwF4kiR31vtuiDgmgRPKz.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$h8Cou5aUbCdL7gK5pC2T3eFAGJnFAao4SYu1/J/60BHHBuoOletCq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Reviewers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 5, 13, 11, 57, 2, 410, DateTimeKind.Unspecified).AddTicks(3747), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 5, 13, 11, 57, 2, 410, DateTimeKind.Unspecified).AddTicks(3750), new TimeSpan(0, 0, 0, 0, 0)) });

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
                column: "PasswordHash",
                value: "$2a$11$uOsYAKo4raF3QJxD/D/J6eXO3dfATfRcr7AtJfBUKaft1e6IFbqHW");
        }
    }
}
