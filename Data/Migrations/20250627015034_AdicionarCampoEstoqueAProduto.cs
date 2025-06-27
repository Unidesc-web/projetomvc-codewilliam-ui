using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RyujinBites.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCampoEstoqueAProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estoque",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estoque",
                table: "Produtos");
        }
    }
}
