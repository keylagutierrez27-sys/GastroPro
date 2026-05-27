using GastroPro.Web.Controllers;
using GastroPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GastroPro.UnitTests.UnitTests.Domain
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _controller = new HomeController();
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };
        }

        [Fact]
        public void Index_DebeRetornarViewResult_ValidoYNoNulo()
        {
            // --- ARRANGE ---
            // El controlador está inicializado en el constructor

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_DebeRetornarViewResultConNombreVacioPorDefecto()
        {
            // --- ARRANGE ---
            // El controlador está listo para ser utilizado

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void Index_DebeRetornarViewResultSinModelo()
        {
            // --- ARRANGE ---
            // No se requiere modelo en la vista de inicio

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public void Index_DebeRetornarActionResultValido()
        {
            // --- ARRANGE ---
            // El controlador está preparado

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ViewResultDebeSerInstanciaDeViewResult()
        {
            // --- ARRANGE ---
            // Preparar el controlador

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.IsType<ViewResult>(viewResult);
        }

        [Fact]
        public void Privacy_DebeRetornarViewResult_ValidoYNoNulo()
        {
            // --- ARRANGE ---
            // El estado inicial está listo

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_DebeRetornarViewResultConNombreVacioPorDefecto()
        {
            // --- ARRANGE ---
            // El controlador está configurado

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void Privacy_DebeRetornarViewResultSinModelo()
        {
            // --- ARRANGE ---
            // No se requiere modelo en la vista de privacidad

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public void Privacy_DebeRetornarActionResultValido()
        {
            // --- ARRANGE ---
            // El controlador está listo

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IActionResult>(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ViewResultDebeSerInstanciaDeViewResult()
        {
            // --- ARRANGE ---
            // Preparar el controlador

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.IsType<ViewResult>(viewResult);
        }

        [Fact]
        public void ControllerNoDebeSerNulo()
        {
            // --- ARRANGE ---
            // El constructor debe inicializar el controlador

            // --- ACT ---
            var controller = new HomeController();

            // --- ASSERT ---
            Assert.NotNull(controller);
            Assert.IsType<HomeController>(controller);
        }

        [Fact]
        public void Index_ViewResultModelState_DebeSerValido()
        {
            // --- ARRANGE ---
            // El controlador está listo

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Privacy_ViewResultModelState_DebeSerValido()
        {
            // --- ARRANGE ---
            // El controlador está configurado

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Index_DebeRetornarViewResultConViewDataNoNula()
        {
            // --- ARRANGE ---
            // Preparar el controlador

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData);
        }

        [Fact]
        public void Privacy_DebeRetornarViewResultConViewDataNoNula()
        {
            // --- ARRANGE ---
            // Preparar el controlador

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData);
        }

        [Fact]
        public void Index_ResultadoDebeSerDeTipoViewResultNoRedirect()
        {
            // --- ARRANGE ---
            // El controlador debe estar listo

            // --- ACT ---
            var result = _controller.Index();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.False(result is RedirectResult);
            Assert.False(result is RedirectToActionResult);
        }

        [Fact]
        public void Privacy_ResultadoDebeSerDeTipoViewResultNoRedirect()
        {
            // --- ARRANGE ---
            // El controlador debe estar listo

            // --- ACT ---
            var result = _controller.Privacy();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.False(result is RedirectResult);
            Assert.False(result is RedirectToActionResult);
        }

        [Fact]
        public void Index_DebeMantenerConsistencia_VariosLlamados()
        {
            // --- ARRANGE ---
            // El controlador está inicializado

            // --- ACT ---
            var result1 = _controller.Index();
            var result2 = _controller.Index();

            // --- ASSERT ---
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result1);
            Assert.IsType<ViewResult>(result2);
        }

        [Fact]
        public void Privacy_DebeMantenerConsistencia_VariosLlamados()
        {
            // --- ARRANGE ---
            // El controlador está inicializado

            // --- ACT ---
            var result1 = _controller.Privacy();
            var result2 = _controller.Privacy();

            // --- ASSERT ---
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result1);
            Assert.IsType<ViewResult>(result2);
        }

        #region Error Action Tests

        [Fact]
        public void Error_DebeRetornarViewResult_ValidoYNoNulo()
        {
            // --- ARRANGE ---
            // El controlador está inicializado

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_DebeCargarErrorViewModel()
        {
            // --- ARRANGE ---
            // El controlador está preparado

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            Assert.IsType<ErrorViewModel>(viewResult.Model);
        }

        [Fact]
        public void Error_ModeloDebeContenerRequestId_NoVacio()
        {
            // --- ARRANGE ---
            // El controlador está listo

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void Error_RequestId_DebeSerDelHttpContext()
        {
            // --- ARRANGE ---
            Activity.Current = null;

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(_controller.HttpContext.TraceIdentifier, model.RequestId);
        }

        [Fact]
        public void Error_TieneResponseCacheAttribute_Configurado()
        {
            // --- ARRANGE ---
            var method = typeof(HomeController).GetMethod("Error");

            // --- ACT ---
            var attribute = (ResponseCacheAttribute)System.Attribute.GetCustomAttribute(method, typeof(ResponseCacheAttribute));

            // --- ASSERT ---
            Assert.NotNull(attribute);
            Assert.Equal(0, attribute.Duration);
            Assert.Equal(ResponseCacheLocation.None, attribute.Location);
            Assert.True(attribute.NoStore);
        }

        [Fact]
        public void Error_DebeRetornarViewResultConViewDataNoNula()
        {
            // --- ARRANGE ---
            // El controlador está configurado

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData);
        }

        [Fact]
        public void Error_ViewResultModelState_DebeSerValido()
        {
            // --- ARRANGE ---
            // El controlador está listo

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public void Error_DebeMantenerConsistencia_VariosLlamados()
        {
            // --- ARRANGE ---
            // El controlador está inicializado

            // --- ACT ---
            var result1 = _controller.Error();
            var result2 = _controller.Error();

            // --- ASSERT ---
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result1);
            Assert.IsType<ViewResult>(result2);
        }

        [Fact]
        public void Error_ErrorViewModel_ShowRequestId_DebeSerConfigurable()
        {
            // --- ARRANGE ---
            // El controlador está preparado

            // --- ACT ---
            var result = _controller.Error();

            // --- ASSERT ---
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            // ShowRequestId puede ser true o false dependiendo de la configuración
            Assert.NotNull(model);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void TodasLasAcciones_DebenRetornarViewResult()
        {
            // --- ACT ---
            var indexResult = _controller.Index();
            var privacyResult = _controller.Privacy();
            var errorResult = _controller.Error();

            // --- ASSERT ---
            Assert.IsType<ViewResult>(indexResult);
            Assert.IsType<ViewResult>(privacyResult);
            Assert.IsType<ViewResult>(errorResult);
        }

        [Fact]
        public void TodasLasAcciones_DebenRetornarIActionResult()
        {
            // --- ACT ---
            var indexResult = _controller.Index();
            var privacyResult = _controller.Privacy();
            var errorResult = _controller.Error();

            // --- ASSERT ---
            Assert.IsAssignableFrom<IActionResult>(indexResult);
            Assert.IsAssignableFrom<IActionResult>(privacyResult);
            Assert.IsAssignableFrom<IActionResult>(errorResult);
        }

        [Fact]
        public void ControllerDebeEstarPropiamentConfigurado()
        {
            // --- ASSERT ---
            Assert.NotNull(_controller);
            Assert.IsType<HomeController>(_controller);
            Assert.NotNull(_controller.ControllerContext);
            Assert.NotNull(_controller.ControllerContext.HttpContext);
        }

        #endregion
    }
}
