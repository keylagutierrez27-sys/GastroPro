using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using GastroPro.Web.Controllers;
using GastroPro.Domain.Interfaces;
using GastroPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GastroPro.UnitTests.UnitTests.Domain
{
    public class PagosControllerWebTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ISession> _mockSession;
        private readonly PagosController _pagosController;

        public PagosControllerWebTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSession = new Mock<ISession>();

            _pagosController = new PagosController(_mockUnitOfWork.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
            _pagosController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var tempData = new TempDataDictionary(mockHttpContext.Object, Mock.Of<ITempDataProvider>());
            _pagosController.TempData = tempData;
        }

        #region Index Tests

        [Fact]
        public async Task Index_DebeRetornarViewResult()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>();

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_DebeCargarDiaActivo_EnViewBag()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, Estado = "Abierto", TotalVendido = 1500.00m };
            var pagos = new List<Pago>();

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_pagosController.ViewBag.DiaActivo);
            Assert.Equal(diaActivo.CierreCajaId, ((CierreCaja)_pagosController.ViewBag.DiaActivo).CierreCajaId);
        }

        [Fact]
        public async Task Index_DebeCalcularTotalEfectivo()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, MetodoPago = "Efectivo", TotalPagado = 100.00m, CierreCajaId = 1 },
                new Pago { PagoId = 2, MetodoPago = "Efectivo", TotalPagado = 50.00m, CierreCajaId = 1 },
                new Pago { PagoId = 3, MetodoPago = "Yape", TotalPagado = 75.00m, CierreCajaId = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(150.00m, _pagosController.ViewBag.TotalEfectivo);
        }

        [Fact]
        public async Task Index_DebeCalcularTotalYape()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, MetodoPago = "Efectivo", TotalPagado = 100.00m, CierreCajaId = 1 },
                new Pago { PagoId = 2, MetodoPago = "Yape", TotalPagado = 75.50m, CierreCajaId = 1 },
                new Pago { PagoId = 3, MetodoPago = "Yape", TotalPagado = 24.50m, CierreCajaId = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            Assert.Equal(100.00m, _pagosController.ViewBag.TotalYape);
        }

        [Fact]
        public async Task Index_DebeCalcularTotalPlin()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, MetodoPago = "Plin", TotalPagado = 60.00m, CierreCajaId = 1 },
                new Pago { PagoId = 2, MetodoPago = "Plin", TotalPagado = 40.00m, CierreCajaId = 1 },
                new Pago { PagoId = 3, MetodoPago = "Tarjeta", TotalPagado = 100.00m, CierreCajaId = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            Assert.Equal(100.00m, _pagosController.ViewBag.TotalPlin);
        }

        [Fact]
        public async Task Index_DebeCalcularTotalTarjeta()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, MetodoPago = "Efectivo", TotalPagado = 100.00m, CierreCajaId = 1 },
                new Pago { PagoId = 2, MetodoPago = "Tarjeta", TotalPagado = 200.00m, CierreCajaId = 1 },
                new Pago { PagoId = 3, MetodoPago = "Tarjeta", TotalPagado = 300.00m, CierreCajaId = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            Assert.Equal(500.00m, _pagosController.ViewBag.TotalTarjeta);
        }

        [Fact]
        public async Task Index_DebeCargarHistorialCierres()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var historial = new List<CierreCaja>
            {
                new CierreCaja { CierreCajaId = 1, Estado = "Cerrado", TotalVendido = 1000.00m },
                new CierreCaja { CierreCajaId = 2, Estado = "Cerrado", TotalVendido = 1200.00m }
            };
            var pagos = new List<Pago>();

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(historial);

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            Assert.NotNull(_pagosController.ViewBag.HistorialCierres);
            var historialResult = (List<CierreCaja>)_pagosController.ViewBag.HistorialCierres;
            Assert.Equal(2, historialResult.Count);
        }

        #endregion

        #region CobrarMesa Tests

        [Fact]
        public async Task CobrarMesa_DebeRetornarViewResult_ConPedidosValidos()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 1, Plato = new Plato { Precio = 50 } }
            };

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _pagosController.CobrarMesa("1");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CobrarMesa_DebeCalcularTotalCuenta_Correctamente()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 2, Plato = new Plato { Precio = 50 } },
                new Pedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 1, Plato = new Plato { Precio = 100 } }
            };

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _pagosController.CobrarMesa("1");

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pago>(viewResult.Model);
            Assert.Equal(200.00m, model.TotalPagado);
        }

        [Fact]
        public async Task CobrarMesa_DebeExcluirPedidosPagados()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 1, Plato = new Plato { Precio = 100 } },
                new Pedido { NumeroMesa = "1", Estado = "Pagado", Cantidad = 1, Plato = new Plato { Precio = 50 } }
            };

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _pagosController.CobrarMesa("1");

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pago>(viewResult.Model);
            Assert.Equal(100.00m, model.TotalPagado);
        }

        [Fact]
        public async Task CobrarMesa_DebeRedireccionar_AlNohayPedidos()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>();

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _pagosController.CobrarMesa("1");

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Pedidos", redirectResult.ControllerName);
        }

        [Fact]
        public async Task CobrarMesa_DebeCrearPagoConDatosCorrectos()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "5", Estado = "Entregado", Cantidad = 2, Plato = new Plato { Precio = 75 } }
            };

            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _pagosController.CobrarMesa("5");

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pago>(viewResult.Model);
            Assert.Equal("5", model.NumeroMesa);
            Assert.Equal(150.00m, model.TotalPagado);
            Assert.Equal("000000", model.NroOperacion);
        }

        #endregion

        #region ProcesarPago Tests

        [Fact]
        public async Task ProcesarPago_DebeGuardarPago_SiModelStateEsValido()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new Pago 
            { 
                NumeroMesa = "1", 
                TotalPagado = 100.00m, 
                MetodoPago = "Efectivo",
                NroOperacion = "EFECTIVO"
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado", Cantidad = 1, Plato = new Plato { Precio = 100 } }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddPagoAsync(It.IsAny<Pago>()), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task ProcesarPago_DebeGenerarNroOperacion_SiNoEstaProvisto()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new Pago 
            { 
                NumeroMesa = "1", 
                TotalPagado = 100.00m, 
                MetodoPago = "Yape",
                NroOperacion = null
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado" }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.AddPagoAsync(It.Is<Pago>(p => p.NroOperacion.StartsWith("AUTO-"))), Times.Once());
        }

        [Fact]
        public async Task ProcesarPago_DebeActualizarTotalVendido()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 1000.00m };
            var pago = new Pago 
            { 
                NumeroMesa = "1", 
                TotalPagado = 250.00m, 
                MetodoPago = "Tarjeta",
                NroOperacion = "TARJETA-001"
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado" }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Callback<CierreCaja>(c => 
            {
                Assert.Equal(1250.00m, c.TotalVendido);
            }).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>()), Times.Once());
        }

        [Fact]
        public async Task ProcesarPago_DebeMarcarPedidosPagados()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new Pago 
            { 
                NumeroMesa = "2", 
                TotalPagado = 100.00m, 
                MetodoPago = "Efectivo",
                NroOperacion = "EFECTIVO"
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "2", Estado = "Entregado", Cantidad = 1 },
                new Pedido { NumeroMesa = "2", Estado = "Entregado", Cantidad = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Callback<Pedido>(p =>
            {
                Assert.Equal("Pagado", p.Estado);
            }).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.UpdatePedido(It.IsAny<Pedido>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ProcesarPago_DebeRedirigiAVerBoleta_AlCompletarse()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 0.00m };
            var pago = new Pago 
            { 
                PagoId = 5,
                NumeroMesa = "1", 
                TotalPagado = 100.00m, 
                MetodoPago = "Efectivo",
                NroOperacion = "EFECTIVO"
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "1", Estado = "Entregado" }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("VerBoleta", redirectResult.ActionName);
        }

        [Fact]
        public async Task ProcesarPago_DebeRedireccionar_SiModelStateNoEsValido()
        {
            // --- ARRANGE ---
            var pago = new Pago 
            { 
                NumeroMesa = "", 
                TotalPagado = 0m, 
                MetodoPago = null
            };

            _pagosController.ModelState.AddModelError("NumeroMesa", "La mesa es requerida");

            // --- ACT ---
            var result = await _pagosController.ProcesarPago(pago);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region VerBoleta Tests

        [Fact]
        public async Task VerBoleta_DebeRetornarViewResult_ConPagoValido()
        {
            // --- ARRANGE ---
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, NumeroMesa = "1", MetodoPago = "Efectivo", TotalPagado = 100.00m }
            };

            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);

            // --- ACT ---
            var result = await _pagosController.VerBoleta(1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task VerBoleta_DebeRetornarNotFound_SiPagoNoExiste()
        {
            // --- ARRANGE ---
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, NumeroMesa = "1" }
            };

            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);

            // --- ACT ---
            var result = await _pagosController.VerBoleta(999);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task VerBoleta_DebeCargarPagoCorrectoPorId()
        {
            // --- ARRANGE ---
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, NumeroMesa = "1", MetodoPago = "Efectivo", TotalPagado = 100.00m },
                new Pago { PagoId = 2, NumeroMesa = "2", MetodoPago = "Yape", TotalPagado = 150.00m }
            };

            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);

            // --- ACT ---
            var result = await _pagosController.VerBoleta(2);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pago>(viewResult.Model);
            Assert.Equal(2, model.PagoId);
            Assert.Equal("2", model.NumeroMesa);
        }

        #endregion

        #region ImprimirCarta Tests

        [Fact]
        public void ImprimirCarta_DebeRetornarViewResult()
        {
            // --- ACT ---
            var result = _pagosController.ImprimirCarta();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FlujoPagoCompleto_DebeProcessarCorrectamente()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1, TotalVendido = 500.00m };
            var pago = new Pago 
            { 
                PagoId = 1,
                NumeroMesa = "3", 
                TotalPagado = 200.00m, 
                MetodoPago = "Tarjeta",
                NroOperacion = "TARJETA-001"
            };
            var pedidos = new List<Pedido>
            {
                new Pedido { NumeroMesa = "3", Estado = "Entregado", Cantidad = 1, Plato = new Plato { Precio = 200 } }
            };
            var todosLosPagos = new List<Pago>
            {
                pago
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.AddPagoAsync(It.IsAny<Pago>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.UpdateCierreCaja(It.IsAny<CierreCaja>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.GetPedidosAsync()).ReturnsAsync(pedidos);
            _mockUnitOfWork.Setup(u => u.UpdatePedido(It.IsAny<Pedido>())).Verifiable();
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(todosLosPagos);

            // --- ACT ---
            var procesarResult = await _pagosController.ProcesarPago(pago);
            var verBoletaResult = await _pagosController.VerBoleta(pago.PagoId);

            // --- ASSERT ---
            Assert.IsType<RedirectToActionResult>(procesarResult);
            Assert.IsType<ViewResult>(verBoletaResult);
        }

        [Fact]
        public async Task MultiplesPagos_DebeCalcularCorrectamente()
        {
            // --- ARRANGE ---
            var diaActivo = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, MetodoPago = "Efectivo", TotalPagado = 100.00m, CierreCajaId = 1 },
                new Pago { PagoId = 2, MetodoPago = "Yape", TotalPagado = 75.00m, CierreCajaId = 1 },
                new Pago { PagoId = 3, MetodoPago = "Plin", TotalPagado = 50.00m, CierreCajaId = 1 },
                new Pago { PagoId = 4, MetodoPago = "Tarjeta", TotalPagado = 200.00m, CierreCajaId = 1 }
            };

            _mockUnitOfWork.Setup(u => u.GetCierreActivoAsync()).ReturnsAsync(diaActivo);
            _mockUnitOfWork.Setup(u => u.GetPagosAsync()).ReturnsAsync(pagos);
            _mockUnitOfWork.Setup(u => u.GetHistorialCierresAsync()).ReturnsAsync(new List<CierreCaja>());

            // --- ACT ---
            var result = await _pagosController.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(100.00m, _pagosController.ViewBag.TotalEfectivo);
            Assert.Equal(75.00m, _pagosController.ViewBag.TotalYape);
            Assert.Equal(50.00m, _pagosController.ViewBag.TotalPlin);
            Assert.Equal(200.00m, _pagosController.ViewBag.TotalTarjeta);
        }

        #endregion
    }
}
