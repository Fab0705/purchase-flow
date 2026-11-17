using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace backend.Modules.FieldService.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialFieldService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fieldservice");

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "fieldservice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTechnicianId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ScheduledDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedTechnicianId",
                schema: "fieldservice",
                table: "Tickets",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_LocationId",
                schema: "fieldservice",
                table: "Tickets",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "fieldservice");
        }
    }
}
