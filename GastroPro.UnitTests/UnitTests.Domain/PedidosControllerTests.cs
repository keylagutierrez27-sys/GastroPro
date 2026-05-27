using GastroPro.Domain.Entities;
using GastroPro.Domain.Interfaces;
using GastroPro.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Domain
{
    public class PedidosControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly PedidosController _controller;

        public PedidosControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new PedidosController(_mockUnitOfWork.Object);
        }

        #region Index Tests

        [Fact]
        public async Task Index_DebeRetornarViewResult_ConListaDePedidos()
        {
            // --- ARRANGE ---
            var pedidosEsperados = new List<Pedido>
            {
                new Pedido { PedidoId = 1, NumeroMesa = "1", Cantidad = 2, Estado = "Pendiente", PlatoId = 1 },
                new Pedido { PedidoId = 2, NumeroMesa = "2", Cantidad = 1, Estado = "En Cocina", PlatoId = 2 }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidosAsync())
                .ReturnsAsync(pedidosEsperados);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Pedido>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal("1", model.First().NumeroMesa);
        }

        [Fact]
        public async Task Index_DebeRetornarViewResult_ConListaVacia()
        {
            // --- ARRANGE ---
            var pedidosVacios = new List<Pedido>();

            _mockUnitOfWork
                .Setup(u => u.GetPedidosAsync())
                .ReturnsAsync(pedidosVacios);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Pedido>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_DebeRetornarViewResultValido()
        {
            // --- ARRANGE ---
            var pedidos = new List<Pedido>
            {
                new Pedido { PedidoId = 1, NumeroMesa = "1", Cantidad = 2, Estado = "Pendiente", PlatoId = 1 }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidosAsync())
                .ReturnsAsync(pedidos);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPedidosAsync(), Times.Once());
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task CreateGet_DebeRetornarViewResult_ConListaDePlatos()
        {
            // --- ARRANGE ---
            var platosEsperados = new List<Plato>
            {
                new Plato { PlatoId = 1, Nombre = "Ceviche", Precio = 25.00m },
                new Plato { PlatoId = 2, Nombre = "Lomo Saltado", Precio = 30.00m }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatosAsync())
                .ReturnsAsync(platosEsperados);

            // --- ACT ---
            var result = await _controller.Create();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.PlatoId);
            _mockUnitOfWork.Verify(u => u.GetPlatosAsync(), Times.Once());
        }

        [Fact]
        public async Task CreateGet_DebeRetornarViewResultValido()
        {
            // --- ARRANGE ---
            var platos = new List<Plato>
            {
                new Plato { PlatoId = 1, Nombre = "Tacos", Precio = 15.00m }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatosAsync())
                .ReturnsAsync(platos);

            // --- ACT ---
            var result = await _controller.Create();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task CreatePost_DebeGuardarPedido_SiMesaEstaDisponible()
        {
            // --- ARRANGE ---
            var nuevosPedido = new Pedido
            {
                PedidoId = 1,
                NumeroMesa = "5",
                Cantidad = 3,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.AddPedidoAsync(nuevosPedido))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevosPedido);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PedidosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.AddPedidoAsync(nuevosPedido), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task CreatePost_DebeRetornarView_SiModelStateNoEsValido()
        {
            // --- ARRANGE ---
            var pedidoInvalido = new Pedido
            {
                PedidoId = 1,
                NumeroMesa = "", // Mesa vacía
                Cantidad = 0, // Cantidad inválida
                Estado = "Pendiente",
                PlatoId = 1
            };

            var platos = new List<Plato>
            {
                new Plato { PlatoId = 1, Nombre = "Pasta", Precio = 20.00m }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatosAsync())
                .ReturnsAsync(platos);

            _controller.ModelState.AddModelError("NumeroMesa", "El número de mesa es obligatorio");

            // --- ACT ---
            var result = await _controller.Create(pedidoInvalido);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.PlatoId);
            _mockUnitOfWork.Verify(u => u.AddPedidoAsync(It.IsAny<Pedido>()), Times.Never());
        }

        [Fact]
        public async Task CreatePost_DebeRetornarRedirectToAction_AlGuardarExitosamente()
        {
            // --- ARRANGE ---
            var nuevoPedido = new Pedido
            {
                NumeroMesa = "10",
                Cantidad = 2,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.AddPedidoAsync(It.IsAny<Pedido>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevoPedido);

            // --- ASSERT ---
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task CreatePost_DebeVerificarQueCompleteSeaLlamado()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = "8",
                Cantidad = 1,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.AddPedidoAsync(It.IsAny<Pedido>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            await _controller.Create(pedido);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task DeleteGet_DebeRetornarViewConPedido_SiPedidoExiste()
        {
            // --- ARRANGE ---
            var pedidoId = 1;
            var pedidoEsperado = new Pedido
            {
                PedidoId = pedidoId,
                NumeroMesa = "3",
                Cantidad = 2,
                Estado = "Entregado",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync(pedidoEsperado);

            // --- ACT ---
            var result = await _controller.Delete(pedidoId);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Pedido>(viewResult.Model);
            Assert.Equal(pedidoId, model.PedidoId);
            Assert.Equal("3", model.NumeroMesa);
        }

        [Fact]
        public async Task DeleteGet_DebeRetornarNotFound_SiPedidoNoExiste()
        {
            // --- ARRANGE ---
            var pedidoId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync((Pedido?)null);

            // --- ACT ---
            var result = await _controller.Delete(pedidoId);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPedidoByIdAsync(pedidoId), Times.Once());
        }

        [Fact]
        public async Task DeleteGet_DebeRetornarViewResultValido()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                PedidoId = 1,
                NumeroMesa = "7",
                Cantidad = 1,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(1))
                .ReturnsAsync(pedido);

            // --- ACT ---
            var result = await _controller.Delete(1);

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_DebeEliminarPedido_YRedireccionar()
        {
            // --- ARRANGE ---
            var pedidoId = 1;
            var pedidoAEliminar = new Pedido
            {
                PedidoId = pedidoId,
                NumeroMesa = "4",
                Cantidad = 2,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync(pedidoAEliminar);

            _mockUnitOfWork
                .Setup(u => u.RemovePedido(pedidoAEliminar))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.DeleteConfirmed(pedidoId);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PedidosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.RemovePedido(pedidoAEliminar), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteConfirmed_DebeRedireccionar_AunSiPedidoNoExiste()
        {
            // --- ARRANGE ---
            var pedidoId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync((Pedido?)null);

            // --- ACT ---
            var result = await _controller.DeleteConfirmed(pedidoId);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PedidosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.RemovePedido(It.IsAny<Pedido>()), Times.Never());
        }

        [Fact]
        public async Task DeleteConfirmed_DebeRequiereConfirmacionAntesDeEliminar()
        {
            // --- ARRANGE ---
            var pedidoId = 1;
            var pedido = new Pedido
            {
                PedidoId = pedidoId,
                NumeroMesa = "2",
                Cantidad = 3,
                Estado = "En Cocina",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync(pedido);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var deleteGetResult = await _controller.Delete(pedidoId);
            var deletePostResult = await _controller.DeleteConfirmed(pedidoId);

            // --- ASSERT ---
            // La confirmación se realiza mediante GET antes de POST
            Assert.IsType<ViewResult>(deleteGetResult);
            Assert.IsType<RedirectToActionResult>(deletePostResult);
        }

        [Fact]
        public async Task DeleteConfirmed_DebeVerificarQueCompleteSeaLlamadoAlEliminar()
        {
            // --- ARRANGE ---
            var pedidoId = 1;
            var pedido = new Pedido
            {
                PedidoId = pedidoId,
                NumeroMesa = "6",
                Cantidad = 2,
                Estado = "Pendiente",
                PlatoId = 1
            };

            _mockUnitOfWork
                .Setup(u => u.GetPedidoByIdAsync(pedidoId))
                .ReturnsAsync(pedido);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            await _controller.DeleteConfirmed(pedidoId);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        #endregion
    }
}
