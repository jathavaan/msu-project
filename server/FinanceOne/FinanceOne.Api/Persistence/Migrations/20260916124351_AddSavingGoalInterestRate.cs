using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOne.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingGoalInterestRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "SavingGoals",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "SavingGoals");
        }
    }
}
