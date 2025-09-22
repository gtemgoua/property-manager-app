using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RentalUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    State = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "numeric", nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    SquareFeet = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentalContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RentalUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentDueDay = table.Column<int>(type: "integer", nullable: false),
                    PaymentSchedule = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalContracts_RentalUnits_RentalUnitId",
                        column: x => x.RentalUnitId,
                        principalTable: "RentalUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalContracts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RentalContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LateFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ReceiptSent = table.Column<bool>(type: "boolean", nullable: false),
                    ReceiptSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentPayments_RentalContracts_RentalContractId",
                        column: x => x.RentalContractId,
                        principalTable: "RentalContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    RentPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentLogs_RentPayments_RentPaymentId",
                        column: x => x.RentPaymentId,
                        principalTable: "RentPayments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RentPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AlertDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAlerts_RentPayments_RentPaymentId",
                        column: x => x.RentPaymentId,
                        principalTable: "RentPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RentalUnits",
                columns: new[] { "Id", "AddressLine1", "AddressLine2", "Bathrooms", "Bedrooms", "City", "CreatedAt", "MonthlyRent", "Name", "Notes", "PostalCode", "SquareFeet", "State", "Status", "UpdatedAt" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "123 Main Street", null, 2, 2, "Metropolis", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1800m, "Unit 101", null, "94016", 950.0, "CA", 1, null });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "Email", "EmergencyContactName", "EmergencyContactPhone", "FirstName", "LastName", "Notes", "PhoneNumber", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ada@example.com", null, null, "Ada", "Lovelace", null, "+11234567890", null });

            migrationBuilder.InsertData(
                table: "RentalContracts",
                columns: new[] { "Id", "CreatedAt", "DepositAmount", "EndDate", "MonthlyRent", "Notes", "PaymentDueDay", "PaymentSchedule", "RentalUnitId", "StartDate", "Status", "TenantId", "UpdatedAt" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1800m, null, 1800m, null, 5, 0, new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new Guid("11111111-1111-1111-1111-111111111111"), null });

            migrationBuilder.InsertData(
                table: "RentPayments",
                columns: new[] { "Id", "AmountDue", "AmountPaid", "CreatedAt", "DueDate", "LateFee", "Notes", "PaidDate", "PaymentMethod", "ReceiptNumber", "ReceiptSent", "ReceiptSentAt", "ReferenceNumber", "RentalContractId", "Status", "UpdatedAt" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), 1800m, 0m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), 0m, null, null, 0, "RCPT-DEMO-202401", false, null, null, new Guid("33333333-3333-3333-3333-333333333333"), 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_DocumentType",
                table: "DocumentLogs",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_RentPaymentId",
                table: "DocumentLogs",
                column: "RentPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAlerts_RentPaymentId",
                table: "PaymentAlerts",
                column: "RentPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalContracts_RentalUnitId",
                table: "RentalContracts",
                column: "RentalUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalContracts_TenantId",
                table: "RentalContracts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RentPayments_RentalContractId_DueDate",
                table: "RentPayments",
                columns: new[] { "RentalContractId", "DueDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentLogs");

            migrationBuilder.DropTable(
                name: "PaymentAlerts");

            migrationBuilder.DropTable(
                name: "RentPayments");

            migrationBuilder.DropTable(
                name: "RentalContracts");

            migrationBuilder.DropTable(
                name: "RentalUnits");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
