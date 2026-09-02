using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOne.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingGoalCurrentAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentAmount",
                table: "SavingGoals",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAmount",
                table: "SavingGoals");
        }
    }
}
