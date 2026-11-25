using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerFeedback.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnalysisCategory",
                table: "Feedbacks",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisSentiment",
                table: "Feedbacks",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisSummary",
                table: "Feedbacks",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalyzedAtUtc",
                table: "Feedbacks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisCategory",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AnalysisSentiment",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AnalysisSummary",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "AnalyzedAtUtc",
                table: "Feedbacks");
        }
    }
}
