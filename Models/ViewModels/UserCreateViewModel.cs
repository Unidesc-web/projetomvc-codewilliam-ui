using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.ViewModels
{
    // ViewModel para criação de novo usuário pelo administrador.
    public class UserCreateViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Senha", ErrorMessage = "A senha e a confirmação de senha não correspondem.")]
        public string ConfirmarSenha { get; set; } = default!;

        [Required]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Required]
        [Display(Name = "Papel Inicial")]
        public string SelectedRole { get; set; } = string.Empty; // Papel selecionado no dropdown
    }
}
