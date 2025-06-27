using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class Pagamento
    {
        [Key]
        public int PagamentoId { get; set; }
        public int PedidoId { get; set; } // Referência ao Pedido associado
        public Pedido? Pedido { get; set; }
        [Required]
        [StringLength(50)]
        public string MetodoPagamento { get; set; } = string.Empty; // Cartão de Crédito, Boleto, Pix, etc.
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorPago { get; set; } // Valor pago pelo cliente
        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
        [Required]
        [StringLength(50)]
        public string StatusPagamento { get; set; } = string.Empty; // Pendente, Aprovado, Rejeitado, etc.
        [StringLength(100)]
        public string? TransacaoIdExterno { get; set; } // ID da transação, se aplicável (ex: para pagamentos online)
    }
}
