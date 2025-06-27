using System.ComponentModel.DataAnnotations;

namespace RyujinBites.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Nome { get; set; } // Pode ser nulo

        [Display(Name = "Papel Atual")]
        public string CurrentRoleName { get; set; } = null!; // Novo: Papel atual do usuário

        [Display(Name = "Novo Papel")] // Novo: O papel para o qual ele será alterado
        public string NewRoleName { get; set; } = null!; // O papel para o qual queremos mudar
    }

    // A classe RoleSelectionViewModel não será mais usada com esta abordagem,
    // você pode deletar o arquivo RoleSelectionViewModel.cs se quiser limpar.
    // public class RoleSelectionViewModel { ... }
}