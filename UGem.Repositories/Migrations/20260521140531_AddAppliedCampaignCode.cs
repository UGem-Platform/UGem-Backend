using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UGem.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAppliedCampaignCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCampaignCode",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedCampaignCode",
                table: "Orders");   
        }
    }
}
