using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class Avaliacao
    {
        [Key]
        public int AvaliacaoId { get; set; }
        public int ProdutoId { get; set; }
        [ForeignKey("ProdutoId")]
        public Produto? Produto { get; set; }
        public string ClienteId { get; set; } = null!;
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
        public int Pontuacao { get; set; }
        [StringLength(1000, ErrorMessage = "O comentário deve ter no máximo 1000 caracteres.")]
        public string? Comentario { get; set; }
        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
        public bool IsReported { get; set; } = false; // Indica se a avaliação foi marcada como denunciada
        public string Status { get; set; } = "Pendente";
    }
}
