using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMonetizationLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MonetizationProcessedAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewerWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerWalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewerWalletTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewerWalletTransactions_Reviewers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Reviewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Reviewers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 23, 8, 35, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$ETv6aeIJVgY3sBsmsQBzBehjdNxLPddu0z.1bREP/vWjYc0h1ZBr2");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerWalletTransactions_OrderId_Type",
                table: "ReviewerWalletTransactions",
                columns: new[] { "OrderId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerWalletTransactions_ReviewerId",
                table: "ReviewerWalletTransactions",
                column: "ReviewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewerWalletTransactions");

            migrationBuilder.DropColumn(
                name: "MonetizationProcessedAtUtc",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Reviewers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 5, 17, 2, 45, 48, 241, DateTimeKind.Unspecified).AddTicks(6990), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 5, 17, 2, 45, 48, 241, DateTimeKind.Unspecified).AddTicks(6995), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$.4RxXv.Mu9zdzoGGDouDDuJN1NP4heUVKNr9XcSHOVc7xHr4jbDQ6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("19191919-1919-1919-1919-191919191919"),
                column: "PasswordHash",
                value: "$2a$11$u4k7XAzysRmeNO042aaMbejvdf7h3OQQOEYjj0Lq2eX5/LEsDzyXy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                column: "PasswordHash",
                value: "$2a$11$ZU1aCE0Pdv5vBiV1zumjxupeuiOWNXM46Z7pyVgfKABtaBkZsD9CS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$F0/S9UvNeSOkE166jT.GyO8hW5bz8xGCq3/4ITbNu9QV6IAFIjo6a");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$SxUIqpefqX4JRga9SWw5duetZkGihB8sAnuVWVLItBh2r2nJ5bVgG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "PasswordHash",
                value: "$2a$11$A6t20s4Im2fCUreZCM4Pv.HG5oMMnEwwHJTNEGOrG.EqpdsswxhTC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "PasswordHash",
                value: "$2a$11$wgpV82qfEa39Bwrr5pRX1u55fsI9bDWVzyMvUkpIgqVHmHtSz1ihW");
        }
    }
}
