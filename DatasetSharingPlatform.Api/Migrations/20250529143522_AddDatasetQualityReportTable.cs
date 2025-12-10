using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatasetSharingPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasetQualityReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DatasetQualityReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatasetId = table.Column<int>(type: "int", nullable: false),
                    MissingValueRate = table.Column<double>(type: "float", nullable: false),
                    DuplicateRowCount = table.Column<int>(type: "int", nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: false),
                    QualityScore = table.Column<double>(type: "float", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasetQualityReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatasetQualityReports_Datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "Datasets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatasetQualityReports_DatasetId",
                table: "DatasetQualityReports",
                column: "DatasetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatasetQualityReports");
        }
    }
}
