using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeClimate.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetricKinds",
                columns: table => new
                {
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    MinValue = table.Column<double>(type: "double precision", nullable: false),
                    MaxValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricKinds", x => x.Kind);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Firmware = table.Column<string>(type: "text", nullable: false),
                    IntervalSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorMetric",
                columns: table => new
                {
                    SensorId = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorMetric", x => new { x.SensorId, x.Kind });
                    table.ForeignKey(
                        name: "FK_SensorMetric_MetricKinds_Kind",
                        column: x => x.Kind,
                        principalTable: "MetricKinds",
                        principalColumn: "Kind",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensorMetric_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MetricKinds",
                columns: new[] { "Kind", "MaxValue", "MinValue", "Unit" },
                values: new object[] { "co2", 10000.0, 300.0, "ppm" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorMetric_Kind",
                table: "SensorMetric",
                column: "Kind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorMetric");

            migrationBuilder.DropTable(
                name: "MetricKinds");

            migrationBuilder.DropTable(
                name: "Sensors");
        }
    }
}
