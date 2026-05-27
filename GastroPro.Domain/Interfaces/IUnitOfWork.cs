using GastroPro.Domain.Entities;

namespace GastroPro.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IEnumerable<Plato>> GetPlatosAsync();
        Task AddPlatoAsync(Plato plato);

        // --- NUEVOS CONTRATOS PARA PLATOS ---
        Task<Plato?> GetPlatoByIdAsync(int id); // Para buscar el plato antes de editarlo o borrarlo
        void UpdatePlato(Plato plato);          // Para guardar el nuevo precio
        void RemovePlato(Plato plato);          // Para eliminarlo de la carta

        Task<IEnumerable<Pedido>> GetPedidosAsync();
        Task AddPedidoAsync(Pedido pedido);
        Task<int> CompleteAsync();

        // --- NUEVOS CONTRATOS PARA PEDIDOS ---
        Task<Pedido?> GetPedidoByIdAsync(int id); // Buscar una comanda específica
        void UpdatePedido(Pedido pedido);          // Cambiar estado (Pendiente -> En Cocina -> Entregado)
        void RemovePedido(Pedido pedido);          // Eliminar o cancelar el pedido

        // --- CONTRATOS PARA PAGOS ---
        Task<IEnumerable<Pago>> GetPagosAsync();
        Task AddPagoAsync(Pago pago);

        // --- CONTRATOS PARA CIERRE DE CAJA ---
        Task<IEnumerable<CierreCaja>> GetHistorialCierresAsync();
        Task<CierreCaja> GetCierreActivoAsync();
        Task AddCierreCajaAsync(CierreCaja cierre);
        void UpdateCierreCaja(CierreCaja cierre);

        // --- CONTRATOS PARA USUARIOS ---
        Task<IEnumerable<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task AddUsuarioAsync(Usuario usuario);

        void UpdateUsuario(Usuario usuario);
    }
}