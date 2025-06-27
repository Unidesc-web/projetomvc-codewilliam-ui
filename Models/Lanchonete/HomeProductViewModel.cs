using System.ComponentModel.DataAnnotations; // Usado para atributos como [Display], [Required] se você os adicionar.

namespace RyujinBites.Models.Lanchonete // <--- GARANTA QUE ESTE NAMESPACE ESTÁ CORRETO
{
    public class HomeProductViewModel
    {
        public int Id { get; set; } // ID do produto
        public string Nome { get; set; } = string.Empty; // Nome do produto
        public string Descricao { get; set; } = string.Empty; // Descrição curta
        public decimal Preco { get; set; } // Preço do produto
        public string ImagemUrl { get; set; } = string.Empty; // URL da imagem
    }
}