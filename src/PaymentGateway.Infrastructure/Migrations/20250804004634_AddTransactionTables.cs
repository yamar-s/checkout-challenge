using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionEvents_TransactionId_StepOrder",
                table: "TransactionEvents");

            migrationBuilder.DropColumn(
                name: "ErrorDetails",
                table: "TransactionEvents");

            migrationBuilder.DropColumn(
                name: "StepOrder",
                table: "TransactionEvents");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "TransactionEvents",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "TransactionEvents",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TransactionEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEvents_TransactionId",
                table: "TransactionEvents",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionEvents_TransactionId",
                table: "TransactionEvents");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TransactionEvents");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TransactionEvents",
                newName: "OccurredAt");

            migrationBuilder.AlterColumn<int>(
                name: "EventType",
                table: "TransactionEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ErrorDetails",
                table: "TransactionEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepOrder",
                table: "TransactionEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionEvents_TransactionId_StepOrder",
                table: "TransactionEvents",
                columns: new[] { "TransactionId", "StepOrder" });
        }
    }
}
