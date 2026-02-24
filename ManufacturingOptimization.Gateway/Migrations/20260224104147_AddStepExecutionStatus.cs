using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManufacturingOptimization.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddStepExecutionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionStatus",
                table: "ProcessSteps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionStatus",
                table: "ProcessSteps");
        }
    }
}
