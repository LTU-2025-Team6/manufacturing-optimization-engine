using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManufacturingOptimization.Gateway.Migrations
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
                name: "ProviderSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSchedules", x => x.Id);
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
                name: "ProviderScheduleSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderScheduleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SegmentType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderScheduleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderScheduleSegments_ProviderSchedules_ProviderScheduleId",
                        column: x => x.ProviderScheduleId,
                        principalTable: "ProviderSchedules",
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

            migrationBuilder.CreateTable(
                name: "OptimizationMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrategyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalTime = table.Column<long>(type: "INTEGER", nullable: false),
                    AverageQuality = table.Column<double>(type: "REAL", nullable: false),
                    TotalEmissionsKgCO2 = table.Column<double>(type: "REAL", nullable: false),
                    SolverStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ObjectiveValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SelectedStrategyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptimizationStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StrategyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WorkflowType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimizationStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptimizationStrategies_OptimizationPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "OptimizationPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrategyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Process = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SelectedProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SelectedProviderName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProviderScheduleId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessSteps_OptimizationStrategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "OptimizationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessSteps_ProviderSchedules_ProviderScheduleId",
                        column: x => x.ProviderScheduleId,
                        principalTable: "ProviderSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrategyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DurationMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IncludesInsurance = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyTerms_OptimizationStrategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "OptimizationStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcessStepId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Duration = table.Column<double>(type: "REAL", nullable: false),
                    QualityScore = table.Column<double>(type: "REAL", nullable: false),
                    EmissionsKgCO2 = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessEstimates_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreakPeriods_ProviderId",
                table: "BreakPeriods",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationMetrics_StrategyId",
                table: "OptimizationMetrics",
                column: "StrategyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationPlans_SelectedStrategyId",
                table: "OptimizationPlans",
                column: "SelectedStrategyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptimizationStrategies_PlanId",
                table: "OptimizationStrategies",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessCapabilities_ProviderId",
                table: "ProcessCapabilities",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessEstimates_ProcessStepId",
                table: "ProcessEstimates",
                column: "ProcessStepId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_ProviderScheduleId",
                table: "ProcessSteps",
                column: "ProviderScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_StrategyId",
                table: "ProcessSteps",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderScheduleSegments_ProviderScheduleId",
                table: "ProviderScheduleSegments",
                column: "ProviderScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalCapabilities_ProviderId",
                table: "TechnicalCapabilities",
                column: "ProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyTerms_StrategyId",
                table: "WarrantyTerms",
                column: "StrategyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OptimizationMetrics_OptimizationStrategies_StrategyId",
                table: "OptimizationMetrics",
                column: "StrategyId",
                principalTable: "OptimizationStrategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OptimizationPlans_OptimizationStrategies_SelectedStrategyId",
                table: "OptimizationPlans",
                column: "SelectedStrategyId",
                principalTable: "OptimizationStrategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptimizationPlans_OptimizationStrategies_SelectedStrategyId",
                table: "OptimizationPlans");

            migrationBuilder.DropTable(
                name: "BreakPeriods");

            migrationBuilder.DropTable(
                name: "OptimizationMetrics");

            migrationBuilder.DropTable(
                name: "ProcessCapabilities");

            migrationBuilder.DropTable(
                name: "ProcessEstimates");

            migrationBuilder.DropTable(
                name: "ProviderScheduleSegments");

            migrationBuilder.DropTable(
                name: "TechnicalCapabilities");

            migrationBuilder.DropTable(
                name: "WarrantyTerms");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropTable(
                name: "ProcessSteps");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "ProviderSchedules");

            migrationBuilder.DropTable(
                name: "OptimizationStrategies");

            migrationBuilder.DropTable(
                name: "OptimizationPlans");
        }
    }
}
