using Microsoft.AspNetCore.Authorization; // Para [AllowAnonymous], [Authorize], [Authorize(Roles)]
using Microsoft.AspNetCore.Identity;     // Para UserManager<ApplicationUser>
using Microsoft.AspNetCore.Mvc;          // Para Controller, IActionResult
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectList
using Microsoft.EntityFrameworkCore;      // Para métodos do EF Core (Include, ToListAsync)
using RyujinBites.Data;                   // Seu DbContext
using RyujinBites.Models.Identity;        // Sua classe ApplicationUser
using RyujinBites.Models.Lanchonete;      // Suas classes de modelo (Avaliacao, Produto, Cliente)

namespace RyujinBites.Controllers
{
    // Este controlador não terá um [Authorize] no nível da classe, pois algumas ações são públicas.
    public class AvaliacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Injetar UserManager para verificar o usuário logado

        // Construtor
        public AvaliacoesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES DE VISUALIZAÇÃO (Públicas ou para todos logados)
        // ---------------------------------------------------------------------------------------------------

        // GET: Avaliacaos (Lista todas as avaliações para visualização pública)
        // Permite que qualquer um (logado ou não) veja as avaliações.
        [AllowAnonymous] // Permite acesso público
        public async Task<IActionResult> Index()
        {
            // --- CORREÇÃO AQUI: Adicionar .Include(a => a.Cliente.ApplicationUser) ---
            var applicationDbContext = _context.Avaliacoes
                                                .Include(a => a.Cliente)
                                                .Include(a => a.Cliente.ApplicationUser) // <--- ADICIONE ESTA LINHA
                                                .Include(a => a.Produto);
            // -------------------------------------------------------------------------
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Avaliacaos/Details/5 (Detalhes de uma avaliação específica)
        // Permite que qualquer um (logado ou não) veja os detalhes de uma avaliação.
        [AllowAnonymous] // Permite acesso público
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Cliente)
                .Include(a => a.Produto)
                .FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null)
            {
                return NotFound();
            }

            return View(avaliacao);
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES PARA USUÁRIOS LOGADOS (Clientes e Administradores)
        // ---------------------------------------------------------------------------------------------------

        // GET: Avaliacaos/Create (Criar uma nova avaliação)
        // Apenas usuários logados podem criar avaliações.
        [Authorize(Roles = "Cliente, Administrador")] // Clientes e Administradores podem criar.
        public IActionResult Create()
        {
            // Popula dropdowns para ProdutoId. ClienteId será preenchido automaticamente.
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome");
            return View();
        }

        // POST: Avaliacaos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente, Administrador")]
        // Importante: Removi "DataAvaliacao" do [Bind], pois ela é preenchida automaticamente.
        public async Task<IActionResult> Create([Bind("AvaliacaoId,ProdutoId,Pontuacao,Comentario")] Avaliacao avaliacao)
        {

            var userId = _userManager.GetUserId(User)!;
            avaliacao.ClienteId = userId; // Preenche o ClienteId do usuário logado
            avaliacao.DataAvaliacao = DateTime.UtcNow; // Preenche a data de criação
            avaliacao.IsReported = false; // Define como não denunciada por padrão na criação
            avaliacao.Status = "Pendente"; // Define o status inicial 

            // Remove o erro de validação do ClienteId do ModelState ***
            ModelState.Remove("ClienteId");

            // Se estiver usando o campo Status e não houver um campo no formulário:
            ModelState.Remove("Status"); // <--- ADICIONE ESTA LINHA (se Status não vem do formulário)

            // Se estiver usando o campo IsReported e não houver um campo no formulário:
            ModelState.Remove("IsReported");

            if (ModelState.IsValid)
            {  // Agora, ClienteId, DataAvaliacao, IsReported e Status já têm valores.

                _context.Add(avaliacao);
                await _context.SaveChangesAsync();
                // Redireciona para a Index após a criação (ou para uma página de sucesso)
                return RedirectToAction(nameof(Index));
            }

            // Se o ModelState.IsValid ainda for false (por outros motivos), recarrega dropdowns.
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome", avaliacao.ProdutoId);
            return View(avaliacao);
        }


        // GET: Avaliacaos/Edit/5 (Editar uma avaliação existente)
        // Permite editar APENAS a própria avaliação ou se for Administrador.
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // --- CORREÇÃO AQUI: Use FirstOrDefaultAsync com Include ---
            // Carrega a avaliação E inclui os dados do Produto, Cliente, e ApplicationUser do Cliente
            var avaliacao = await _context.Avaliacoes
                                            .Include(a => a.Produto) // Carrega o Produto avaliado
                                            .Include(a => a.Cliente) // Carrega o Cliente que fez a avaliação
                                            .Include(a => a.Cliente.ApplicationUser) // Carrega o ApplicationUser do Cliente (para obter o Nome/Email)
                                            .FirstOrDefaultAsync(a => a.AvaliacaoId == id); // Busca pelo ID

            if (avaliacao == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Permissão: Apenas o dono da avaliação OU um Administrador pode editar.
            if (avaliacao.ClienteId != userId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            // ViewData["ProdutoId"] não é mais necessário aqui, pois o nome será exibido diretamente na view.
            return View(avaliacao);
        }

        // POST: Avaliacaos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("AvaliacaoId,ProdutoId,ClienteId,Pontuacao,Comentario,DataAvaliacao")] Avaliacao avaliacao)
        {
            if (id != avaliacao.AvaliacaoId) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Permissão: Apenas o dono da avaliação OU um Administrador pode editar.
            // Re-verifica no POST para segurança.
            if (avaliacao.ClienteId != userId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Se o cliente não é admin, não deixe ele mudar o ClienteId ou DataAvaliacao manualmente no POST
                    if (!User.IsInRole("Administrador"))
                    {
                        var originalAvaliacao = await _context.Avaliacoes.AsNoTracking().FirstOrDefaultAsync(a => a.AvaliacaoId == id);
                        if (originalAvaliacao != null)
                        {
                            avaliacao.ClienteId = originalAvaliacao.ClienteId; // Impede mudança de dono
                            avaliacao.DataAvaliacao = originalAvaliacao.DataAvaliacao; // Impede mudança da data original
                        }
                    }

                    _context.Update(avaliacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvaliacaoExists(avaliacao.AvaliacaoId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "ProdutoId", "Nome", avaliacao.ProdutoId);
            return View(avaliacao);
        }

        // GET: Avaliacaos/Delete/5 (Deletar uma avaliação existente)
        // Permite deletar APENAS a própria avaliação ou se for Administrador.
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Cliente)              // <--- Já tem esta linha
                .Include(a => a.Cliente.ApplicationUser) // <--- ADICIONE ESTA LINHA para carregar o nome do usuário
                .Include(a => a.Produto)              // <--- Já tem esta linha
                .FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Permissão: Apenas o dono da avaliação OU um Administrador pode deletar.
            if (avaliacao.ClienteId != userId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }
            return View(avaliacao);
        }

        // POST: Avaliacaos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente, Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return NotFound(); // Se não encontrar, pode ter sido deletado por outro processo

            var userId = _userManager.GetUserId(User);

            // Permissão: Apenas o dono da avaliação OU um Administrador pode deletar.
            // Re-verifica no POST para segurança.
            if (avaliacao.ClienteId != userId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AÇÃO: Usuário logado denuncia uma avaliação.
        [Authorize(Roles = "Cliente, Administrador")]
        [HttpPost]
        public async Task<IActionResult> Denunciar(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return Json(new { success = false, message = "Avaliação não encontrada." });

            // Impede que um usuário denuncie sua própria avaliação ou avaliações já denunciadas
            if (avaliacao.ClienteId == _userManager.GetUserId(User) || avaliacao.IsReported)
            {
                return Json(new { success = false, message = "Você não pode denunciar sua própria avaliação ou esta avaliação já foi denunciada." });
            }

            avaliacao.IsReported = true; // Marca como denunciada
            _context.Update(avaliacao);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Avaliação denunciada com sucesso! Ela será revisada por um administrador." });
        }

        // ---------------------------------------------------------------------------------------------------
        // AÇÕES EXCLUSIVAS PARA ADMINISTRADORES
        // ---------------------------------------------------------------------------------------------------

        // AÇÃO: Administrador aceitar/rejeitar comentário (se for uma funcionalidade de moderação)
        // Isso pode ser um método POST chamado via AJAX de uma lista de avaliações pendentes.
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ModerarAvaliacao(int id, string novoStatus) // Recebe o novo status
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return Json(new { success = false, message = "Avaliação não encontrada." });

            avaliacao.Status = novoStatus; // Atualiza o status
            _context.Update(avaliacao);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Avaliação atualizada para '{novoStatus}' com sucesso!" });
        }

        // AÇÃO: Administrador gerenciar avaliações denunciadas (remover/manter)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Denuncias()
        {
            // Retorna as avaliações que foram marcadas como denunciadas.
            var avaliacoesDenunciadas = await _context.Avaliacoes
                                                        .Where(a => a.IsReported) // Filtra por IsReported
                                                        .Include(a => a.Cliente)  // Inclui o cliente que fez a avaliação
                                                        .Include(a => a.Produto)  // Inclui o produto avaliado
                                                        .ToListAsync();
            return View(avaliacoesDenunciadas); // Retorna uma view específica para Denúncias
        }

        // AÇÃO: Administrador remover uma avaliação denunciada (POST)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> RemoverAvaliacaoDenunciada(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return Json(new { success = false, message = "Avaliação não encontrada." });

            // Deletar a avaliação
            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Avaliação removida com sucesso!" });
        }

        // AÇÃO: Administrador manter uma avaliação denunciada (POST) (remover o status de denunciado)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> ManterAvaliacaoDenunciada(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return Json(new { success = false, message = "Avaliação não encontrada." });

            // Supondo que você tenha um campo "IsReported"
            // avaliacao.IsReported = false; // Define que não está mais denunciada
            // _context.Update(avaliacao); // Marca para atualização
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Status de denúncia da avaliação removido!" });
        }

        // Método auxiliar para verificar a existência de uma avaliação.
        private bool AvaliacaoExists(int id)
        {
            return _context.Avaliacoes.Any(e => e.AvaliacaoId == id);
        }
    }
}