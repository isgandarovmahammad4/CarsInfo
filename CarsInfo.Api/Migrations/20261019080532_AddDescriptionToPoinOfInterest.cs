using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarsInfo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToPoinOfInterest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointsOfInterest_Cars_CarId",
                table: "PointsOfInterest");

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "PointsOfInterest",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PointsOfInterest",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PointsOfInterest_Cars_CarId",
                table: "PointsOfInterest",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointsOfInterest_Cars_CarId",
                table: "PointsOfInterest");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PointsOfInterest");

            migrationBuilder.AlterColumn<int>(
                name: "CarId",
                table: "PointsOfInterest",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_PointsOfInterest_Cars_CarId",
                table: "PointsOfInterest",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id");
        }
    }
}
