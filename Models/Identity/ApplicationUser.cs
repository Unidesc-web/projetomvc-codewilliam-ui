using Microsoft.AspNetCore.Identity;
using RyujinBites.Models.Lanchonete;

namespace RyujinBites.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? Nome { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
        public Cliente? Cliente { get; set; }
        public Administrador? Administrador { get; set; }

    }
}
 