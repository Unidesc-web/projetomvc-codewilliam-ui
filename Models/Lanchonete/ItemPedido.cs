using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RyujinBites.Models.Lanchonete
{
    public class ItemPedido
    {
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }
        [ForeignKey("ProdutoId")]
        public Produto? Produto { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; } = 1;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoUnitario { get; set; } // Preço do produto no momento do pedido
    }
}
