using Microsoft.AspNetCore.Mvc;
using GastroPro.Domain.Interfaces;
using GastroPro.Domain.Entities;
using Microsoft.AspNetCore.Http; // Usado para el control de sesiones perimetrales

namespace GastroPro.Web.Controllers
{
    public class PagosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PagosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 📊 DASHBOARD DE CAJA REEMPLAZADO CON DESGLOSE LINQ AVANZADO
        public async Task<IActionResult> Index()
        {
            // 1. Obtenemos el turno o día comercial activo
            var diaActivo = await _unitOfWork.GetCierreActivoAsync();
            ViewBag.DiaActivo = diaActivo;

            // 2. Obtenemos todos los pagos para listarlos en el historial
            var todosLosPagos = await _unitOfWork.GetPagosAsync();

            // Filtramos los pagos que corresponden únicamente al día que está abierto actualmente
            var pagosDelDia = todosLosPagos.Where(p => p.CierreCajaId == diaActivo.CierreCajaId).ToList();

            // 🚀 EXCELENCIA ACADÉMICA - CONSULTAS LINQ AVANZADAS PARA EL ARQUEO DIGITAL
            // Agrupamos y sumamos acumulativamente según el método de pago seleccionado para el balance de caja
            ViewBag.TotalEfectivo = pagosDelDia.Where(p => p.MetodoPago == "Efectivo").Sum(p => p.TotalPagado);
            ViewBag.TotalYape = pagosDelDia.Where(p => p.MetodoPago == "Yape").Sum(p => p.TotalPagado);
            ViewBag.TotalPlin = pagosDelDia.Where(p => p.MetodoPago == "Plin").Sum(p => p.TotalPagado);
            ViewBag.TotalTarjeta = pagosDelDia.Where(p => p.MetodoPago == "Tarjeta").Sum(p => p.TotalPagado);

            // Pasamos el historial de cierres pasados a la vista
            ViewBag.HistorialCierres = await _unitOfWork.GetHistorialCierresAsync();

            return View(pagosDelDia);
        }

