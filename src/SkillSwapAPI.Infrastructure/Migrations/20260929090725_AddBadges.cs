using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SkillSwapAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBadgeAwards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevieweeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AwardedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadgeAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadgeAwards_AspNetUsers_RevieweeId",
                        column: x => x.RevieweeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBadgeAwards_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBadgeAwards_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBadgeAwards_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Description", "IconUrl", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Awarded to mentors who explain difficult concepts clearly.", "/images/badges/super-patient.svg", true, "Super Patient" },
                    { 2, "Awarded for outstanding teaching and guidance in sessions.", "/images/badges/best-tutor.svg", true, "Best Tutor" },
                    { 3, "Awarded for breaking down tough problems into simple steps.", "/images/badges/problem-solver.svg", true, "Problem Solver" },
                    { 4, "Awarded for clear, responsive, and helpful communication.", "/images/badges/great-communicator.svg", true, "Great Communicator" },
                    { 5, "Awarded for consistency and punctuality in scheduled sessions.", "/images/badges/reliable-partner.svg", true, "Reliable Partner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Badges_Name",
                table: "Badges",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBadgeAwards_BadgeId",
                table: "UserBadgeAwards",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadgeAwards_RevieweeId",
                table: "UserBadgeAwards",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadgeAwards_ReviewerId",
                table: "UserBadgeAwards",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadgeAwards_ReviewId",
                table: "UserBadgeAwards",
                column: "ReviewId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBadgeAwards");

            migrationBuilder.DropTable(
                name: "Badges");
        }
    }
}
