using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_Profile.Migrations
{
    /// <inheritdoc />
    public partial class AddReportedPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReported",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportsCount",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReported",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ReportsCount",
                table: "Posts");
        }
    }
}
