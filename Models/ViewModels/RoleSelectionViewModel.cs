namespace RyujinBites.Models.ViewModels
{
    // ViewModel para seleção de um papel individual.
    public class RoleSelectionViewModel
    {
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsSelected { get; set; }
    }

}
