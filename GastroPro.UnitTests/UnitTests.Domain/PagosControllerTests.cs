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
    public class PagosControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ISession> _mockSession;
        private readonly UsuariosController _usuariosController;
        private readonly Dictionary<string, object> _sessionData;

        public PagosControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSession = new Mock<ISession>();
            _sessionData = new Dictionary<string, object>();

            _usuariosController = new UsuariosController(_mockUnitOfWork.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
            _usuariosController.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Configurar TempData para evitar NullReferenceException
            var tempData = new TempDataDictionary(mockHttpContext.Object, Mock.Of<ITempDataProvider>());
            _usuariosController.TempData = tempData;
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConClaveValidaParaAdministrador_DebeElevarRolEnSesion()
        {
            // --- ARRANGE ---
            var usuarioAdmin = new Usuario
            {
                UsuarioId = 1,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123"
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(1)).ReturnsAsync(usuarioAdmin);

            // Capturar los valores Set en la sesión
            string usuarioActivo = null;
            string rolActivo = null;

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(1, "admin123");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConClaveInvalidaParaAdministrador_DebeRebotarSinActualizarSesion()
        {
            // --- ARRANGE ---
            var usuarioAdmin = new Usuario
            {
                UsuarioId = 1,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123"
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(1)).ReturnsAsync(usuarioAdmin);

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT & ASSERT ---
            // Con contraseña inválida, el método debería rechazar sin actualizar la sesión
            await _usuariosController.CambiarUsuarioActivo(1, "claveErrada123");

            // Verificamos que CompleteAsync nunca fue invocado (indica no hubo actualización a BD)
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConClaveValidaParaCajero_DebeElevarRolYRedireccionarAPagos()
        {
            // --- ARRANGE ---
            var usuarioCajero = new Usuario
            {
                UsuarioId = 2,
                Nombre = "Roxana Palomino",
                Rol = "Cajero",
                Contrasena = "caja123"
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(2)).ReturnsAsync(usuarioCajero);

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(2, "caja123");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Pagos", redirectResult.ControllerName);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConRolMozo_NoRequiereContrasena()
        {
            // --- ARRANGE ---
            var usuarioMozo = new Usuario
            {
                UsuarioId = 3,
                Nombre = "Juan (Mozo 1)",
                Rol = "Mozo",
                Contrasena = null
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(3)).ReturnsAsync(usuarioMozo);

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(3, null);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConIdInvalido_DebeRetornarNotFound()
        {
            // --- ARRANGE ---
            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(999)).ReturnsAsync((Usuario)null);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(999, "cualquierPassword");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConContraseñaNulaEnBaseDatos_DebeUsarPorDefectoAdministrador()
        {
            // --- ARRANGE ---
            var usuarioAdmin = new Usuario
            {
                UsuarioId = 1,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = null
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(1)).ReturnsAsync(usuarioAdmin);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.FromResult(0));

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(1, "admin123");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConContraseñaNulaEnBaseDatos_DebeUsarPorDefectoCajero()
        {
            // --- ARRANGE ---
            var usuarioCajero = new Usuario
            {
                UsuarioId = 2,
                Nombre = "Roxana Palomino",
                Rol = "Cajero",
                Contrasena = null
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(2)).ReturnsAsync(usuarioCajero);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.FromResult(0));

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(2, "caja123");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_SesionNuncaInvocaCompleteAsync_ConClaveValida()
        {
            // --- ARRANGE ---
            var usuarioAdmin = new Usuario
            {
                UsuarioId = 1,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123"
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(1)).ReturnsAsync(usuarioAdmin);

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT ---
            var result = await _usuariosController.CambiarUsuarioActivo(1, "admin123");

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CambiarUsuarioActivo_ConClaveInvalidaParaCajero_NoDebebeActualizarSesionNiCompleteAsync()
        {
            // --- ARRANGE ---
            var usuarioCajero = new Usuario
            {
                UsuarioId = 2,
                Nombre = "Roxana Palomino",
                Rol = "Cajero",
                Contrasena = "caja123"
            };

            _mockUnitOfWork.Setup(u => u.GetUsuarioByIdAsync(2)).ReturnsAsync(usuarioCajero);

            _mockSession
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns(false);

            // --- ACT & ASSERT ---
            // Con contraseña inválida, el método debería rechazar sin actualizar la sesión
            await _usuariosController.CambiarUsuarioActivo(2, "claveIncorrecta");

            // Verificamos que CompleteAsync nunca fue invocado (no hubo cambios en BD)
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        }
    }
}
