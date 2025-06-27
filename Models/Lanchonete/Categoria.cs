using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.Lanchonete
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
