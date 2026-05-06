using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class kkk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ReviewerApplications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ReviewerApplications",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
                column: "RejectionReason",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$E0bDlQKOp5o/Pji1GOVFZOmEF3PeykAkmXnFa1WyXaFg2YzijxO0m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$hEF.jONJGWg4F7xNnDnbD.L6c1Gz9YuPeH4uEb2p7xBdd318Qc8NG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$JmuAIu1xo6A5kwxSVLScPuKq4rqB1VZLcdMYGWTSYxHm5zD4Y6w46");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$doUoRW4nw4kyeh21Q.X9Y.hVIQZQ9Yoq4SBrAnoid1t0qf7sayAK6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$BXCWwYcfru8lZNuekIDXUebivEgvVTJWIMVSd0Nzk8IqoK2fUKm3m");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$oiUx/BM3oXjL2pYTpEJELOt3t3XMt0z0Bxrf.z.focjEvwdjHm3Am");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$Wkmoqp76eQjcsKBWrSCpNefZWdVxX9nYO6ti9CBpqOhsOttChXUXW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ReviewerApplications");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$9vvt35PAlSYX2LB2HHwkauEr2.zocPzEnDLCWzzkImFGD6.Ak.Igu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$d0.R5j.NLWKIiqrWAu6kI.XIRltj8PWmI.SVOxHE9xWfD9QojA0n2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$toOzWbPGM4UI/GPq/mmBhu94.55nfUQQ62S545eUoaxCehpem7gQO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$STwXPqOd9d.NH65yq9X0EeL6A2aZ68GrOm1ZkiR1QaIT3P/LXwNeK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$geB1h4sxlUX6eKCAPigP.ezbJ2xlNmgxlkLpZ7moZA/Etwr0QdDHm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$E5Vp277uYx13ift2/rDrKup6gAIR2xyEUgpqDOykJropTJ3mbbQwq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$Wc7uVHwDCU3wOGAzWxY7ee03kw1JH0WhBPwCWet3/QeIllSgLiZ4S");
        }
    }
}
