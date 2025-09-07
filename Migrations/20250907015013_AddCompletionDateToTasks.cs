using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TouchGanttChart.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletionDateToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "Tasks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "Tasks");
        }
    }
}
