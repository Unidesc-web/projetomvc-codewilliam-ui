using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RyujinBites.Data;
using RyujinBites.Models;
using RyujinBites.Models.Identity;
using RyujinBites.Models.ViewModels; // <-- NOVO: Para HomeDashboardViewModel
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RyujinBites.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // *** DADOS MOCKADOS PARA O DASHBOARD ***
            var model = new HomeDashboardViewModel();

            // Seção de Boas-Vindas
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                model.UserDisplayName = currentUser?.Nome ?? currentUser?.Email ?? "Usuário Logado";
                model.WelcomeMessage = $"Bem-vindo(a), {model.UserDisplayName}!";
            }

            // Seção de Análise (dados estáticos por enquanto)
            model.PedidosConcluidosCount = 156;
            model.PedidosConcluidosPercent = 15.6m;
            model.PedidosEmProcessamentoCount = 80;
            model.PedidosEmProcessamentoPercent = 10.2m;
            model.TotalClientesCount = 500;
            model.TotalClientesPercent = 5.0m;

            // Seção de Pedido em Preparação (dados mockados ou buscar o último pendente)
            // Para simplificar, vamos mockar um ou buscar um real se existir.
            var latestOrder = await _context.Pedidos
                                            .Where(p => p.ClienteId == _userManager.GetUserId(User) && p.StatusPedido == "Pendente")
                                            .Include(p => p.ItensPedido).ThenInclude(ip => ip.Produto)
                                            .OrderByDescending(p => p.DataPedido)
                                            .FirstOrDefaultAsync();
            if (latestOrder != null)
            {
                model.LatestOrderId = latestOrder.PedidoId;
                model.LatestOrderDate = latestOrder.DataPedido;
                model.LatestOrderStatus = latestOrder.StatusPedido;
                model.DeliveryAddress = latestOrder.EnderecoEntrega;
                model.DeliveryCityState = $"{latestOrder.Cliente?.Cidade}, {latestOrder.Cliente?.Estado}"; // Precisa incluir Cliente no Pedido
                model.TotalAmountPending = latestOrder.ValorTotal;

                model.LatestOrderItems = latestOrder.ItensPedido.Select(ip => new HomeDashboardViewModel.OrderItemViewModel
                {
                    ProductName = ip.Produto?.Nome ?? "Produto Desconhecido",
                    ProductType = ip.Produto?.Categoria?.Nome ?? "Categoria", // Precisa incluir Categoria no Produto
                    Price = ip.PrecoUnitario * ip.Quantidade,
                    ImageUrl = ip.Produto?.ImagemUrl ?? "https://via.placeholder.com/60x60?text=Item"
                }).ToList();
            }
            else
            {
                // Dados mockados se não houver pedido pendente
                model.LatestOrderItems = new List<HomeDashboardViewModel.OrderItemViewModel>
                {
                    new HomeDashboardViewModel.OrderItemViewModel { ProductName = "Nenhum Pedido Pendente", ProductType = "", Price = 0, ImageUrl = "" }
                };
            }


            // Seção de Pratos Populares (mockados, ou buscar produtos mais vendidos)
            model.PopularDishes = new List<HomeDashboardViewModel.PopularDishViewModel>
            {
                new HomeDashboardViewModel.PopularDishViewModel
                {
                    Name = "X-Dragão Supremo", Category = "Lanches", Price = 45.90m, ImageUrl = "https://via.placeholder.com/150x100?text=Prato1"
                },
                new HomeDashboardViewModel.PopularDishViewModel
                {
                    Name = "Batata Frita da Fúria", Category = "Acompanhamentos", Price = 18.50m, ImageUrl = "https://via.placeholder.com/150x100?text=Prato2"
                },
                new HomeDashboardViewModel.PopularDishViewModel
                {
                    Name = "Poção do Gelo Azul", Category = "Bebidas", Price = 12.00m, ImageUrl = "https://via.placeholder.com/150x100?text=Prato3"
                }
            };
            // *** FIM DOS DADOS PARA O DASHBOARD ***

            return View(model); // Passa o HomeDashboardViewModel para a view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}  