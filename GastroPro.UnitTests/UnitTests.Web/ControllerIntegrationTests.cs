using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using GastroPro.Web.Controllers;
using GastroPro.Domain.Interfaces;
using DomainPago = GastroPro.Domain.Entities.Pago;
using DomainPedido = GastroPro.Domain.Entities.Pedido;
using DomainPlato = GastroPro.Domain.Entities.Plato;
using DomainUsuario = GastroPro.Domain.Entities.Usuario;
using DomainCierreCaja = GastroPro.Domain.Entities.CierreCaja;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GastroPro.UnitTests.UnitTests.Web
{
    public class ControllerIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ISession> _mockSession;

        public ControllerIntegrationTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSession = new Mock<ISession>();
        }

        private T CreateControllerWithContext<T>(T controller) where T : Controller
        {
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            var tempData = new TempDataDictionary(mockHttpContext.Object, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            return controller;
        }

        #region PlatosController Flujos Complejos

        [Fact]
        public async Task PlatosController_FlujoCRUD_Completo()
        {
            // --- ARRANGE ---
            var platos = new List<DomainPlato>
            {
                new DomainPlato { PlatoId = 1, Nombre = "Ceviche", Precio = 50 },
                new DomainPlato { PlatoId = 2, Nombre = "Lomo Saltado", Precio = 60 }
            };

            var platosController = CreateControllerWithContext(new PlatosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPlatosAsync()).ReturnsAsync(platos);

            // --- ACT ---
            var result = await platosController.Index();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPlatosAsync(), Times.Once());
        }

        [Fact]
        public async Task PlatosController_Create_ConDatos_Validos()
        {
            // --- ARRANGE ---
            var nuevoPlato = new DomainPlato { Nombre = "Ají de Gallina", Precio = 45 };
            var platosController = CreateControllerWithContext(new PlatosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.AddPlatoAsync(It.IsAny<DomainPlato>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await platosController.Create(nuevoPlato);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddPlatoAsync(It.IsAny<DomainPlato>()), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task PlatosController_Edit_Actualiza_Precio()
        {
            // --- ARRANGE ---
            var platoModificado = new DomainPlato { PlatoId = 1, Nombre = "Ceviche", Precio = 55 };
            var platosController = CreateControllerWithContext(new PlatosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.UpdatePlato(It.IsAny<DomainPlato>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await platosController.Edit(platoModificado.PlatoId, platoModificado);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.UpdatePlato(It.Is<DomainPlato>(p => p.Precio == 55)), Times.Once());
        }

        [Fact]
        public async Task PlatosController_Delete_EliminaPlato()
        {
            // --- ARRANGE ---
            var platosController = CreateControllerWithContext(new PlatosController(_mockUnitOfWork.Object));
            var platoAEliminar = new DomainPlato { PlatoId = 1, Nombre = "Ceviche" };

            _mockUnitOfWork.Setup(u => u.GetPlatoByIdAsync(1)).ReturnsAsync(platoAEliminar);
            _mockUnitOfWork.Setup(u => u.RemovePlato(It.IsAny<DomainPlato>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await platosController.DeleteConfirmed(1);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.RemovePlato(It.IsAny<DomainPlato>()), Times.Once());
        }

        #endregion

        #region PedidosController Flujos Complejos

        [Fact]
        public async Task PedidosController_FlujoCRUD_Completo()
        {
            // --- ARRANGE ---
            var pedidos = new List<DomainPedido>
            {
                new DomainPedido { PedidoId = 1, NumeroMesa = "1", Estado = "Pendiente" },
                new DomainPedido { PedidoId = 2, NumeroMesa = "2", Estado = "En Cocina" }
            };

            var pedidosController = CreateControllerWithContext(new PedidosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await pedidosController.Index();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPedidosAsync(), Times.Once());
        }

        [Fact]
        public async Task PedidosController_Create_NuevosPedidos()
        {
            // --- ARRANGE ---
            var nuevoPedido = new DomainPedido { NumeroMesa = "3", Cantidad = 2, Estado = "Pendiente" };
            var pedidosController = CreateControllerWithContext(new PedidosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.AddPedidoAsync(It.IsAny<DomainPedido>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await pedidosController.Create(nuevoPedido);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddPedidoAsync(It.IsAny<DomainPedido>()), Times.Once());
        }

        [Fact]
        public async Task PedidosController_UpdateEstado_PendienteAEnCocina()
        {
            // --- ARRANGE ---
            var pedido = new DomainPedido { PedidoId = 1, NumeroMesa = "1", Estado = "Pendiente" };
            var pedidosController = CreateControllerWithContext(new PedidosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPedidoByIdAsync(1)).ReturnsAsync(pedido);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<DomainPedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await pedidosController.Edit(1, "En Cocina");

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.UpdatePedido(It.Is<DomainPedido>(p => p.Estado == "En Cocina")), Times.Once());
        }

        [Fact]
        public async Task PedidosController_Delete_CancelaPedido()
        {
            // --- ARRANGE ---
            var pedidosController = CreateControllerWithContext(new PedidosController(_mockUnitOfWork.Object));
            var pedidoAEliminar = new DomainPedido { PedidoId = 1, NumeroMesa = "1" };

            _mockUnitOfWork.Setup(u => u.GetPedidoByIdAsync(1)).ReturnsAsync(pedidoAEliminar);
            _mockUnitOfWork.Setup(u => u.RemovePedido(It.IsAny<DomainPedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await pedidosController.DeleteConfirmed(1);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.RemovePedido(It.IsAny<DomainPedido>()), Times.Once());
        }

        #endregion

        #region UsuariosController Flujos Complejos

        [Fact]
        public async Task UsuariosController_FlujoCRUD_Completo()
        {
            // --- ARRANGE ---
            var usuarios = new List<DomainUsuario>
            {
                new DomainUsuario { UsuarioId = 1, Nombre = "Admin", Rol = "Administrador" },
                new DomainUsuario { UsuarioId = 2, Nombre = "Cajero", Rol = "Cajero" }
            };

            var usuariosController = CreateControllerWithContext(new UsuariosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetUsuariosAsync()).ReturnsAsync(usuarios);

            // --- ACT ---
            var result = await usuariosController.Index();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
            _mockUnitOfWork.Verify(u => u.GetUsuariosAsync(), Times.Once());
        }

        [Fact]
        public async Task UsuariosController_Create_NuevoUsuario()
        {
            // --- ARRANGE ---
            var nuevoUsuario = new DomainUsuario { Nombre = "Mozo1", Rol = "Mozo" };
            var usuariosController = CreateControllerWithContext(new UsuariosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.AddUsuarioAsync(It.IsAny<DomainUsuario>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await usuariosController.Create(nuevoUsuario);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddUsuarioAsync(It.IsAny<DomainUsuario>()), Times.Once());
        }

        [Fact]
        public async Task UsuariosController_Edit_ModificaRol()
        {
            // --- ARRANGE ---
            var usuarioModificado = new DomainUsuario { UsuarioId = 1, Nombre = "Admin", Rol = "Administrador", Contrasena = "admin123" };
            var usuariosController = CreateControllerWithContext(new UsuariosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.UpdateUsuario(It.IsAny<DomainUsuario>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await usuariosController.Edit(1, usuarioModificado);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.UpdateUsuario(It.Is<DomainUsuario>(u => u.Rol == "Administrador")), Times.Once());
        }

        #endregion

        #region CierreCaja Flujos Complejos

        [Fact]
        public async Task Pagos_Index_Carga_DiaActivo_Y_Historial()
        {
            // --- ARRANGE ---
            var diaActivo = new DomainCierreCaja { CierreCajaId = 1, Estado = "Abierto", TotalVendido = 2500.00m };
            var historial = new List<DomainCierreCaja>
            {
                new DomainCierreCaja { CierreCajaId = 1, Estado = "Cerrado", TotalVendido = 3000.00m },
                new DomainCierreCaja { CierreCajaId = 2, Estado = "Cerrado", TotalVendido = 2800.00m }
            };
            var pagos = new List<DomainPago>
            {
                new DomainPago { PagoId = 1, MetodoPago = "Efectivo", TotalPagado = 500.00m, CierreCajaId = 1 },
                new DomainPago { PagoId = 2, MetodoPago = "Yape", TotalPagado = 750.00m, CierreCajaId = 1 }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(historial);

            // --- ACT ---
            var result = await pagosController.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(pagosController.ViewBag.DiaActivo);
            Assert.NotNull(pagosController.ViewBag.HistorialCierres);
            _mockUnitOfWork.Verify(u => u.GetCierreActivoAsync(), Times.Once());
            _mockUnitOfWork.Verify(u => u.GetHistorialCierresAsync(), Times.Once());
        }

        [Fact]
        public async Task CobrarMesa_Con_MultiplasComandas()
        {
            // --- ARRANGE ---
            var pedidos = new List<DomainPedido>
            {
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 2, Plato = new DomainPlato { Precio = 50 } },
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 1, Plato = new DomainPlato { Precio = 75 } },
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 3, Plato = new DomainPlato { Precio = 40 } }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await pagosController.CobrarMesa("5");

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<DomainPago>(viewResult.Model);
            // 2*50 + 1*75 + 3*40 = 100 + 75 + 120 = 295
            Assert.Equal(295.00m, model.TotalPagado);
        }

        [Fact]
        public async Task ProcesarPago_Actualiza_CierreCaja_Y_Pedidos()
        {
            // --- ARRANGE ---
            var diaActivo = new DomainCierreCaja { CierreCajaId = 1, TotalVendido = 1000.00m };
            var pago = new DomainPago 
            { 
                NumeroMesa = "5", 
                TotalPagado = 300.00m, 
                MetodoPago = "Tarjeta",
                NroOperacion = "TARJETA-100"
            };
            var pedidos = new List<DomainPedido>
            {
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 1 },
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 1 }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<DomainPago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<DomainCierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<DomainPedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddPagoAsync(It.IsAny<DomainPago>()), Times.Once());
            _mockUnitOfWork.Verify(u => u.UpdateCierreCaja(It.IsAny<DomainCierreCaja>()), Times.Once());
            _mockUnitOfWork.Verify(u => u.UpdatePedido(It.IsAny<DomainPedido>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce());
        }

        #endregion

        #region Validación de Datos y EdgeCases

        [Fact]
        public async Task PlatosController_No_Permite_Precio_Negativo()
        {
            // --- ARRANGE ---
            var platoInvalido = new DomainPlato { Nombre = "Ceviche", Precio = -50 };
            var platosController = CreateControllerWithContext(new PlatosController(_mockUnitOfWork.Object));
            platosController.ModelState.AddModelError("Precio", "El precio no puede ser negativo");

            // --- ACT ---
            var result = await platosController.Create(platoInvalido);

            // --- ASSERT ---
            Assert.False(platosController.ModelState.IsValid);
        }

        [Fact]
        public async Task PedidosController_No_Permite_Cantidad_Cero()
        {
            // --- ARRANGE ---
            var pedidoInvalido = new DomainPedido { NumeroMesa = "1", Cantidad = 0 };
            var pedidosController = CreateControllerWithContext(new PedidosController(_mockUnitOfWork.Object));
            pedidosController.ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a cero");

            // --- ACT ---
            var result = await pedidosController.Create(pedidoInvalido);

            // --- ASSERT ---
            Assert.False(pedidosController.ModelState.IsValid);
        }

        [Fact]
        public async Task PagosController_Maneja_PagoConTotalCero()
        {
            // --- ARRANGE ---
            var diaActivo = new DomainCierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new DomainPago 
            { 
                NumeroMesa = "1", 
                TotalPagado = 0m, 
                MetodoPago = "Efectivo"
            };
            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));
            pagosController.ModelState.AddModelError("TotalPagado", "El total debe ser mayor a cero");

            // --- ACT ---
            var result = await pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            Assert.False(pagosController.ModelState.IsValid);
        }

        #endregion

        #region Pruebas de Concurrencia y Consistencia

        [Fact]
        public async Task Multiples_Pagos_Actualizan_TotalVendido_Correctamente()
        {
            // --- ARRANGE ---
            var diaActivo = new DomainCierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pagos = new List<DomainPago>
            {
                new DomainPago { NumeroMesa = "1", TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new DomainPago { NumeroMesa = "2", TotalPagado = 150.00m, MetodoPago = "Yape", CierreCajaId = 1 },
                new DomainPago { NumeroMesa = "3", TotalPagado = 200.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<DomainCierreCaja>());

            // --- ACT ---
            var result = await pagosController.Index();

            // --- ASSERT ---
            Assert.Equal(100.00m, pagosController.ViewBag.TotalEfectivo);
            Assert.Equal(150.00m, pagosController.ViewBag.TotalYape);
            Assert.Equal(200.00m, pagosController.ViewBag.TotalTarjeta);
        }

        [Fact]
        public async Task Multiples_Pedidos_Por_Mesa_Se_Agrupan_Correctamente()
        {
            // --- ARRANGE ---
            var pedidos = new List<DomainPedido>
            {
                new DomainPedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 2, Plato = new DomainPlato { Precio = 50 } },
                new DomainPedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 1, Plato = new DomainPlato { Precio = 75 } },
                new DomainPedido { NumeroMesa = "1", Estado = "Pagado", Cantidad = 1, Plato = new DomainPlato { Precio = 100 } }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await pagosController.CobrarMesa("1");

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<DomainPago>(viewResult.Model);
            // Solo cuenta Entregado: 2*50 + 1*75 = 175
            Assert.Equal(175.00m, model.TotalPagado);
        }

        #endregion

        #region Pruebas de Redirecciones y Flujos

        [Fact]
        public async Task Pago_Exitoso_Redirige_A_VerBoleta()
        {
            // --- ARRANGE ---
            var diaActivo = new DomainCierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new DomainPago 
            { 
                PagoId = 10,
                NumeroMesa = "5", 
                TotalPagado = 100.00m, 
                MetodoPago = "Efectivo",
                NroOperacion = "EFECTIVO"
            };
            var pedidos = new List<DomainPedido>
            {
                new DomainPedido { NumeroMesa = "5", Estado = "Entregado" }
            };

            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<DomainPago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<DomainCierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<DomainPedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("VerBoleta", redirectResult.ActionName);
        }

        [Fact]
        public async Task CobrarMesa_SinPedidos_Redirige_A_Pedidos_Index()
        {
            // --- ARRANGE ---
            var pedidos = new List<DomainPedido>();
            var pagosController = CreateControllerWithContext(new PagosController(_mockUnitOfWork.Object));

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await pagosController.CobrarMesa("10");

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Pedidos", redirectResult.ControllerName);
        }

        #endregion
    }
}
