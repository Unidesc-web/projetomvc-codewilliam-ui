using System.ComponentModel.DataAnnotations; // Necessário se você for usar atributos como [Display] ou [Required] aqui.

namespace RyujinBites.Models.ViewModels // <--- ESTE É O NAMESPACE CORRETO PARA ESTA PASTA
{
    public class HomeProductViewModel
    {
        public int Id { get; set; } // ID do produto (corresponde a ProdutoId na entidade Produto)
        public string Nome { get; set; } = string.Empty; // Nome do produto
        public string Descricao { get; set; } = string.Empty; // Descrição curta do produto
        public decimal Preco { get; set; } // Preço do produto
        public string ImagemUrl { get; set; } = string.Empty; // URL da imagem do produto
    }
}