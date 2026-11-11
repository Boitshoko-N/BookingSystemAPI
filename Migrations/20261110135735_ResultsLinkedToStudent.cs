using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class ResultsLinkedToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentResults_Bookings_BookingId",
                table: "AssessmentResults");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "AssessmentResults",
                newName: "StudentNo");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentResults_BookingId",
                table: "AssessmentResults",
                newName: "IX_AssessmentResults_StudentNo");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentResults_Students_StudentNo",
                table: "AssessmentResults",
                column: "StudentNo",
                principalTable: "Students",
                principalColumn: "StudentNo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentResults_Students_StudentNo",
                table: "AssessmentResults");

            migrationBuilder.RenameColumn(
                name: "StudentNo",
                table: "AssessmentResults",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentResults_StudentNo",
                table: "AssessmentResults",
                newName: "IX_AssessmentResults_BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentResults_Bookings_BookingId",
                table: "AssessmentResults",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
