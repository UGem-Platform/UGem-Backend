using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldForOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
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
