using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Data;
using RyujinBites.Models.Lanchonete;
using Microsoft.AspNetCore.Authorization; // Adicione esta linha
using Microsoft.Extensions.Logging;       // Adicione esta linha
using Microsoft.AspNetCore.Identity;       // Adicione esta linha (para UserManager)
using RyujinBites.Models.Identity;        // Adicione esta linha (para ApplicationUser)

namespace RyujinBites.Controllers
{
    // NOTA: Não colocaremos [Authorize] no nível da classe para permitir acesso misto
    public class PagamentosController : Controller // Lembre-se de renomear o arquivo e a pasta Views para 'PagamentosController.cs'
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PagamentosController> _logger;
        private readonly UserManager<ApplicationUser> _userManager; // Injetado para obter o usuário logado

        public PagamentosController(ApplicationDbContext context, ILogger<PagamentosController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager; // Atribua o UserManager
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES PARA CLIENTES E ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // GET: Pagamentos/MeusPagamentos
        // Permite que usuários com papel "Cliente" ou "Administrador" vejam seus próprios pagamentos.
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> MeusPagamentos()
        {
            var userId = _userManager.GetUserId(User); // Obtém o ID do usuário logado

            // Filtra os pagamentos para mostrar apenas aqueles pertencentes ao usuário logado,
            // e inclui o Pedido associado para exibir informações.
            var pagamentosDoUsuario = await _context.Pagamentos
                                                    .Where(p => p.Pedido != null && p.Pedido.ClienteId == userId) // Filtra por Pedido.ClienteId
                                                    .Include(p => p.Pedido)
                                                    .ToListAsync();
            return View("Index", pagamentosDoUsuario); // Reutiliza a view 'Index' para exibir 'Meus Pagamentos'
        }

        // GET: Pagamentos/Details/5 (Detalhes via Modal para Cliente/Admin)
        // Permite que o dono do pagamento OU um Administrador vejam os detalhes.
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pagamento = await _context.Pagamentos
                                        .Include(p => p.Pedido)
                                        .Include(p => p.Pedido.Cliente) // Incluir Cliente para verificar o dono
                                        .FirstOrDefaultAsync(m => m.PagamentoId == id);
            if (pagamento == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            // Se não for Administrador E o pagamento não for do usuário logado, nega acesso.
            if (!User.IsInRole("Administrador") && pagamento.Pedido?.ClienteId != userId)
            {
                return Forbid();
            }

            // Retorna uma PartialView se você quiser que os detalhes apareçam em um modal.
            // Se não for modal, retorne View("Details", pagamento);
            return PartialView("_DetailsPagamentoModalPartial", pagamento); // Mudado para PartialView
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES EXCLUSIVAS PARA ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // GET: Pagamentos/GerenciarPagamentos
        // Permite que apenas usuários com papel "Administrador" vejam TODOS os pagamentos no sistema.
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GerenciarPagamentos()
        {
            // Retorna todos os pagamentos no sistema, incluindo o Pedido associado.
            var todosOsPagamentos = await _context.Pagamentos
                                                    .Include(p => p.Pedido)
                                                    .Include(p => p.Pedido.Cliente) // Opcional: para ver o Cliente.Id na lista
                                                    .Include(p => p.Pedido.Cliente.ApplicationUser) // Opcional: para ver o Nome do Cliente
                                                    .ToListAsync();
            return View("Index", todosOsPagamentos); // Reutiliza a view 'Index' para exibir todos os pagamentos
        }

        // GET: Pagamentos/Create (Criar via Modal - Admin Only)
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            // Popula o dropdown de Pedidos para associar um pagamento
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "PedidoId"); // Pode querer mostrar PedidoId e um resumo
            return PartialView("_CreatePagamentoModalPartial"); // Mudado para PartialView
        }

        // POST: Pagamentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("PagamentoId,PedidoId,MetodoPagamento,ValorPago,DataPagamento,StatusPagamento,TransacaoIdExterno")] Pagamento pagamento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagamento);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Pagamento ID {PagamentoId} criado com sucesso.", pagamento.PagamentoId);
                return Json(new { success = true, message = "Pagamento criado com sucesso!" });
            }
            _logger.LogWarning("Falha na criação de pagamento. Erros: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." });
        }

        // GET: Pagamentos/Edit/5 (Editar via Modal - Admin Only)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento == null) return NotFound();

            // Incluir Pedido para exibição no formulário
            pagamento = await _context.Pagamentos.Include(p => p.Pedido)
                                                .FirstOrDefaultAsync(p => p.PagamentoId == id);

            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "PedidoId", pagamento.PedidoId);
            return PartialView("_EditPagamentoModalPartial", pagamento); // Mudado para PartialView
        }

        // POST: Pagamentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("PagamentoId,PedidoId,MetodoPagamento,ValorPago,DataPagamento,StatusPagamento,TransacaoIdExterno")] Pagamento pagamento)
        {
            if (id != pagamento.PagamentoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagamento);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Pagamento ID {PagamentoId} editado com sucesso.", pagamento.PagamentoId);
                    return Json(new { success = true, message = "Pagamento editado com sucesso!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagamentoExists(pagamento.PagamentoId)) return Json(new { success = false, message = "Pagamento não encontrado para edição." });
                    else { _logger.LogError("Erro de concorrência ao editar pagamento ID {Id}.", pagamento.PagamentoId); throw; }
                }
            }
            _logger.LogWarning("Falha na edição de pagamento (ID: {Id}). Erros: {Errors}", pagamento.PagamentoId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "PedidoId", pagamento.PedidoId);
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." });
        }

        // GET: Pagamentos/Delete/5 (Deletar via Modal - Admin Only)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pagamento = await _context.Pagamentos
                                        .Include(p => p.Pedido)
                                        .FirstOrDefaultAsync(m => m.PagamentoId == id);
            if (pagamento == null) return NotFound();

            return PartialView("_DeletePagamentoModalPartial", pagamento); // Mudado para PartialView
        }

        // POST: Pagamentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagamento = await _context.Pagamentos.FindAsync(id);
            if (pagamento != null)
            {
                _context.Pagamentos.Remove(pagamento);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Pagamento ID {PagamentoId} deletado com sucesso.", pagamento.PagamentoId);
                return Json(new { success = true, message = "Pagamento deletado com sucesso!" });
            }
            _logger.LogWarning("Falha na deleção de pagamento (ID: {Id}): Pagamento não encontrado.", id);
            return Json(new { success = false, message = "Pagamento não encontrado para deleção." });
        }

        private bool PagamentoExists(int id)
        {
            return _context.Pagamentos.Any(e => e.PagamentoId == id);
        }
    }
}