        public async Task<IActionResult> CobrarMesa(string mesa)
        {
            var todosLosPedidos = await _unitOfWork.GetPedidosAsync();
            var pedidosMesa = todosLosPedidos.Where(p => p.NumeroMesa == mesa && p.Estado != "Pagado").ToList();

            if (!pedidosMesa.Any()) return RedirectToAction("Index", "Pedidos");

            decimal totalCuenta = pedidosMesa.Sum(p => p.Cantidad * (p.Plato?.Precio ?? 0));

            var nuevoPago = new Pago
            {
                NumeroMesa = mesa,
                TotalPagado = totalCuenta,
                NroOperacion = "000000"
            };

            return View(nuevoPago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarPago(Pago pago)
        {

            System.Diagnostics.Debug.WriteLine("DEBUG: Procesando pago para mesa: " + pago.NumeroMesa);
            // Validar que tenemos datos básicos
            if (pago == null || string.IsNullOrEmpty(pago.NumeroMesa))
            {
                return RedirectToAction("Index", "Pedidos");
            }

            pago.FechaPago = DateTime.Now;

            // 1. Obtenemos el día comercial activo para asociar este dinero
            var diaActivo = await _unitOfWork.GetCierreActivoAsync();
            if (diaActivo == null)
            {
                TempData["ErrorCaja"] = "Error: No hay un turno comercial abierto.";
                return RedirectToAction("Index");
            }

            pago.CierreCajaId = diaActivo.CierreCajaId;

            // 2. Generar número de operación si no existe
            if (string.IsNullOrWhiteSpace(pago.NroOperacion))
            {
                pago.NroOperacion = pago.MetodoPago == "Efectivo" ? "EFECTIVO" : "AUTO-" + DateTime.Now.ToString("mmss");
            }

            // 3. Remover validaciones de propiedades no requeridas
            ModelState.Remove("NroOperacion");
            ModelState.Remove("Plato");
            ModelState.Remove("FechaPago");
            ModelState.Remove("CierreCajaId");
            ModelState.Remove("CierreCaja");

            // 4. Validar solo los campos críticos
            if (string.IsNullOrWhiteSpace(pago.MetodoPago))
            {
                ModelState.AddModelError("MetodoPago", "Debe seleccionar un método de pago.");
            }

            if (pago.TotalPagado <= 0)
            {
                ModelState.AddModelError("TotalPagado", "El monto a pagar debe ser mayor a cero.");
            }

            if (!ModelState.IsValid)
            {
                return View("CobrarMesa", pago);
            }

            try
            {
                // --- PASO CRÍTICO 1: AGREGAR EL PAGO ---
                await _unitOfWork.AddPagoAsync(pago);

                // --- PASO CRÍTICO 2: GUARDAR EN BD PARA GENERAR PagoId ---
                await _unitOfWork.CompleteAsync();

                // ✅ AHORA pago.PagoId TIENE EL VALOR REAL DE LA BASE DE DATOS

                // --- PASO 3: ACTUALIZAR DATOS RELACIONADOS ---
                diaActivo.TotalVendido += pago.TotalPagado;
                _unitOfWork.UpdateCierreCaja(diaActivo);

                // --- PASO 4: MARCAR PEDIDOS COMO PAGADOS ---
                var todosLosPedidos = await _unitOfWork.GetPedidosAsync();
                var pedidosMesa = todosLosPedidos.Where(p => p.NumeroMesa == pago.NumeroMesa && p.Estado != "Pagado").ToList();

                foreach (var pedido in pedidosMesa)
                {
                    pedido.Estado = "Pagado";
                    _unitOfWork.UpdatePedido(pedido);
                }

                // --- PASO 5: GUARDAR LOS CAMBIOS FINALES ---
                await _unitOfWork.CompleteAsync();

                // --- PASO 6: REDIRIGIR CON PagoId VÁLIDO ---
                return RedirectToAction(nameof(VerBoleta), new { id = pago.PagoId });
            }
            catch (Exception ex)
            {
                // Logging del error
                Console.WriteLine($"Error en ProcesarPago: {ex.Message}");
                TempData["ErrorCaja"] = $"Error al procesar el pago: {ex.Message}";
                return View("CobrarMesa", pago);
            }
        }

        // ACCIÓN CRÍTICA MEJORADA: Protege la caja y audita mesas antes del corte
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarDiaComercial()
        {
            // 🛡️ 1. Seguridad perimetral: Validar que solo el Cajero o Administrador ejecuten esto
            var rolActivo = HttpContext.Session.GetString("RolActivo");
            if (rolActivo != "Cajero" && rolActivo != "Administrador")
            {
                return Forbid();
            }

            // 🕵️ 2. Regla de negocio: Validar que no queden mesas consumiendo en el salón
            var todosLosPedidos = await _unitOfWork.GetPedidosAsync();
            var pedidosPendientes = todosLosPedidos.Where(p => p.Estado != "Pagado").ToList();

            if (pedidosPendientes.Any())
            {
                // Enviamos una alerta a la interfaz del Cajero si hay cuentas colgadas
                TempData["ErrorCaja"] = "⚠️ No se puede cerrar la caja. Aún existen mesas en el salón con pedidos pendientes de pago.";
                return RedirectToAction(nameof(Index));
            }

            // 🏦 3. Si todo está en orden, procedemos con el cierre del día actual
            var diaActivo = await _unitOfWork.GetCierreActivoAsync();
            diaActivo.Estado = "Cerrado";
            diaActivo.FechaCierre = DateTime.Now;
            _unitOfWork.UpdateCierreCaja(diaActivo);

            // 🚀 4. Creamos AUTOMÁTICAMENTE el nuevo día comercial en S/ 0.00
            var nuevoDia = new CierreCaja
            {
                FechaApertura = DateTime.Now,
                Estado = "Abierto",
                TotalVendido = 0.00m
            };
            await _unitOfWork.AddCierreCajaAsync(nuevoDia);

            // 💾 5. Confirmamos la transacción en la base de datos de GastroPro
            await _unitOfWork.CompleteAsync();

            var cajeroResponsable = HttpContext.Session.GetString("UsuarioActivo") ?? "Desconocido";
            TempData["ExitoCaja"] = $"✅ Cierre de caja ejecutado con éxito por {cajeroResponsable}. Turno archivado.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> VerBoleta(int? id) // Cambia a int? para evitar errores
        {
            if (id == null) return NotFound();

            var pagos = await _unitOfWork.GetPagosAsync();
            var pagoRealizado = pagos.FirstOrDefault(p => p.PagoId == id);

            if (pagoRealizado == null) return NotFound();

            return View(pagoRealizado);
        }

        // GET: Pagos/ImprimirCarta
        [HttpGet]
        public IActionResult ImprimirCarta()
        {
            // Retorna la vista Views/Pagos/ImprimirCarta.cshtml directamente
            return View();
        }
    }
}