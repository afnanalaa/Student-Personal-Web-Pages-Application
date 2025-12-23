using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Profile.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivacyMode",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyMode",
                table: "StudentProfiles");
        }
    }
}
