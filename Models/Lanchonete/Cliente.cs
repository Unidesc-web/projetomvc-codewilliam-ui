using RyujinBites.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.Lanchonete
{
    public class Cliente
    {

        [Key]
        public string ClienteId { get; set; } = null!;
        [StringLength(500)]
        public string? Endereco { get; set; }

        [StringLength(100)]
        public string? Complemento { get; set; }

        [StringLength(50)]
        public string? Cidade { get; set; }

        [StringLength(50)]
        public string? Estado { get; set; }

        [StringLength(20)]
        public string? CEP { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
