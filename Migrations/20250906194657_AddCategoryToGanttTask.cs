using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TouchGanttChart.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToGanttTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectManager = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Budget = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ActualCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false, defaultValue: "#3498db")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EstimatedDurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "📋"),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Assignee = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "General"),
                    EstimatedHours = table.Column<double>(type: "REAL", precision: 10, scale: 2, nullable: false),
                    ActualHours = table.Column<double>(type: "REAL", precision: 10, scale: 2, nullable: false),
                    ParentTaskId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Tasks_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    EstimatedDurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentTaskTemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    DefaultAssigneeRole = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartOffsetDays = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMilestone = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCriticalPath = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTemplates_ProjectTemplates_ProjectTemplateId",
                        column: x => x.ProjectTemplateId,
                        principalTable: "ProjectTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTemplates_TaskTemplates_ParentTaskTemplateId",
                        column: x => x.ParentTaskTemplateId,
                        principalTable: "TaskTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskDependencies",
                columns: table => new
                {
                    DependentTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrerequisiteTaskId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDependencies", x => new { x.DependentTaskId, x.PrerequisiteTaskId });
                    table.ForeignKey(
                        name: "FK_TaskDependencies_Tasks_DependentTaskId",
                        column: x => x.DependentTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskDependencies_Tasks_PrerequisiteTaskId",
                        column: x => x.PrerequisiteTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplateDependency",
                columns: table => new
                {
                    DependentTaskTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrerequisiteTaskTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    DependencyType = table.Column<string>(type: "TEXT", nullable: false),
                    LagDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplateDependency", x => new { x.DependentTaskTemplateId, x.PrerequisiteTaskTemplateId });
                    table.ForeignKey(
                        name: "FK_TaskTemplateDependency_TaskTemplates_DependentTaskTemplateId",
                        column: x => x.DependentTaskTemplateId,
                        principalTable: "TaskTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTemplateDependency_TaskTemplates_PrerequisiteTaskTemplateId",
                        column: x => x.PrerequisiteTaskTemplateId,
                        principalTable: "TaskTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedDate",
                table: "Projects",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EndDate",
                table: "Projects",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsArchived",
                table: "Projects",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate",
                table: "Projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTemplates_Category",
                table: "ProjectTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTemplates_CreatedDate",
                table: "ProjectTemplates",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTemplates_IsActive",
                table: "ProjectTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTemplates_IsBuiltIn",
                table: "ProjectTemplates",
                column: "IsBuiltIn");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTemplates_Name",
                table: "ProjectTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDependencies_PrerequisiteTaskId",
                table: "TaskDependencies",
                column: "PrerequisiteTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Assignee",
                table: "Tasks",
                column: "Assignee");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Category",
                table: "Tasks",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedDate",
                table: "Tasks",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_EndDate",
                table: "Tasks",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Name",
                table: "Tasks",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentTaskId",
                table: "Tasks",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Priority",
                table: "Tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Progress",
                table: "Tasks",
                column: "Progress");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StartDate",
                table: "Tasks",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateDependency_DependentTaskTemplateId",
                table: "TaskTemplateDependency",
                column: "DependentTaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplateDependency_PrerequisiteTaskTemplateId",
                table: "TaskTemplateDependency",
                column: "PrerequisiteTaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_IsCriticalPath",
                table: "TaskTemplates",
                column: "IsCriticalPath");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_IsMilestone",
                table: "TaskTemplates",
                column: "IsMilestone");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_Order",
                table: "TaskTemplates",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_ParentTaskTemplateId",
                table: "TaskTemplates",
                column: "ParentTaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_ProjectTemplateId",
                table: "TaskTemplates",
                column: "ProjectTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskDependencies");

            migrationBuilder.DropTable(
                name: "TaskTemplateDependency");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TaskTemplates");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectTemplates");
        }
    }
}
