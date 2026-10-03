using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwapAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveSessionRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditsCount = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiveSessionRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SwapRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ScheduledStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ActualEndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveSessionRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveSessionRooms_SwapRequests_SwapRequestId",
                        column: x => x.SwapRequestId,
                        principalTable: "SwapRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BalanceMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalEarnedMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalSpentMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeWallets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExternalPaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_CreditPackages_CreditPackageId",
                        column: x => x.CreditPackageId,
                        principalTable: "CreditPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeLedgerTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SwapRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    AmountMinutes = table.Column<int>(type: "int", nullable: false),
                    RunningBalanceMinutes = table.Column<int>(type: "int", nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PartnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeLedgerTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeLedgerTransactions_AspNetUsers_PartnerUserId",
                        column: x => x.PartnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeLedgerTransactions_SwapRequests_SwapRequestId",
                        column: x => x.SwapRequestId,
                        principalTable: "SwapRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeLedgerTransactions_TimeWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "TimeWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveSessionRooms_SwapRequestId",
                table: "LiveSessionRooms",
                column: "SwapRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_CreditPackageId",
                table: "PaymentOrders",
                column: "CreditPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLedgerTransactions_PartnerUserId",
                table: "TimeLedgerTransactions",
                column: "PartnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLedgerTransactions_ReferenceCode",
                table: "TimeLedgerTransactions",
                column: "ReferenceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeLedgerTransactions_SwapRequestId_TransactionType",
                table: "TimeLedgerTransactions",
                columns: new[] { "SwapRequestId", "TransactionType" },
                unique: true,
                filter: "[SwapRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLedgerTransactions_WalletId",
                table: "TimeLedgerTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeWallets_UserId",
                table: "TimeWallets",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveSessionRooms");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "TimeLedgerTransactions");

            migrationBuilder.DropTable(
                name: "CreditPackages");

            migrationBuilder.DropTable(
                name: "TimeWallets");
        }
    }
}
