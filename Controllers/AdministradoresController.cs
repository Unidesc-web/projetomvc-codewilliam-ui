using Microsoft.AspNetCore.Authorization; // Para o atributo [Authorize]
using Microsoft.AspNetCore.Identity; // Para UserManager, RoleManager
using Microsoft.AspNetCore.Mvc; // Para Controller, IActionResult
using Microsoft.AspNetCore.Mvc.Rendering; // <--- ADICIONADO para SelectList
using Microsoft.EntityFrameworkCore; // Para métodos de extensão do EF Core (ToListAsync)
using RyujinBites.Data; // Seu DbContext
using RyujinBites.Models.Identity; // Sua classe ApplicationUser
using RyujinBites.Models.ViewModels; // Seu namespace para ViewModels

namespace RyujinBites.Controllers
{
    // Apenas usuários com o papel 'Administrador' podem acessar este controlador.
    [Authorize(Roles = "Administrador")]
    public class AdministradoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;       // Para gerenciar usuários
        private readonly RoleManager<IdentityRole> _roleManager;         // Para gerenciar papéis
        private readonly ILogger<AdministradoresController> _logger;     // <--- ADICIONADO: Para registrar logs

        // Construtor: Injeta o contexto do banco de dados, UserManager, RoleManager e ILogger.
        public AdministradoresController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdministradoresController> logger) // <--- ADICIONADO AO CONSTRUTOR
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger; // <--- ATRIBUÍDO AQUI
        }

        // GET: Administradores (Lista todos os usuários cadastrados)
        public async Task<IActionResult> Index()
        {
            // Obtém todos os usuários do sistema.
            var users = await _userManager.Users.ToListAsync();

            // Cria um ViewModel para exibir as informações necessárias na view,
            // incluindo os papéis de cada usuário.
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Obtém os papéis do usuário
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Nome = user.Nome,
                    Email = user.Email,
                    Telefone = user.PhoneNumber, // Usando a propriedade PhoneNumber do IdentityUser
                    Roles = string.Join(", ", roles) // Concatena os papéis em uma string
                });
            }
            return View(userViewModels); // Retorna a view com a lista de usuários.
        }

        // -------------------------------------------------------------------------
        // AÇÃO: CRIAR NOVO USUÁRIO E ATRIBUIR PAPEL INICIAL
        // -------------------------------------------------------------------------

        // GET: Administradores/CreateUser
        // Exibe o formulário para criar um novo usuário.
        public IActionResult CreateUser()
        {
            // Popula um SelectList com todos os papéis disponíveis para o dropdown na view.
            ViewData["AllRoles"] = new SelectList(_roleManager.Roles.OrderBy(r => r.Name), "Name", "Name");
            return View();
        }

        // POST: Administradores/CreateUser
        // Processa a submissão do formulário para criar um novo usuário.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            // Popula os papéis novamente caso haja erro de validação e a view precise ser recarregada.
            ViewData["AllRoles"] = new SelectList(_roleManager.Roles.OrderBy(r => r.Name), "Name", "Name");

            if (ModelState.IsValid)
            {
                // Cria uma nova instância de ApplicationUser com os dados do formulário.
                var user = new ApplicationUser
                {
                    UserName = model.Email, // UserName é geralmente o email.
                    Email = model.Email,
                    Nome = model.Nome,
                    PhoneNumber = model.Telefone, // Atribui o telefone.
                    DataRegistro = DateTime.UtcNow,
                    EmailConfirmed = true // Para facilitar o teste em dev, pode ser false e exigir confirmação.
                };

                // Tenta criar o usuário com a senha fornecida.
                var result = await _userManager.CreateAsync(user, model.Senha);

                if (result.Succeeded)
                {
                    // ... atribui o papel selecionado ...
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    // *** ADICIONE AQUI A CRIAÇÃO DO REGISTRO NA TABELA CLIENTES ***
                    // Se o papel selecionado for 'Cliente', ou se ele for um usuário que fará pedidos/avaliações
                    if (model.SelectedRole == "Cliente") // Ou outro critério
                    {
                        var cliente = new RyujinBites.Models.Lanchonete.Cliente
                        {
                            ClienteId = user.Id,
                            Endereco = null, // Preencha se aplicável
                            Complemento = null,
                            Cidade = null,
                            Estado = null,
                            CEP = null
                        };
                        _context.Clientes.Add(cliente);
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation("Novo usuário criado por administrador.");
                    return RedirectToAction(nameof(Index));
                }

                // Se a criação do usuário falhar, adiciona os erros ao ModelState.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model); // Retorna a view com o modelo para exibir erros de validação.
        }

        // -------------------------------------------------------------------------
        // AÇÃO: EDITAR DETALHES DO USUÁRIO
        // -------------------------------------------------------------------------

        // GET: Administradores/EditUser/userId
        // Exibe o formulário para editar detalhes de um usuário.
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id); // Busca o usuário
            if (user == null) return NotFound();

            // Obtém o papel(is) atual(is) do usuário
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault(); // Pega o primeiro papel (considerando um papel principal)

            // Obtém todos os papéis disponíveis para o dropdown
            var allRoles = await _roleManager.Roles.ToListAsync();

            // Popula o SelectList para o dropdown de papéis
            ViewData["AllRoles"] = new SelectList(allRoles.OrderBy(r => r.Name), "Name", "Name", currentRole);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Nome = user.Nome,
                Email = user.Email,
                Telefone = user.PhoneNumber,
                SelectedRoleName = currentRole // Preenche o papel atual no ViewModel
            };
            return View(model);
        }

        // POST: Administradores/EditUser/userId
        // Processa a submissão do formulário para editar detalhes do usuário e seu papel.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            // Re-popula o SelectList caso o ModelState não seja válido e a view precise ser recarregada.
            var allRolesForViewData = await _roleManager.Roles.ToListAsync();
            ViewData["AllRoles"] = new SelectList(allRolesForViewData.OrderBy(r => r.Name), "Name", "Name", model.SelectedRoleName);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // ** ATUALIZAÇÃO DE DETALHES DO USUÁRIO **
            user.Nome = model.Nome;
            user.PhoneNumber = model.Telefone;

            if (user.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Erro ao alterar o e-mail.");
                    return View(model);
                }
                user.UserName = model.Email;
                user.EmailConfirmed = false;
            }

            var updateResult = await _userManager.UpdateAsync(user); // Salva as alterações nos detalhes do usuário.

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // ** ATUALIZAÇÃO DE PAPÉIS DO USUÁRIO **
            var currentRoles = await _userManager.GetRolesAsync(user);
            // Remove todos os papéis existentes do usuário
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Adiciona o novo papel selecionado (se um foi selecionado)
            if (!string.IsNullOrEmpty(model.SelectedRoleName))
            {
                if (!await _roleManager.RoleExistsAsync(model.SelectedRoleName))
                {
                    ModelState.AddModelError(string.Empty, $"Erro: O papel '{model.SelectedRoleName}' não foi encontrado. Contate o suporte.");
                    // Você pode querer re-adicionar os papéis antigos aqui se o novo não for válido.
                    return View(model);
                }
                await _userManager.AddToRoleAsync(user, model.SelectedRoleName);
            }
            else // Se "Nenhum" foi selecionado ou string vazia
            {
                _logger.LogWarning("Administrador {AdminId} removeu todos os papéis do usuário {UserId}.", _userManager.GetUserId(User), model.Id);
            }

            _logger.LogInformation("Detalhes e papéis do usuário {UserId} atualizados para '{NewRole}' por administrador {AdminId}.",
                                    model.Id, model.SelectedRoleName, _userManager.GetUserId(User));

            return RedirectToAction(nameof(Index)); // Redireciona para a lista de usuários.
        }


        // -------------------------------------------------------------------------
        // AÇÃO: DELETAR USUÁRIO
        // -------------------------------------------------------------------------

        // POST: Administradores/DeleteConfirmed/userId
        // Processa a deleção confirmada de um usuário.
        [HttpPost, ActionName("Delete")] // Mapeia para a URL Delete, mas usa ActionName para a ação real.
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        // CORREÇÃO: Altere o tipo de retorno para JsonResult
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // Se o usuário não for encontrado, retorna falha.
                return Json(new { success = false, message = "Usuário não encontrado." });
            }

            // ADIÇÃO: Verifica se o usuário tentando deletar é o próprio admin logado.
            // Isso evita que um admin se auto-delete acidentalmente ou que um superadmin seja removido indevidamente.
            if (id == _userManager.GetUserId(User))
            {
                return Json(new { success = false, message = "Você não pode deletar sua própria conta através desta interface." });
            }

            // Deleta o usuário do sistema.
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário {UserId} deletado por administrador {AdminId}.", id, _userManager.GetUserId(User));
                // Retorna sucesso para o AJAX.
                return Json(new { success = true, message = "Usuário deletado com sucesso!" });
            }
            else
            {
                // Retorna falha e a descrição dos erros para o AJAX.
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Falha ao deletar usuário {UserId}: {Errors}", id, errors);
                return Json(new { success = false, message = "Erro ao deletar: " + errors });
            }
        }

        // REMOVA O MÉTODO GET: Administradores/Delete/userId, pois não será mais usado diretamente.
        // Se você mantiver o GET Delete, ele ainda funcionará como uma página separada, mas o modal o ignora.
        // Se remover, garanta que nenhum link direto a ele exista (o que não deve, pois o Index agora usa o modal).
    }
}