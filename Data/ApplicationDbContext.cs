using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RyujinBites.Models.Identity;
using RyujinBites.Models.Lanchonete;

namespace RyujinBites.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 1. Adicione suas propriedades DbSet para cada uma das suas classes de modelo:
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Cupom> Cupons { get; set; }

        // 2. Configure os relacionamentos no método OnModelCreating:
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // SEMPRE CHAME ESTE MÉTODO PRIMEIRO!

            // Configurações para o relacionamento 1:1 entre ApplicationUser e Cliente
            builder.Entity<Cliente>()
                .HasOne(c => c.ApplicationUser)
                .WithOne(au => au.Cliente)
                .HasForeignKey<Cliente>(c => c.ClienteId);

            // Configurações para o relacionamento 1:1 entre ApplicationUser e Administrador
            builder.Entity<Administrador>()
                .HasOne(a => a.ApplicationUser)
                .WithOne(au => au.Administrador)
                .HasForeignKey<Administrador>(a => a.AdministradorId);

            // Configuração da chave composta para ItemPedido
            builder.Entity<ItemPedido>()
                .HasKey(ip => new { ip.PedidoId, ip.ProdutoId });

            // Relacionamento ItemPedido com Pedido
            builder.Entity<ItemPedido>()
                .HasOne(ip => ip.Pedido)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(ip => ip.PedidoId);

            // Relacionamento ItemPedido com Produto
            builder.Entity<ItemPedido>()
                .HasOne(ip => ip.Produto)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(ip => ip.ProdutoId);

            // Relacionamento Pedido com Pagamento (1:1)
            builder.Entity<Pedido>()
                .HasOne(p => p.Pagamento)
                .WithOne(pag => pag.Pedido)
                .HasForeignKey<Pagamento>(pag => pag.PedidoId);
        }
    }
}