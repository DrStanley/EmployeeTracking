using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAuditAndReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Employees",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferredByEmployeeId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ReferredByEmployeeId",
                table: "Employees",
                column: "ReferredByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_ReferredByEmployeeId",
                table: "Employees",
                column: "ReferredByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_ReferredByEmployeeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ReferredByEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ReferredByEmployeeId",
                table: "Employees");
        }
    }
}
