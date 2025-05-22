using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesTmdb.Migrations
{
    /// <inheritdoc />
    public partial class AddPopularityToMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Popularity",
                table: "Movies",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Movies");
        }
    }
}
