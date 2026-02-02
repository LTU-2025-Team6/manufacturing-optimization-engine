using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManufacturingOptimization.ProviderRegistry.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Process = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CostPerHour = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SpeedMultiplier = table.Column<double>(type: "REAL", nullable: false),
                    QualityScore = table.Column<double>(type: "REAL", nullable: false),
                    EnergyConsumptionKwhPerHour = table.Column<double>(type: "REAL", nullable: false),
                    CarbonIntensityKgCO2PerKwh = table.Column<double>(type: "REAL", nullable: false),
                    UsesRenewableEnergy = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessCapabilities_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicalCapabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AxisHeight = table.Column<double>(type: "REAL", nullable: false),
                    Power = table.Column<double>(type: "REAL", nullable: false),
                    Tolerance = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicalCapabilities_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkDayStartHour = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkDayEndHour = table.Column<int>(type: "INTEGER", nullable: false),
                    Is24x7 = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkingDaysJson = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => x.ProviderId);
                    table.ForeignKey(
                        name: "FK_WorkingHours_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BreakPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartHour = table.Column<int>(type: "INTEGER", nullable: false),
                    StartMinute = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakPeriods_WorkingHours_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "WorkingHours",
                        principalColumn: "ProviderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreakPeriods_ProviderId",
                table: "BreakPeriods",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessCapabilities_ProviderId",
                table: "ProcessCapabilities",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalCapabilities_ProviderId",
                table: "TechnicalCapabilities",
                column: "ProviderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreakPeriods");

            migrationBuilder.DropTable(
                name: "ProcessCapabilities");

            migrationBuilder.DropTable(
                name: "TechnicalCapabilities");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}
