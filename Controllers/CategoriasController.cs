using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Data;
using RyujinBites.Models.Lanchonete;

namespace RyujinBites.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriasController> _logger; // Opcional, mas bom para logs de sucesso/erro

        public CategoriasController(ApplicationDbContext context, ILogger<CategoriasController> logger)
        {
            _context = context;
            _logger = logger; // Atribuir
        }

        // GET: Categorias (AÇÃO DE LISTAGEM - PERMANECE COMO VIEW NORMAL)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES DE CRUD VIA MODAL
        // ---------------------------------------------------------------------------------------------------

        // GET: Categorias/Details/5 (Detalhes via Modal)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsCategoriaModalPartial", categoria); // <--- MUDADO PARA PartialView
        }

        // GET: Categorias/Create (Criar via Modal)
        public IActionResult Create()
        {
            return PartialView("_CreateCategoriaModalPartial"); // <--- MUDADO PARA PartialView
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaId,Nome,Descricao")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Categoria '{Nome}' criada com sucesso.", categoria.Nome);
                return Json(new { success = true, message = "Categoria criada com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na criação de categoria. Erros: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Categorias/Edit/5 (Editar via Modal)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return PartialView("_EditCategoriaModalPartial", categoria); // <--- MUDADO PARA PartialView
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nome,Descricao")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Categoria '{Nome}' (ID: {Id}) editada com sucesso.", categoria.Nome, categoria.CategoriaId);
                    return Json(new { success = true, message = "Categoria editada com sucesso!" }); // <--- MUDADO PARA JsonResult
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.CategoriaId))
                    {
                        return Json(new { success = false, message = "Categoria não encontrada para edição." }); // <--- MUDADO PARA JsonResult
                    }
                    else
                    {
                        _logger.LogError("Erro de concorrência ao editar categoria '{Nome}' (ID: {Id}).", categoria.Nome, categoria.CategoriaId);
                        throw; // Ainda lança para depuração se não for not found
                    }
                }
            }
            _logger.LogWarning("Falha na edição de categoria (ID: {Id}). Erros: {Errors}", categoria.CategoriaId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Categorias/Delete/5 (Deletar via Modal)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.CategoriaId == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteCategoriaModalPartial", categoria); // <--- MUDADO PARA PartialView
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Categoria '{Nome}' (ID: {Id}) deletada com sucesso.", categoria.Nome, categoria.CategoriaId);
                return Json(new { success = true, message = "Categoria deletada com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na deleção de categoria (ID: {Id}): Categoria não encontrada.", id);
            return Json(new { success = false, message = "Categoria não encontrada para deleção." }); // <--- MUDADO PARA JsonResult
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id);
        }
    }
}