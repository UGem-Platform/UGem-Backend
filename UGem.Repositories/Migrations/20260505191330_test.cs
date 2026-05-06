using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$djQIzA3jqT8bLNkbv26WbeXrsNFKxWiRH.3Dqopu2swKnvXzrXWia");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$oI9RrKpffBN/DAaIN.AxlOnvUL0UF2JUsbbgAB6hVt8wpDxWa3h8S");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$ftV3kzNi4KLOIV16nbzSl.rcsD3ydcEd4LDTHo3kA/FPORo712MIi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$MxgCS6RnOdy9J5QbZUz2zuA9dqkh6iTdnEx.sGfQwf.80m8b2dIfC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$uD8c.deEZRKwUhIUh9Qga.CxVVwCU8.aWXUNnmmwNFHCupUCS6vtG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$u3c1iisQ6Glyp2c5zqgvaOi4hsim/XDaZbXP9nEidZ/PyVX61HWyG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$9hcNLUScBT5RUoxgm2.ZSulX4Kn4U4lEccpb1MoGoeeTfAeVFHar2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$JpUjuPAYFf3RUC9f4EMQYOdOaQ.M2OcPND9uDqiF617WbAydxgDt2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$5M6c1QRQLhewIU/tk2ENFO4F9mPxr8LRphxtR209i3/VDdWw5rDsu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$6eOLJ5l5.eXbO1hgXTXObeiHeKOlAsBgAgUdr434K1FszLsU28qs2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$X0WTkX.ZS9z52D1PAPMySOqpRMSOro3CyU7cvVRw6BnRejlOAMULK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$dB9hqOm/4Lq/rIHjvi/HWeuPYishHZKmmGhjPNA2lv1dYidf4s/Dq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$aEaVT/SmRkCzLIN1RDkpOOrYemeOo7iebHD6qL8Disvwyt9VNr3Ee");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$NTtoCwbUSqb0FIBBuWZrjeSg6FJ///qnO0OkdC7l/DgUqgvceeHJ6");
        }
    }
}
