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
    // Apenas usuários com o papel 'Administrador' podem acessar este controlador.
    [Authorize(Roles = "Administrador")]
    public class CuponsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CuponsController> _logger; // Logger para logs

        public CuponsController(ApplicationDbContext context, ILogger<CuponsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Cupons (AÇÃO DE LISTAGEM - PERMANECE COMO VIEW NORMAL)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cupons.ToListAsync());
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES DE CRUD VIA MODAL
        // ---------------------------------------------------------------------------------------------------

        // GET: Cupons/Details/5 (Detalhes via Modal)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cupom = await _context.Cupons.FirstOrDefaultAsync(m => m.CupomId == id);
            if (cupom == null) return NotFound();

            return PartialView("_DetailsCupomModalPartial", cupom); // <--- MUDADO PARA PartialView
        }

        // GET: Cupons/Create (Criar via Modal)
        public IActionResult Create()
        {
            return PartialView("_CreateCupomModalPartial"); // <--- MUDADO PARA PartialView
        }

        // POST: Cupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CupomId,Codigo,TipoDesconto,ValorDesconto,DataInicio,DataFim,Ativo,UsosMaximos")] Cupom cupom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cupom);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cupom '{Codigo}' criado com sucesso.", cupom.Codigo);
                return Json(new { success = true, message = "Cupom criado com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na criação de cupom. Erros: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Cupons/Edit/5 (Editar via Modal)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null) return NotFound();

            return PartialView("_EditCupomModalPartial", cupom); // <--- MUDADO PARA PartialView
        }

        // POST: Cupons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CupomId,Codigo,TipoDesconto,ValorDesconto,DataInicio,DataFim,Ativo,UsosMaximos")] Cupom cupom)
        {
            if (id != cupom.CupomId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cupom);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cupom '{Codigo}' (ID: {Id}) editado com sucesso.", cupom.Codigo, cupom.CupomId);
                    return Json(new { success = true, message = "Cupom editado com sucesso!" }); // <--- MUDADO PARA JsonResult
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CupomExists(cupom.CupomId))
                    {
                        return Json(new { success = false, message = "Cupom não encontrado para edição." }); // <--- MUDADO PARA JsonResult
                    }
                    else
                    {
                        _logger.LogError("Erro de concorrência ao editar cupom '{Codigo}' (ID: {Id}).", cupom.Codigo, cupom.CupomId);
                        throw;
                    }
                }
            }
            _logger.LogWarning("Falha na edição de cupom (ID: {Id}). Erros: {Errors}", cupom.CupomId, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return Json(new { success = false, message = "Erro de validação. Verifique os campos." }); // <--- MUDADO PARA JsonResult
        }

        // GET: Cupons/Delete/5 (Deletar via Modal)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cupom = await _context.Cupons.FirstOrDefaultAsync(m => m.CupomId == id);
            if (cupom == null) return NotFound();

            return PartialView("_DeleteCupomModalPartial", cupom); // <--- MUDADO PARA PartialView
        }

        // POST: Cupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom != null)
            {
                _context.Cupons.Remove(cupom);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cupom '{Codigo}' (ID: {Id}) deletado com sucesso.", cupom.Codigo, cupom.CupomId);
                return Json(new { success = true, message = "Cupom deletado com sucesso!" }); // <--- MUDADO PARA JsonResult
            }
            _logger.LogWarning("Falha na deleção de cupom (ID: {Id}): Cupom não encontrado.", id);
            return Json(new { success = false, message = "Cupom não encontrado para deleção." }); // <--- MUDADO PARA JsonResult
        }

        private bool CupomExists(int id)
        {
            return _context.Cupons.Any(e => e.CupomId == id);
        }
    }
}