using Microsoft.AspNetCore.Authentication; // Para AuthenticationScheme (login externo)
using Microsoft.AspNetCore.Authorization; // Para o atributo [AllowAnonymous]
using Microsoft.AspNetCore.Identity; // Para classes como UserManager, SignInManager, IdentityRole
using Microsoft.AspNetCore.Identity.UI.Services; // Para a interface IEmailSender
using Microsoft.AspNetCore.Mvc; // Para classes como PageModel, IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages; // Para herdar de PageModel
using Microsoft.AspNetCore.WebUtilities; // Para WebEncoders
using RyujinBites.Data; // Para ApplicationDbContext
using RyujinBites.Models.Identity; // Importa o namespace onde sua classe ApplicationUser está definida
using System.ComponentModel.DataAnnotations; // Para atributos como [Required], [Display], [StringLength]
using System.Text; // Para Encoding.UTF8
using System.Text.Encodings.Web; // Para HtmlEncoder.Default

namespace RyujinBites.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context; // <--- ADICIONADO: Para acessar o DBContext

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context) // <--- ADICIONADO: Injeta o ApplicationDbContext
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context; // <--- ATRIBUÍDO AQUI
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = default!;

            [Required]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; } = default!;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Senha")]
            [Compare("Password", ErrorMessage = "A senha e a senha de confirmação não correspondem.")]
            public string ConfirmPassword { get; set; } = default!;

            [Required]
            [Display(Name = "Nome Completo")]
            public string Nome { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.Nome = Input.Nome;
                user.DataRegistro = DateTime.UtcNow;
                user.EmailConfirmed = true;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!await _roleManager.RoleExistsAsync("Cliente"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Cliente"));
                    }
                    await _userManager.AddToRoleAsync(user, "Cliente");

                    // *** ADICIONE AQUI A CRIAÇÃO DO REGISTRO NA TABELA CLIENTES ***
                    var cliente = new RyujinBites.Models.Lanchonete.Cliente // Use o namespace completo
                    {
                        ClienteId = user.Id, // O ID do Cliente é o mesmo do ApplicationUser
                        // Deixe Endereco, etc., como null/default se não forem preenchidos no registro
                        Endereco = null,
                        Complemento = null,
                        Cidade = null,
                        Estado = null,
                        CEP = null
                    };
                    _context.Clientes.Add(cliente); // Adiciona o novo cliente ao DbSet
                    await _context.SaveChangesAsync(); // Salva no banco de dados (IMPORTANTE!)
                    // *************************************************************

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}