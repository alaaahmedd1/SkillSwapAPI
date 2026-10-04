using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwapAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhiteboardSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhiteboardSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanvasDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteboardSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteboardSnapshots_LiveSessionRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "LiveSessionRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhiteboardSnapshots_RoomId",
                table: "WhiteboardSnapshots",
                column: "RoomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhiteboardSnapshots");
        }
    }
}
