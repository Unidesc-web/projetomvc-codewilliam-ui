using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Data;
using RyujinBites.Models.Lanchonete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using RyujinBites.Models.Identity; // Para ApplicationUser

namespace RyujinBites.Controllers
{ 
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<PedidosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES PARA CLIENTES E ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // GET: Pedidos/MeusPedidos
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> MeusPedidos()
        {
            var userId = _userManager.GetUserId(User);
            var pedidosDoUsuario = await _context.Pedidos
                                                    .Where(p => p.ClienteId == userId)
                                                    .Include(p => p.Cliente).ThenInclude(c => c.ApplicationUser)
                                                    .Include(p => p.Cupom)
                                                    .Include(p => p.ItensPedido).ThenInclude(ip => ip.Produto)
                                                    .ToListAsync();
            ViewData["Title"] = "Meus Pedidos";
            return View("Index", pedidosDoUsuario);
        }

        // POST: Pedidos/AdicionarProdutoAoPedido
        // Adiciona um produto a um pedido existente (ou cria um novo pedido se não houver um "carrinho" ativo).
        [HttpPost]
        [Authorize(Roles = "Cliente, Administrador")] // Apenas usuários logados podem adicionar produtos ao pedido
        // [ValidateAntiForgeryToken] // REMOVIDO: Como pedido, para fins de teste/aprendizado, mas ciente do risco CSRF
        public async Task<IActionResult> AdicionarProdutoAoPedido(int produtoId, int quantidade)
        {
            // 1. Validar a entrada (Reativado e com 'return Json' explícito)
            if (quantidade <= 0)
            {
                _logger.LogWarning("Tentativa de adicionar produto com quantidade inválida ({Quantidade}).", quantidade);
                return Json(new { success = false, message = "A quantidade deve ser maior que zero." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // Este caso não deveria ocorrer com [Authorize], mas é uma validação de segurança extra.
                _logger.LogError("Tentativa de adicionar produto por usuário não autenticado.");
                return Json(new { success = false, message = "Usuário não autenticado." });
            }

            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                _logger.LogWarning("Tentativa de adicionar produto inexistente (ID: {ProdutoId}).", produtoId);
                return Json(new { success = false, message = "Produto não encontrado." });
            }

            // 2. Encontrar ou Criar um Pedido "Ativo" (como um carrinho) para o usuário
            var pedidoAtual = await _context.Pedidos
                                            .Include(p => p.ItensPedido)
                                            .FirstOrDefaultAsync(p => p.ClienteId == userId && p.StatusPedido == "Pendente");

            if (pedidoAtual == null)
            {
                // Se não houver pedido pendente, cria um novo
                pedidoAtual = new Pedido
                {
                    ClienteId = userId,
                    DataPedido = DateTime.UtcNow,
                    StatusPedido = "Pendente",
                    TipoEntrega = "Aguardando Definição",
                    ValorTotal = 0,
                    ItensPedido = new List<ItemPedido>()
                };
                _context.Pedidos.Add(pedidoAtual);
                // *** CORREÇÃO AQUI: ATIVAR O SAVECHANGESASYNC PARA O NOVO PEDIDO ***
                await _context.SaveChangesAsync(); // <-- ISSO É CRUCIAL!
                _logger.LogInformation("Novo pedido (carrinho) criado para o usuário {UserId}.", userId);
            }

            // 3. Adicionar o Produto como ItemPedido
            var itemExistente = pedidoAtual.ItensPedido.FirstOrDefault(ip => ip.ProdutoId == produtoId);
            if (itemExistente != null)
            {
                itemExistente.Quantidade += quantidade;
                itemExistente.PrecoUnitario = produto.Preco;
                _context.Update(itemExistente);
                _logger.LogInformation("Quantidade do item {ProdutoNome} atualizada no pedido {PedidoId}.", produto.Nome, pedidoAtual.PedidoId);
            }
            else
            {
                var novoItem = new ItemPedido
                {
                    PedidoId = pedidoAtual.PedidoId,
                    ProdutoId = produtoId,
                    Quantidade = quantidade,
                    PrecoUnitario = produto.Preco
                };
                pedidoAtual.ItensPedido.Add(novoItem);
                _context.ItensPedido.Add(novoItem);
                _logger.LogInformation("Item {ProdutoNome} adicionado ao pedido {PedidoId}.", produto.Nome, pedidoAtual.PedidoId);
            }

            // 4. Recalcular o ValorTotal do Pedido
            pedidoAtual.ValorTotal = pedidoAtual.ItensPedido.Sum(ip => ip.Quantidade * ip.PrecoUnitario);
            _context.Update(pedidoAtual);

            //await _context.SaveChangesAsync(); // Este SaveChangesAsync final pode estar falhando.
            _logger.LogInformation("Pedido {PedidoId} atualizado. {Quantidade}x {ProdutoNome} adicionado/atualizado. Valor total: {ValorTotal}.",
                                   pedidoAtual.PedidoId, quantidade, produto.Nome, pedidoAtual.ValorTotal);

            // Se tudo der certo, o controller retorna:
            return Json(new { success = true, message = $"{quantidade}x {produto.Nome} adicionado ao seu pedido!", pedidoId = pedidoAtual.PedidoId });
        }


        // ... (restante do seu controlador: Details, Delete, GerenciarPedidos, Edit, PedidoExists) ...
        // Certifique-se que o restante do código está o que definimos anteriormente.
    }
}