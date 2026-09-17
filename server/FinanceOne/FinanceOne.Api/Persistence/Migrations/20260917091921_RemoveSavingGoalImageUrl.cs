using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOne.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSavingGoalImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "SavingGoals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "SavingGoals",
                type: "longtext",
                nullable: true);
        }
    }
}
