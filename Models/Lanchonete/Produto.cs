using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class Produto
    {
        [Key]
        public int ProdutoId { get; set; }
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
        [StringLength(500)]
        public string? ImagemUrl { get; set; }

        public bool Disponivel { get; set; } = true;

        [Required] // Assumimos que todo produto tem uma quantidade em estoque
        [Range(0, int.MaxValue, ErrorMessage = "O estoque deve ser um número inteiro positivo.")]
        public int Estoque { get; set; } = 0; // Quantidade em estoque
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
        public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
    }
}
