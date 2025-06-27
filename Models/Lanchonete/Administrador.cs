using RyujinBites.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.Lanchonete
{
    public class Administrador
    {
        [Key]
        public string AdministradorId { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Cargo { get; set; } = string.Empty; // Ex: Gerente, Supervisor, etc.
        public DateTime DataContratacao { get; set; } = DateTime.UtcNow;
        public ApplicationUser? ApplicationUser { get; set; }

    }
}
