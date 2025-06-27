using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RyujinBites.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCampoStatusEmAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Avaliacoes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Avaliacoes");
        }
    }
}
