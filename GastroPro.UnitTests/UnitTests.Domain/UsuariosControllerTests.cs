using GastroPro.Domain.Entities;
using GastroPro.Domain.Interfaces;
using GastroPro.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Domain
{
    public class UsuariosControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UsuariosController _controller;

        public UsuariosControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new UsuariosController(_mockUnitOfWork.Object);
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
            // Inicializamos TempData con un diccionario vacío para evitar NullReferenceException
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        /// <summary>
        /// Obtiene el usuario activo de la sesión
        /// </summary>
        private string? GetUsuarioActivoFromSession()
        {
            return _controller.HttpContext.Session.GetString("UsuarioActivo");
        }

        /// <summary>
        /// Obtiene el rol activo de la sesión
        /// </summary>
        private string? GetRolActivoFromSession()
        {
            return _controller.HttpContext.Session.GetString("RolActivo");
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_DebeRetornarViewResult_ConListaDeUsuarios()
        {
            // --- ARRANGE ---
            var usuariosEsperados = new List<Usuario>
            {
                new Usuario { UsuarioId = 1, Nombre = "Keyla Gutierrez", Rol = "Administrador", Contrasena = "admin123" },
                new Usuario { UsuarioId = 2, Nombre = "Juan (Mozo 1)", Rol = "Mozo" },
                new Usuario { UsuarioId = 3, Nombre = "Roxana Palomino", Rol = "Cajero", Contrasena = "caja123" }
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuariosAsync())
                .ReturnsAsync(usuariosEsperados);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Usuario>>(viewResult.Model);
            Assert.Equal(3, model.Count());
            _mockUnitOfWork.Verify(u => u.GetUsuariosAsync(), Times.Once());
        }

        [Fact]
        public async Task Index_DebeCrearUsuariosDefault_SiLaBaseDatosEstaVacia()
        {
            // --- ARRANGE ---
            var usuariosVacios = new List<Usuario>();

            _mockUnitOfWork
                .Setup(u => u.GetUsuariosAsync())
                .ReturnsAsync(usuariosVacios);

            // Para la segunda llamada después de crear los usuarios
            var usuariosCreados = new List<Usuario>
            {
                new Usuario { UsuarioId = 1, Nombre = "Keyla Gutierrez", Rol = "Administrador", Contrasena = "admin123" },
                new Usuario { UsuarioId = 2, Nombre = "Juan (Mozo 1)", Rol = "Mozo" },
                new Usuario { UsuarioId = 3, Nombre = "Roxana Palomino", Rol = "Cajero", Contrasena = "caja123" }
            };

            var callCount = 0;
            _mockUnitOfWork
                .Setup(u => u.GetUsuariosAsync())
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1 ? Task.FromResult<IEnumerable<Usuario>>(usuariosVacios) : Task.FromResult<IEnumerable<Usuario>>(usuariosCreados);
                });

            _mockUnitOfWork
                .Setup(u => u.AddUsuarioAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Usuario>>(viewResult.Model);
            Assert.Equal(3, model.Count);
            _mockUnitOfWork.Verify(u => u.AddUsuarioAsync(It.IsAny<Usuario>()), Times.Exactly(3));
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        #endregion

        #region Create Tests

        [Fact]
        public void CreateGet_DebeRetornarViewResult()
        {
            // --- ARRANGE ---
            // No se necesita setup

            // --- ACT ---
            var result = _controller.Create();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_DebeGuardarUsuario_SiModelStateEsValido()
        {
            // --- ARRANGE ---
            var nuevoUsuario = new Usuario
            {
                Nombre = "Pedro Martinez",
                Rol = "Mozo",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.AddUsuarioAsync(nuevoUsuario))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Create(nuevoUsuario);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.AddUsuarioAsync(nuevoUsuario), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task CreatePost_DebeValidarContrasena_SiRolEsAdministrador()
        {
            // --- ARRANGE ---
            var nuevoUsuario = new Usuario
            {
                Nombre = "Admin Sin Contraseña",
                Rol = "Administrador",
                Contrasena = null,
                EstaActivo = true
            };

            // Simulamos que ModelState falla
            _controller.ModelState.AddModelError("Contrasena", "La contraseña es obligatoria para el rol Administrador.");

            // --- ACT ---
            var result = await _controller.Create(nuevoUsuario);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _mockUnitOfWork.Verify(u => u.AddUsuarioAsync(It.IsAny<Usuario>()), Times.Never());
        }

        [Fact]
        public async Task CreatePost_DebeValidarContrasena_SiRolEsCajero()
        {
            // --- ARRANGE ---
            var nuevoUsuario = new Usuario
            {
                Nombre = "Cajero Sin Contraseña",
                Rol = "Cajero",
                Contrasena = null,
                EstaActivo = true
            };

            _controller.ModelState.AddModelError("Contrasena", "La contraseña es obligatoria para el rol Cajero.");

            // --- ACT ---
            var result = await _controller.Create(nuevoUsuario);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _mockUnitOfWork.Verify(u => u.AddUsuarioAsync(It.IsAny<Usuario>()), Times.Never());
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task EditGet_DebeRetornarViewConUsuario_SiUsuarioExiste()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioEsperado = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioEsperado);

            // --- ACT ---
            var result = await _controller.Edit(usuarioId);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Usuario>(viewResult.Model);
            Assert.Equal(usuarioId, model.UsuarioId);
            Assert.Equal("Keyla Gutierrez", model.Nombre);
        }

        [Fact]
        public async Task EditGet_DebeRetornarNotFound_SiUsuarioNoExiste()
        {
            // --- ARRANGE ---
            int usuarioId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync((Usuario?)null);

            // --- ACT ---
            var result = await _controller.Edit(usuarioId);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
            _mockUnitOfWork.Verify(u => u.GetUsuarioByIdAsync(usuarioId), Times.Once());
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task EditPost_DebeActualizarUsuario_SiModelStateEsValido()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioModificado = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Keyla Gutierrez Editado",
                Rol = "Administrador",
                Contrasena = "newadmin123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.UpdateUsuario(usuarioModificado))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.Edit(usuarioId, usuarioModificado);

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
            _mockUnitOfWork.Verify(u => u.UpdateUsuario(usuarioModificado), Times.Once());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task EditPost_DebeRetornarNotFound_SiIdNoCoincide()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioModificado = new Usuario
            {
                UsuarioId = 5,
                Nombre = "Usuario Diferente",
                Rol = "Mozo",
                EstaActivo = true
            };

            // --- ACT ---
            var result = await _controller.Edit(usuarioId, usuarioModificado);

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditPost_DebeRetornarView_SiModelStateNoEsValido()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioInvalido = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "",
                Rol = "Administrador",
                Contrasena = null,
                EstaActivo = true
            };

            _controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

            // --- ACT ---
            var result = await _controller.Edit(usuarioId, usuarioInvalido);

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            _mockUnitOfWork.Verify(u => u.UpdateUsuario(It.IsAny<Usuario>()), Times.Never());
        }

        #endregion

        #region Login Tests (CambiarUsuarioActivo)

        [Fact]
        public async Task Login_DebeGuardarUsuarioEnSesion_SiCredencialesValidas_Administrador()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioAdministrador = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioAdministrador);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "admin123");

            // --- ASSERT ---
            Assert.Equal("Keyla Gutierrez", GetUsuarioActivoFromSession());
            Assert.Equal("Administrador", GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_DebeGuardarUsuarioEnSesion_SiCredencialesValidas_Cajero()
        {
            // --- ARRANGE ---
            int usuarioId = 3;
            var usuarioCajero = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Roxana Palomino",
                Rol = "Cajero",
                Contrasena = "caja123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioCajero);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "caja123");

            // --- ASSERT ---
            Assert.Equal("Roxana Palomino", GetUsuarioActivoFromSession());
            Assert.Equal("Cajero", GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Pagos", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_DebeGuardarUsuarioEnSesion_SiCredencialesValidas_Mozo()
        {
            // --- ARRANGE ---
            int usuarioId = 2;
            var usuarioMozo = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Juan (Mozo 1)",
                Rol = "Mozo",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioMozo);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, null);

            // --- ASSERT ---
            Assert.Equal("Juan (Mozo 1)", GetUsuarioActivoFromSession());
            Assert.Equal("Mozo", GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_DebeRetornarErrorPassword_SiCredencialesInvalidas_Administrador()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioAdministrador = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Keyla Gutierrez",
                Rol = "Administrador",
                Contrasena = "admin123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioAdministrador);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "contraseñaIncorrecta");

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["ErrorPassword"]);
            Assert.Contains("Contraseña incorrecta", _controller.TempData["ErrorPassword"].ToString()!);
            // Usuario no debe estar en sesión
            Assert.Null(GetUsuarioActivoFromSession());
        }

        [Fact]
        public async Task Login_DebeRetornarErrorPassword_SiCredencialesInvalidas_Cajero()
        {
            // --- ARRANGE ---
            int usuarioId = 3;
            var usuarioCajero = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Roxana Palomino",
                Rol = "Cajero",
                Contrasena = "caja123",
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioCajero);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "passwordErronea");

            // --- ASSERT ---
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["ErrorPassword"]);
            Assert.Null(GetUsuarioActivoFromSession());
        }

        [Fact]
        public async Task Login_DebeRetornarNotFound_SiUsuarioNoExisteEnLaBaseDatos()
        {
            // --- ARRANGE ---
            int usuarioId = 999;

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync((Usuario?)null);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "cualquierPassword");

            // --- ASSERT ---
            Assert.IsType<NotFoundResult>(result);
            Assert.Null(GetUsuarioActivoFromSession());
            _mockUnitOfWork.Verify(u => u.GetUsuarioByIdAsync(usuarioId), Times.Once());
        }

        [Fact]
        public async Task Login_DebeAplicarClaveDefault_SiAdministradorNoTieneContrasena()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuarioSinContrasena = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Admin Sin Clave",
                Rol = "Administrador",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioSinContrasena);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "admin123");

            // --- ASSERT ---
            Assert.Equal("Admin Sin Clave", GetUsuarioActivoFromSession());
            Assert.Equal("Administrador", GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Login_DebeAplicarClaveDefault_SiCajeroNoTieneContrasena()
        {
            // --- ARRANGE ---
            int usuarioId = 3;
            var usuarioCajeroSinClave = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Cajero Sin Clave",
                Rol = "Cajero",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioCajeroSinClave);

            _mockUnitOfWork
                .Setup(u => u.CompleteAsync())
                .ReturnsAsync(1);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, "caja123");

            // --- ASSERT ---
            Assert.Equal("Cajero Sin Clave", GetUsuarioActivoFromSession());
            Assert.Equal("Cajero", GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Pagos", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_NoDebeValidarPassword_ParaMozo()
        {
            // --- ARRANGE ---
            int usuarioId = 2;
            var usuarioMozo = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Juan (Mozo 1)",
                Rol = "Mozo",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuarioMozo);

            // --- ACT ---
            var result = await _controller.CambiarUsuarioActivo(usuarioId, null);

            // --- ASSERT ---
            Assert.Equal("Juan (Mozo 1)", GetUsuarioActivoFromSession());
            Assert.Equal("Mozo", GetRolActivoFromSession());
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task Login_DebeVerificarQueGetUsuarioByIdSeaLlamado()
        {
            // --- ARRANGE ---
            int usuarioId = 1;
            var usuario = new Usuario
            {
                UsuarioId = usuarioId,
                Nombre = "Test Usuario",
                Rol = "Mozo",
                Contrasena = null,
                EstaActivo = true
            };

            _mockUnitOfWork
                .Setup(u => u.GetUsuarioByIdAsync(usuarioId))
                .ReturnsAsync(usuario);

            // --- ACT ---
            await _controller.CambiarUsuarioActivo(usuarioId, null);

            // --- ASSERT ---
            _mockUnitOfWork.Verify(u => u.GetUsuarioByIdAsync(usuarioId), Times.Once());
        }

        #endregion

        #region CerrarSesion Tests

        [Fact]
        public void CerrarSesion_DebeEliminarVariablesSesion_YRedirigirAlIndex()
        {
            // --- ARRANGE ---
            _controller.HttpContext.Session.SetString("UsuarioActivo", "Keyla Gutierrez");
            _controller.HttpContext.Session.SetString("RolActivo", "Administrador");

            // --- ACT ---
            var result = _controller.CerrarSesion();

            // --- ASSERT ---
            Assert.Null(GetUsuarioActivoFromSession());
            Assert.Null(GetRolActivoFromSession());
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
            Assert.Equal("Usuarios", redirectResult.ControllerName);
        }

        [Fact]
        public void CerrarSesion_DebeEliminarSesion_Completamente()
        {
            // --- ARRANGE ---
            _controller.HttpContext.Session.SetString("UsuarioActivo", "Keyla Gutierrez");
            _controller.HttpContext.Session.SetString("RolActivo", "Administrador");
            _controller.HttpContext.Session.SetString("OtraVariable", "ValorCualquiera");

            // --- ACT ---
            var result = _controller.CerrarSesion();

            // --- ASSERT ---
            var mockSession = (MockSession)_controller.HttpContext.Session;
            Assert.Empty(mockSession.Keys);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UsuariosController.Index), redirectResult.ActionName);
        }

        #endregion
    }

    /// <summary>
    /// Clase auxiliar para simular ISession en pruebas unitarias
    /// </summary>
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public string Id => "test-session-id";

        public bool IsAvailable => true;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Clase auxiliar para simular ITempDataProvider en pruebas unitarias
    /// </summary>
    public class MockTempDataProvider : Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider
    {
        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            // No necesitamos persistir nada para las pruebas
        }

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }
    }
}
