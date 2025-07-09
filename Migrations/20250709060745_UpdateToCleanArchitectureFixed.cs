using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemInstaller.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToCleanArchitectureFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantUsers_TenantId_Email",
                table: "TenantUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "InstallationId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Installation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installation_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_InstallationId",
                table: "Tasks",
                column: "InstallationId");

            migrationBuilder.CreateIndex(
                name: "IX_Installation_EnvironmentId",
                table: "Installation",
                column: "EnvironmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Installation_InstallationId",
                table: "Tasks",
                column: "InstallationId",
                principalTable: "Installation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Installation_InstallationId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Installation");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_InstallationId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "InstallationId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_TenantId_Email",
                table: "TenantUsers",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }
    }
}
