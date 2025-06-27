using Microsoft.AspNetCore.Identity;
// REMOVER: using Microsoft.Extensions.Logging; // Não será mais usado ILogger aqui
using RyujinBites.Models.Identity; // Necessário para ApplicationUser

namespace RyujinBites.Data.Seed
{
    public static class DbInitializer // Classe estática para fácil acesso
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Obter as instâncias do RoleManager, UserManager e DbContext do provedor de serviços
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>(); // Obter a instância do ApplicationDbContext
            // REMOVER: var logger = serviceProvider.GetRequiredService<ILogger<DbInitializer>>(); // Remover o logger aqui

            Console.WriteLine("DbInitializer: Iniciando a população inicial do banco de dados."); // Usando Console.WriteLine

            // 1. Criar Papéis (Roles) se não existirem
            string[] roleNames = { "Administrador", "Cliente" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"DbInitializer: Papel '{roleName}' criado com sucesso."); // Usando Console.WriteLine
                }
            }

            // 2. Criar Usuário Administrador Principal se não existir
            var adminUser = await userManager.FindByEmailAsync("admin@ryujinbites.com");
            if (adminUser == null)
            {
                Console.WriteLine("DbInitializer: Usuário administrador 'admin@ryujinbites.com' não encontrado. Criando..."); // Usando Console.WriteLine
                adminUser = new ApplicationUser
                {
                    UserName = "admin@ryujinbites.com",
                    Email = "admin@ryujinbites.com",
                    Nome = "Administrador Principal",
                    EmailConfirmed = true, // Define como true para que ele possa logar sem confirmar e-mail
                    DataRegistro = DateTime.UtcNow
                };
                // Tenta criar o usuário com a senha
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123"); // <-- ATENÇÃO: Use uma senha forte e mude em produção!
                if (createAdmin.Succeeded)
                {
                    Console.WriteLine("DbInitializer: Usuário administrador criado com sucesso."); // Usando Console.WriteLine
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                    Console.WriteLine("DbInitializer: Papel 'Administrador' atribuído ao usuário admin."); // Usando Console.WriteLine

                    // *** ADIÇÃO: Criar registro Cliente para o Admin (se ele for atuar como cliente) ***
                    var adminCliente = new RyujinBites.Models.Lanchonete.Cliente
                    {
                        ClienteId = adminUser.Id, // O ID do Cliente é o mesmo do ApplicationUser
                        // Preencha com dados iniciais ou deixe nulo se as propriedades são string?
                        Endereco = "Endereço do Administrador",
                        Complemento = "Apto. Admin",
                        Cidade = "Luziânia",
                        Estado = "GO",
                        CEP = "72800-000"
                    };
                    context.Clientes.Add(adminCliente); // Adiciona o novo cliente ao DbSet
                    await context.SaveChangesAsync(); // Salva no banco de dados
                    Console.WriteLine("DbInitializer: Registro Cliente criado para o usuário administrador."); // Usando Console.WriteLine
                    // *************************************************************************
                }
                else
                {
                    Console.WriteLine($"DbInitializer: Falha ao criar usuário admin: {string.Join(", ", createAdmin.Errors.Select(e => e.Description))}"); // Usando Console.WriteLine
                }
            }
            else // Se o usuário administrador já existe
            {
                Console.WriteLine("DbInitializer: Usuário administrador 'admin@ryujinbites.com' já existe."); // Usando Console.WriteLine

                // *** ADIÇÃO: VERIFICAR E CRIAR REGISTRO CLIENTE PARA ADMIN EXISTENTE (SE FALTANDO) ***
                var existingAdminCliente = await context.Clientes.FindAsync(adminUser.Id);
                if (existingAdminCliente == null)
                {
                    Console.WriteLine("DbInitializer: Usuário administrador existe, mas o registro Cliente correspondente está faltando. Criando registro Cliente para admin existente."); // Usando Console.WriteLine
                    var adminCliente = new RyujinBites.Models.Lanchonete.Cliente
                    {
                        ClienteId = adminUser.Id,
                        Endereco = "Endereço do Administrador Existente",
                        Complemento = "Apto. Admin Existente",
                        Cidade = "Luziânia",
                        Estado = "GO",
                        CEP = "72800-000"
                    };
                    context.Clientes.Add(adminCliente);
                    await context.SaveChangesAsync();
                    Console.WriteLine("DbInitializer: Registro Cliente criado para admin existente."); // Usando Console.WriteLine
                }
                // **********************************************************************************
            }

            Console.WriteLine("DbInitializer: População inicial do banco de dados concluída."); // Usando Console.WriteLine
        }
    }
}