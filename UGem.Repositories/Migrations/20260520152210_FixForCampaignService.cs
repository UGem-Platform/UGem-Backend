using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixForCampaignService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCampaignCode",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CampaignId",
                table: "Orders",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Campaigns_CampaignId",
                table: "Orders",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Campaigns_CampaignId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CampaignId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AppliedCampaignCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");
        }
    }
}
