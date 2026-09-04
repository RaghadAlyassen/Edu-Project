using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edu_Project.Migrations
{
    /// <inheritdoc />
    public partial class MakeCategoriesGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_InstructorId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_InstructorId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImg",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImg",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_InstructorId",
                table: "Categories",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_InstructorId",
                table: "Categories",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
