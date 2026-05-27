using GastroPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GastroPro.Infrastructure.Data
{
    public class GastroProDbContext : DbContext
    {
        public GastroProDbContext(DbContextOptions<GastroProDbContext> options) : base(options)
        {
        }

        // Definición de las Tablas en SQL Server
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Data: Insertar platos iniciales de forma automática para probar tu menú
            modelBuilder.Entity<Plato>().HasData(
                new Plato { PlatoId = 1, Nombre = "Caldo de Gallina", Precio = 15.00m, Categoria = "Caldos" },
                new Plato { PlatoId = 2, Nombre = "Arroz Chaufa", Precio = 12.00m, Categoria = "Menu" },
                new Plato { PlatoId = 3, Nombre = "Lomo Saltado", Precio = 18.00m, Categoria = "Segundos" }
            );
        }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<CierreCaja> CierresCaja { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}