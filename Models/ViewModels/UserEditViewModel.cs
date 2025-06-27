using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.ViewModels
{
    // ViewModel para edição de detalhes do usuário.
    public class UserEditViewModel
    {
        public string Id { get; set; } = null!;

        [Required]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }
        [Required]
        [Display(Name = "Papel do Usuário")]
        public string SelectedRoleName { get; set; } = null!;
    }
}
