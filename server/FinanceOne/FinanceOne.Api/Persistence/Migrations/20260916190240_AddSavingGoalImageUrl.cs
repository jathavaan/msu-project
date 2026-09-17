using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOne.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingGoalImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "SavingGoals",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "SavingGoals");
        }
    }
}
