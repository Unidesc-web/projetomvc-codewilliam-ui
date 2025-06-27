using RyujinBites.Models.Lanchonete; // Para usar Produto, Pedido (para PopularDish, OrderItem)
using System.Collections.Generic;
using System;

namespace RyujinBites.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        // --- Seção de Boas-Vindas ---
        public string WelcomeMessage { get; set; } = "Bem-vindo(a)!";
        public string WelcomeDescription { get; set; } = "Aproveite seus pedidos com nosso painel de gerenciamento de alimentos.";
        public string UserDisplayName { get; set; } = "Usuário";

        // --- Seção de Análise ---
        public int PedidosConcluidosCount { get; set; }
        public decimal PedidosConcluidosPercent { get; set; }
        public int PedidosEmProcessamentoCount { get; set; }
        public decimal PedidosEmProcessamentoPercent { get; set; }
        public int TotalClientesCount { get; set; }
        public decimal TotalClientesPercent { get; set; }

        // --- Seção de Pedido em Preparação (Resumo do Último Pedido Pendente) ---
        public int? LatestOrderId { get; set; }
        public DateTime? LatestOrderDate { get; set; }
        public string? LatestOrderStatus { get; set; }
        public List<OrderItemViewModel> LatestOrderItems { get; set; } = new List<OrderItemViewModel>();
        public string? DeliveryAddress { get; set; } // Endereço de entrega do pedido
        public string? DeliveryCityState { get; set; } // Cidade e Estado
        public decimal TotalAmountPending { get; set; }

        // --- Seção de Pratos Populares ---
        public List<PopularDishViewModel> PopularDishes { get; set; } = [];

        // --- ViewModel aninhado para Itens do Pedido (simplificado) ---
        public class OrderItemViewModel
        {
            public string ProductName { get; set; } = string.Empty;
            public string ProductType { get; set; } = string.Empty; // Ex: Prato Principal, Entrada
            public decimal Price { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }

        // --- ViewModel aninhado para Pratos Populares ---
        public class PopularDishViewModel
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }
    }
} 