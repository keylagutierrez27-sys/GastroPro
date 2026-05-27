using Microsoft.EntityFrameworkCore;
using GastroPro.Domain.Entities;
using GastroPro.Domain.Interfaces;
using GastroPro.Infrastructure.Data;

namespace GastroPro.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GastroProDbContext _context;

        public UnitOfWork(GastroProDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Plato>> GetPlatosAsync()
        {
            try
            {
                return await _context.Platos.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en SSMS al obtener los platos.", ex);
            }
        }

        public async Task AddPlatoAsync(Plato plato)
        {
            if (plato == null) throw new ArgumentNullException(nameof(plato));
            await _context.Platos.AddAsync(plato);
        }

        public async Task<IEnumerable<Pedido>> GetPedidosAsync()
        {
            try
            {
                // Incluye los datos del Plato asociado al Pedido de forma automática (INNER JOIN)
                return await _context.Pedidos
                    .Include(p => p.Plato)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error en SSMS al obtener los pedidos.", ex);
            }
        }

        public async Task AddPedidoAsync(Pedido pedido)
        {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));
            await _context.Pedidos.AddAsync(pedido);
        }

        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al confirmar la transacción en SQL Server.", ex);
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<Plato?> GetPlatoByIdAsync(int id)
        {
            return await _context.Platos.FindAsync(id);
        }

        public void UpdatePlato(Plato plato)
        {
            _context.Platos.Update(plato);
        }

        public void RemovePlato(Plato plato)
        {
            _context.Platos.Remove(plato);
        }

        public async Task<Pedido?> GetPedidoByIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Plato) // Traemos también el plato vinculado
                .FirstOrDefaultAsync(p => p.PedidoId == id);
        }

        public void UpdatePedido(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
        }

        public void RemovePedido(Pedido pedido)
        {
            _context.Pedidos.Remove(pedido);
        }

        public async Task<IEnumerable<Pago>> GetPagosAsync()
        {
            return await _context.Pagos.AsNoTracking().OrderByDescending(p => p.FechaPago).ToListAsync();
        }

        public async Task AddPagoAsync(Pago pago)
        {
            await _context.Pagos.AddAsync(pago);
        }

        public async Task<IEnumerable<CierreCaja>> GetHistorialCierresAsync()
        {
            return await _context.CierresCaja.AsNoTracking().OrderByDescending(c => c.FechaApertura).ToListAsync();
        }

        public async Task<CierreCaja> GetCierreActivoAsync()
        {
            // Buscamos el día comercial que sigue "Abierto"
            var activo = await _context.CierresCaja.FirstOrDefaultAsync(c => c.Estado == "Abierto");

            // Si es la primera vez o no hay ninguno abierto, creamos uno automáticamente para que el sistema nunca falle
            if (activo == null)
            {
                activo = new CierreCaja { FechaApertura = DateTime.Now, Estado = "Abierto", TotalVendido = 0.00m };
                await _context.CierresCaja.AddAsync(activo);
                await _context.SaveChangesAsync();
            }
            return activo;
        }

        public async Task AddCierreCajaAsync(CierreCaja cierre)
        {
            await _context.CierresCaja.AddAsync(cierre);
        }

        public void UpdateCierreCaja(CierreCaja cierre)
        {
            _context.CierresCaja.Update(cierre);
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosAsync()
        {
            return await _context.Usuarios.AsNoTracking().ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task AddUsuarioAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        public void UpdateUsuario(Usuario usuario)
        {
            // Esto le dice a Entity Framework que rastree este objeto como "Modificado"
            _context.Usuarios.Update(usuario);
        }
    }
}