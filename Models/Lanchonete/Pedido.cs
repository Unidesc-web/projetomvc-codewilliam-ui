using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }
        public string ClienteId { get; set; } = null!; // ID do cliente que fez o pedido
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;
        [Required]
        [StringLength(50)]
        public string StatusPedido { get; set; } = string.Empty; // Pendente, Em Preparação, Entregue, Cancelado
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorTotal { get; set; }
        [Required]
        [StringLength(50)]
        public string TipoEntrega { get; set; } = string.Empty; // Retirada, Entrega
        [StringLength(500)]
        public string? EnderecoEntrega { get; set; } // Apenas se TipoEntrega for Entrega
        [StringLength(1000)]
        public string? Observacoes { get; set; } // Observações adicionais do cliente
        public int? CupomId { get; set; } // Cupom de desconto, se houver
        [ForeignKey("CupomId")]
        public Cupom? Cupom { get; set; }
        public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
        public Pagamento? Pagamento { get; set; } // Detalhes do pagamento
    }
}
