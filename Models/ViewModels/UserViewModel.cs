namespace RyujinBites.Models.ViewModels
{
    // ViewModel para exibir informações do usuário na lista (Index).
    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string Roles { get; set; } = string.Empty;
    }
}
