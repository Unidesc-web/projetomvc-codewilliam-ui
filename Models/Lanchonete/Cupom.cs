using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class Cupom
    {
        [Key]
        public int CupomId { get; set; }
        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty; // Código do cupom
        [Required]
        [StringLength(50)]
        public string TipoDesconto { get; set; } = string.Empty; // Percentual, Fixo, etc.
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ValorDesconto { get; set; } // Valor do desconto
        public DateTime DataInicio { get; set; } // Data de início da validade do cupom
        public DateTime DataFim { get; set; } // Data de fim da validade do cupom
        public bool Ativo { get; set; } = true; // Indica se o cupom está ativo
        public int? UsosMaximos { get; set; } // Número máximo de usos do cupom
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>(); // Pedidos associados ao cupom

    }
}
