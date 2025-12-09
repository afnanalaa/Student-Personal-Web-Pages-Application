using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Profile.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailContact",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "GitHub",
                table: "StudentProfiles");

            migrationBuilder.RenameColumn(
                name: "LinkedIn",
                table: "StudentProfiles",
                newName: "ContactInformation");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Projects",
                table: "StudentProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFile",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Projects",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ImageFile",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "ContactInformation",
                table: "StudentProfiles",
                newName: "LinkedIn");

            migrationBuilder.AddColumn<string>(
                name: "EmailContact",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHub",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
