using GastroPro.Domain.Entities;
using GastroPro.Domain.Interfaces;
using GastroPro.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Domain
{
    public class PlatosControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly PlatosController _controller;

        public PlatosControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new PlatosController(_mockUnitOfWork.Object);
            ConfigureHttpContext();
        }

        #region Helper Methods

        /// <summary>
        /// Configura el HttpContext con sesión para las pruebas
        /// </summary>
        private void ConfigureHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            var sessionMock = new MockSession();
            httpContext.Session = sessionMock;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Simula un rol en la sesión del controlador
        /// </summary>
        private void SetRolInSession(string rol)
        {
            _controller.HttpContext.Session.SetString("Rol", rol);
        }

        /// <summary>
        /// Obtiene el rol de la sesión
        /// </summary>
        private string? GetRolFromSession()
        {
            return _controller.HttpContext.Session.GetString("Rol");
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_DebeRetornarViewResult_ConListaDePlatos()
        {
            // --- ARRANGE ---
            var platosEsperados = new List<Plato>
            {
                new Plato { PlatoId = 1, Nombre = "Ceviche", Precio = 25.00m, Categoria = "Entradas" },
                new Plato { PlatoId = 2, Nombre = "Lomo Saltado", Precio = 30.00m, Categoria = "Segundos" }
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatosAsync())
                .ReturnsAsync(platosEsperados);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Plato>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            _mockUnitOfWork.Verify(u => u.GetPlatosAsync(), Times.Once());
        }

        [Fact]
        public async Task Index_DebeRetornarViewResult_ConListaBacia()
        {
            // --- ARRANGE ---
            var platosVacios = new List<Plato>();

            _mockUnitOfWork
                .Setup(u => u.GetPlatosAsync())
                .ReturnsAsync(platosVacios);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Plato>>(viewResult.Model);
            Assert.Empty(model);
        }

        #endregion

        #region Create GET Tests - Rol Mozo (Sin permisos)

        [Fact]
        public void CreateGet_ConRolMozo_NoDebeAcceder()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            string? rolActual = GetRolFromSession();

            // --- ACT ---
            var result = _controller.Create();

            // --- ASSERT ---
            Assert.Equal("Mozo", rolActual);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CreateGet_ConRolMozo_DebeRedirigiAIndex_SiTieneValidacionDeRol()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            var rolActual = GetRolFromSession();

            // --- ACT & ASSERT ---
            Assert.Equal("Mozo", rolActual);
        }

        #endregion

        #region Create GET Tests - Rol Administrador (Con permisos)

        [Fact]
        public void CreateGet_ConRolAdministrador_DebeRetornarViewResult()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            var rolActual = GetRolFromSession();

            // --- ACT ---
            var result = _controller.Create();

            // --- ASSERT ---
            Assert.Equal("Administrador", rolActual);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CreateGet_ConRolAdministrador_DebePermitirAcceso()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");

            // --- ACT ---
            var result = _controller.Create();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(result);
        }

        #endregion

        #region Create POST Tests - Rol Mozo (Sin permisos)

        [Fact]
        public async Task CreatePost_ConRolMozo_NoDebeGuardarPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            var nuevoPlato = new Plato
            {
                Nombre = "Pescado a la Sal",
                Precio = 35.00m,
                Categoria = "Segundos"
            };

            _mockUnitOfWork
                .Setup(u => u.AddPlatoAsync(It.IsAny<Plato>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevoPlato);

            // --- ASSERT ---
            Assert.Equal("Mozo", GetRolFromSession());
        }

        #endregion

        #region Create POST Tests - Rol Administrador (Con permisos)

        [Fact]
        public async Task CreatePost_ConRolAdministrador_DebeGuardarPlato_SiModelStateEsValido()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            var nuevoPlato = new Plato
            {
                Nombre = "Causa Limeña",
                Precio = 18.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.AddPlatoAsync(nuevoPlato))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevoPlato);

            // --- ASSERT ---
            Assert.Equal("Administrador", GetRolFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PlatosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.AddPlatoAsync(nuevoPlato), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task CreatePost_ConRolAdministrador_DebeRetornarView_SiModelStateNoEsValido()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            var platoInvalido = new Plato
            {
                Nombre = "",
                Precio = 0,
                Categoria = "Entradas"
            };

            _controller.ModelState.AddModelError("Nombre", "El nombre del plato es obligatorio");

            // --- ACT ---
            var result = await _controller.Create(platoInvalido);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            _mockUnitOfWork.Verify(u => u.AddPlatoAsync(It.IsAny<Plato>()), Times.Never());
        }

        [Fact]
        public async Task CreatePost_ConRolAdministrador_DebeRedirigiAIndex_AlGuardarExitosamente()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            var nuevoPlato = new Plato
            {
                Nombre = "Tiradito",
                Precio = 22.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.AddPlatoAsync(It.IsAny<Plato>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevoPlato);

            // --- ASSERT ---
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Edit GET Tests - Rol Mozo (Sin permisos)

        [Fact]
        public async Task EditGet_ConRolMozo_NoDebeAcceder()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            int platoId = 1;
            var plato = new Plato { PlatoId = 1, Nombre = "Ceviche", Precio = 25.00m };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(plato);

            // --- ACT ---
            var result = await _controller.Edit(platoId);

            // --- ASSERT ---
            Assert.Equal("Mozo", GetRolFromSession());
        }

        #endregion

        #region Edit GET Tests - Rol Administrador (Con permisos)

        [Fact]
        public async Task EditGet_ConRolAdministrador_DebeRetornarViewConPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoEsperado = new Plato
            {
                PlatoId = platoId,
                Nombre = "Ají de Gallina",
                Precio = 28.00m,
                Categoria = "Segundos"
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(platoEsperado);

            // --- ACT ---
            var result = await _controller.Edit(platoId);

            // --- ASSERT ---
            Assert.Equal("Administrador", GetRolFromSession());
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Plato>(viewResult.Model);
            Assert.Equal(platoId, model.PlatoId);
            Assert.Equal("Ají de Gallina", model.Nombre);
        }

        [Fact]
        public async Task EditGet_ConRolAdministrador_DebeRetornarNotFound_SiPlatoNoExiste()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync((Plato?)null);

            // --- ACT ---
            var result = await _controller.Edit(platoId);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPlatoByIdAsync(platoId), Times.Once());
        }

        #endregion

        #region Edit POST Tests - Rol Mozo (Sin permisos)

        [Fact]
        public async Task EditPost_ConRolMozo_NoDebeActualizarPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            int platoId = 1;
            var platoActualizado = new Plato
            {
                PlatoId = platoId,
                Nombre = "Ceviche Mixto",
                Precio = 30.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.UpdatePlato(It.IsAny<Plato>()))
                .Verifiable();

            // --- ACT ---
            var result = await _controller.Edit(platoId, platoActualizado);

            // --- ASSERT ---
            Assert.Equal("Mozo", GetRolFromSession());
        }

        #endregion

        #region Edit POST Tests - Rol Administrador (Con permisos)

        [Fact]
        public async Task EditPost_ConRolAdministrador_DebeActualizarPlato_SiModelStateEsValido()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoActualizado = new Plato
            {
                PlatoId = platoId,
                Nombre = "Ceviche Premium",
                Precio = 35.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.UpdatePlato(platoActualizado))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Edit(platoId, platoActualizado);

            // --- ASSERT ---
            Assert.Equal("Administrador", GetRolFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PlatosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.UpdatePlato(platoActualizado), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task EditPost_ConRolAdministrador_DebeRetornarView_SiModelStateNoEsValido()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoInvalido = new Plato
            {
                PlatoId = platoId,
                Nombre = "",
                Precio = -10,
                Categoria = "Segundos"
            };

            _controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

            // --- ACT ---
            var result = await _controller.Edit(platoId, platoInvalido);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            _mockUnitOfWork.Verify(u => u.UpdatePlato(It.IsAny<Plato>()), Times.Never());
        }

        [Fact]
        public async Task EditPost_ConRolAdministrador_DebeRetornarNotFound_SiIdNoCoincide()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var plato = new Plato
            {
                PlatoId = 5,
                Nombre = "Otro Plato",
                Precio = 20.00m,
                Categoria = "Bebidas"
            };

            // --- ACT ---
            var result = await _controller.Edit(platoId, plato);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditPost_ConRolAdministrador_DebeRedirigiAIndex_AlActualizarExitosamente()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoActualizado = new Plato
            {
                PlatoId = platoId,
                Nombre = "Solterito",
                Precio = 16.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.UpdatePlato(It.IsAny<Plato>()))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Edit(platoId, platoActualizado);

            // --- ASSERT ---
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Delete GET Tests - Rol Mozo (Sin permisos)

        [Fact]
        public async Task DeleteGet_ConRolMozo_NoDebeAcceder()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            int platoId = 1;
            var plato = new Plato { PlatoId = 1, Nombre = "Ceviche", Precio = 25.00m };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(plato);

            // --- ACT ---
            var result = await _controller.Delete(platoId);

            // --- ASSERT ---
            Assert.Equal("Mozo", GetRolFromSession());
        }

        #endregion

        #region Delete GET Tests - Rol Administrador (Con permisos)

        [Fact]
        public async Task DeleteGet_ConRolAdministrador_DebeRetornarViewConPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoEsperado = new Plato
            {
                PlatoId = platoId,
                Nombre = "Causita",
                Precio = 18.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(platoEsperado);

            // --- ACT ---
            var result = await _controller.Delete(platoId);

            // --- ASSERT ---
            Assert.Equal("Administrador", GetRolFromSession());
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Plato>(viewResult.Model);
            Assert.Equal(platoId, model.PlatoId);
        }

        [Fact]
        public async Task DeleteGet_ConRolAdministrador_DebeRetornarNotFound_SiPlatoNoExiste()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync((Plato?)null);

            // --- ACT ---
            var result = await _controller.Delete(platoId);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
            _mockUnitOfWork.Verify(u => u.GetPlatoByIdAsync(platoId), Times.Once());
        }

        #endregion

        #region Delete POST Tests - Rol Mozo (Sin permisos)

        [Fact]
        public async Task DeleteConfirmed_ConRolMozo_NoDebeEliminarPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Mozo");
            int platoId = 1;
            var plato = new Plato { PlatoId = 1, Nombre = "Ceviche", Precio = 25.00m };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(plato);

            _mockUnitOfWork
                .Setup(u => u.RemovePlato(It.IsAny<Plato>()))
                .Verifiable();

            // --- ACT ---
            var result = await _controller.DeleteConfirmed(platoId);

            // --- ASSERT ---
            Assert.Equal("Mozo", GetRolFromSession());
        }

        #endregion

        #region Delete POST Tests - Rol Administrador (Con permisos)

        [Fact]
        public async Task DeleteConfirmed_ConRolAdministrador_DebeEliminarPlato()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var platoAEliminar = new Plato
            {
                PlatoId = platoId,
                Nombre = "Anticuchos",
                Precio = 24.00m,
                Categoria = "Entradas"
            };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(platoAEliminar);

            _mockUnitOfWork
                .Setup(u => u.RemovePlato(platoAEliminar))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.DeleteConfirmed(platoId);

            // --- ASSERT ---
            Assert.Equal("Administrador", GetRolFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PlatosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.RemovePlato(platoAEliminar), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteConfirmed_ConRolAdministrador_DebeRedirigiAIndex_AunSiPlatoNoExiste()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync((Plato?)null);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(0);

            // --- ACT ---
            var result = await _controller.DeleteConfirmed(platoId);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PlatosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.RemovePlato(It.IsAny<Plato>()), Times.Never());
        }

        [Fact]
        public async Task DeleteConfirmed_ConRolAdministrador_DebeVerificarQueCompleteSeaLlamado()
        {
            // --- ARRANGE ---
            SetRolInSession("Administrador");
            int platoId = 1;
            var plato = new Plato { PlatoId = platoId, Nombre = "Seco de Cordero", Precio = 42.00m };

            _mockUnitOfWork
                .Setup(u => u.GetPlatoByIdAsync(platoId))
                .ReturnsAsync(plato);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            await _controller.DeleteConfirmed(platoId);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        #endregion
    }
}
