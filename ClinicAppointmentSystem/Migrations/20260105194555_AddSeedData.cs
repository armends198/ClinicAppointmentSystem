using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppointmentStatuses",
                columns: new[] { "StatusId", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Appointment is awaiting confirmation", "Pending" },
                    { 2, "Appointment has been confirmed", "Approved" },
                    { 3, "Appointment has been completed", "Completed" },
                    { 4, "Appointment has been cancelled", "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "SpecializationId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Heart and cardiovascular system", "Cardiology" },
                    { 2, "Skin, hair, and nails", "Dermatology" },
                    { 3, "Medical care for children", "Pediatrics" },
                    { 4, "Bones, joints, and muscles", "Orthopedics" },
                    { 5, "Primary care and general medicine", "General Practice" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppointmentStatuses",
                keyColumn: "StatusId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AppointmentStatuses",
                keyColumn: "StatusId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AppointmentStatuses",
                keyColumn: "StatusId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AppointmentStatuses",
                keyColumn: "StatusId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "SpecializationId",
                keyValue: 5);
        }
    }
}
