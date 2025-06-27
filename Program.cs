using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Data;
using RyujinBites.Data.Seed; // O namespace da sua nova classe DbInitializer
using RyujinBites.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Habilita a página de exceção para desenvolvedores do banco de dados (útil em dev)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuração do ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Habilita o gerenciamento de roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Adiciona suporte para controladores com views (para o seu MVC)
builder.Services.AddControllersWithViews();

// Adiciona suporte para Razor Pages (necessário para a UI do Identity)
builder.Services.AddRazorPages(); // <--- ADICIONADO AQUI


var app = builder.Build(); // Constrói a aplicação web


// *** BLOCO PARA INICIALIZAR O BANCO DE DADOS E OS PAPÉIS/USUÁRIOS (SEEDING) ***
// Este bloco deve ser executado antes da configuração do pipeline HTTP.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Chama o método Initialize da sua classe DbInitializer
        await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        // Captura e loga quaisquer erros que ocorram durante o seeding
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Um erro ocorreu ao seedar o banco de dados.");
    }
}
// *******************************************************************


// Configure the HTTP request pipeline. (Configura o pipeline de requisições HTTP)
if (app.Environment.IsDevelopment())
{
    // Habilita a página de exceção detalhada para desenvolvedores (em ambiente de desenvolvimento)
    app.UseDeveloperExceptionPage();
    // Exibe a página de erros de migração (para problemas de DB em dev)
    app.UseMigrationsEndPoint();
}
else
{
    // Em produção, usa uma página de erro genérica
    app.UseExceptionHandler("/Home/Error");
    // Adiciona HSTS (HTTP Strict Transport Security) para segurança em produção
    app.UseHsts();
}

// Redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();
// Habilita o uso de arquivos estáticos (CSS, JS, imagens)
app.UseStaticFiles();

// Configura o roteamento. Deve vir antes de Authentication e Authorization.
app.UseRouting();

// Habilita a autenticação (para login, logout, etc. - necessário para o Identity)
app.UseAuthentication(); // <--- ADICIONADO AQUI E MUITO IMPORTANTE

// Habilita a autorização (para verificar permissões de acesso baseadas em roles/políticas)
app.UseAuthorization();

// Mapeia as rotas para controladores MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapeia as rotas para Razor Pages (onde a UI do Identity está localizada)
app.MapRazorPages(); // <--- Continua aqui e é executado após o MapControllerRoute

// Inicia a aplicação
app.Run();