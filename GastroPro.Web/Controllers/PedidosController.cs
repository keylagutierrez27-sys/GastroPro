using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GastroPro.Domain.Interfaces;
using GastroPro.Domain.Entities;

namespace GastroPro.Web.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PedidosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _unitOfWork.GetPedidosAsync();
            return View(pedidos);
        }

        // GET: /Pedidos/Create
        public async Task<IActionResult> Create()
        {
            var platos = await _unitOfWork.GetPlatosAsync();
            // Llenamos un DropDownList para que el usuario seleccione el plato de una lista desplegable
            ViewBag.PlatoId = new SelectList(platos, "PlatoId", "Nombre");
            return View();
        }

        // POST: /Pedidos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.AddPedidoAsync(pedido);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }

            var platos = await _unitOfWork.GetPlatosAsync();
            ViewBag.PlatoId = new SelectList(platos, "PlatoId", "Nombre", pedido.PlatoId);
            return View(pedido);
        }

        // GET: /Pedidos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var pedido = await _unitOfWork.GetPedidoByIdAsync(id);
            if (pedido == null) return NotFound();

            // Lista de estados para el combobox
            ViewBag.Estados = new List<string> { "Pendiente", "En Cocina", "Entregado" };
            return View(pedido);
        }

        // POST: /Pedidos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nuevoEstado)
        {
            var pedido = await _unitOfWork.GetPedidoByIdAsync(id);
            if (pedido == null) return NotFound();

            // Solo actualizamos el estado para no corromper la comanda original
            pedido.Estado = nuevoEstado;

            try
            {
                _unitOfWork.UpdatePedido(pedido);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "No se pudo actualizar el estado en la base de datos.");
                ViewBag.Estados = new List<string> { "Pendiente", "En Cocina", "Entregado" };
                return View(pedido);
            }
        }

        // GET: /Pedidos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _unitOfWork.GetPedidoByIdAsync(id);
            if (pedido == null) return NotFound();
            return View(pedido);
        }

        // POST: /Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedido = await _unitOfWork.GetPedidoByIdAsync(id);
            if (pedido != null)
            {
                _unitOfWork.RemovePedido(pedido);
                await _unitOfWork.CompleteAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}