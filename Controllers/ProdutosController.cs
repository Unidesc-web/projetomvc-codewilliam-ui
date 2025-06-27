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

namespace RyujinBites.Controllers
{
    // <--- ADICIONE ESTE ATRIBUTO PARA PROTEGER O CONTROLADOR INTEIRO --->
    [Authorize(Roles = "Administrador")]
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProdutosController> _logger; // Logger para logs

        public ProdutosController(ApplicationDbContext context, ILogger<ProdutosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Produtoes (AÇÃO DE LISTAGEM - PERMANECE COMO VIEW NORMAL)
        public async Task<IActionResult> Index()
        {
            // Incluir a Categoria para exibição na lista
            var applicationDbContext = _context.Produtos.Include(p => p.Categoria);
            return View(await applicationDbContext.ToListAsync());
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES DE CRUD VIA MODAL
        // ---------------------------------------------------------------------------------------------------

        // GET: Produtoes/Details/5 (Detalhes via Modal)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Incluir a Categoria para exibição nos detalhes
            var produto = await _context.Produtos
                                        .Include(p => p.Categoria)
                                        .FirstOrDefaultAsync(m => m.ProdutoId == id);
            if (produto == null) return NotFound();

            return PartialView("_DetailsProdutoModalPartial", produto); // <--- MUDADO PARA PartialView
        }

        // GET: Produtoes/Create (Criar via Modal)
        public IActionResult Create()
        {
            // Popula o dropdown de categorias
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nome");
            return PartialView("_CreateProdutoModalPartial"); // <--- MUDADO PARA PartialView
        }

        // POST: Produtoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProdutoId,Nome,Descricao,Preco,ImagemUrl,Disponivel,Estoque,Estoque,CategoriaId")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Produto '{Nome}' criado com sucesso.", produto.Nome);
                return Json(new { success = true, message = "Produto criado com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na criação de produto. Erros: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Produtoes/Edit/5 (Editar via Modal)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos.FindAsync(id); // FindAsync não inclui Categoria
            if (produto == null) return NotFound();

            // Recarrega o produto com a Categoria incluída para a view de edição
            // Isso garante que o nome da Categoria esteja disponível se a view o exibir
            produto = await _context.Produtos.Include(p => p.Categoria)
                                        .FirstOrDefaultAsync(p => p.ProdutoId == id);

            // Popula o dropdown de categorias
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nome", produto.CategoriaId);
            return PartialView("_EditProdutoModalPartial", produto); // <--- MUDADO PARA PartialView
        }

        // POST: Produtoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProdutoId,Nome,Descricao,Preco,ImagemUrl,Disponivel,Estoque,CategoriaId")] Produto produto)
        {
            if (id != produto.ProdutoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Produto '{Nome}' (ID: {Id}) editado com sucesso.", produto.Nome, produto.ProdutoId);
                    return Json(new { success = true, message = "Produto editado com sucesso!" }); // <--- MUDADO PARA JsonResult
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(produto.ProdutoId))
                    {
                        return Json(new { success = false, message = "Produto não encontrado para edição." }); // <--- MUDADO PARA JsonResult
                    }
                    else
                    {
                        _logger.LogError("Erro de concorrência ao editar produto '{Nome}' (ID: {Id}).", produto.Nome, produto.ProdutoId);
                        throw;
                    }
                }
            }
            _logger.LogWarning("Falha na edição de produto (ID: {Id}). Erros: {Errors}", produto.ProdutoId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            // Recarrega o dropdown de categorias se o modelo não for válido
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "Nome", produto.CategoriaId);
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Produtoes/Delete/5 (Deletar via Modal)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Incluir a Categoria para exibição nos detalhes de deleção
            var produto = await _context.Produtos
                                        .Include(p => p.Categoria)
                                        .FirstOrDefaultAsync(m => m.ProdutoId == id);
            if (produto == null) return NotFound();

            return PartialView("_DeleteProdutoModalPartial", produto); // <--- MUDADO PARA PartialView
        }

        // POST: Produtoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Produto '{Nome}' (ID: {Id}) deletado com sucesso.", produto.Nome, produto.ProdutoId);
                return Json(new { success = true, message = "Produto deletado com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na deleção de produto (ID: {Id}): Produto não encontrado.", id);
            return Json(new { success = false, message = "Produto não encontrado para deleção." }); // <--- MUDADO PARA JsonResult
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.ProdutoId == id);
        }
    }
}