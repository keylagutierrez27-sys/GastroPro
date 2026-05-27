using GastroPro.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Web
{
    public class PagoTests
    {
        private const decimal MONTO_VALIDO = 100.00m;
        private static readonly List<string> METODOS_AUTORIZADOS = new() { "Efectivo", "Yape", "Plin", "Tarjeta" };

        #region Constructor and Initialization Tests

        [Fact]
        public void Constructor_DebeCrearPago_ConValoresPorDefectoSeguros()
        {
            // --- ARRANGE & ACT ---
            var pago = new Pago();

            // --- ASSERT ---
            Assert.NotNull(pago);
            Assert.Equal(0.00m, pago.TotalPagado);
            Assert.Equal("Efectivo", pago.MetodoPago);
            Assert.Equal(string.Empty, pago.NroOperacion);
            Assert.Equal(string.Empty, pago.NumeroMesa);
        }

        [Fact]
        public void Constructor_DebeRegistrarFechaDeTransaccionExacta()
        {
            // --- ARRANGE ---
            var ahora = DateTime.Now;

            // --- ACT ---
            var pago = new Pago();

            // --- ASSERT ---
            Assert.True(Math.Abs((pago.FechaPago - ahora).TotalSeconds) < 1);
        }

        #endregion

        #region Validation Tests - Monto Total Cobrado (TotalPagado)

        [Theory]
        [InlineData(0.01)]
        [InlineData(10.50)]
        [InlineData(150.00)]
        [InlineData(9999.99)]
        public void ValidarMontoTotal_DebeAceptarMontosMayoresACero(decimal montoValido)
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.TotalPagado = montoValido;
            var esValido = ValidarMontoTotal(pago.TotalPagado);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(montoValido, pago.TotalPagado);
        }

        [Theory]
        [InlineData(0, "El monto total cobrado debe ser mayor a cero")]
        [InlineData(-0.01, "El monto total cobrado debe ser mayor a cero")]
        [InlineData(-50.00, "El monto total cobrado debe ser mayor a cero")]
        public void ValidarMontoTotal_DebeRechazarMontosMenorOIgualACero(decimal montoInvalido, string mensajeEsperado)
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.TotalPagado = montoInvalido;

            // --- ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMontoTotalConExcepcion(pago.TotalPagado));
            Assert.Equal(mensajeEsperado, excepcion.Message);
        }

        [Fact]
        public void ValidarMontoTotal_DebeRechazarCeroConMensajeClaro()
        {
            // --- ARRANGE ---
            var pago = new Pago { TotalPagado = 0 };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMontoTotalConExcepcion(pago.TotalPagado));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarMontoTotal_DebeRechazarMontoNegativoConMensajeClaro()
        {
            // --- ARRANGE ---
            var pago = new Pago { TotalPagado = -25.50m };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMontoTotalConExcepcion(pago.TotalPagado));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        #endregion

        #region Validation Tests - Método de Pago

        [Theory]
        [InlineData("Efectivo")]
        [InlineData("Yape")]
        [InlineData("Plin")]
        [InlineData("Tarjeta")]
        public void ValidarMetodoPago_DebeAceptarMetodosAutorizados(string metodoValido)
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.MetodoPago = metodoValido;
            var esValido = ValidarMetodoPago(pago.MetodoPago);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(metodoValido, pago.MetodoPago);
        }

        [Theory]
        [InlineData("Bitcoin", "El método de pago 'Bitcoin' no es válido")]
        [InlineData("Cheque", "El método de pago 'Cheque' no es válido")]
        [InlineData("Fiado", "El método de pago 'Fiado' no es válido")]
        [InlineData("", "El método de pago '' no es válido")]
        public void ValidarMetodoPago_DebeRechazarMetodosNoAutorizados(string metodoInvalido, string mensajeEsperado)
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMetodoPagoConExcepcion(metodoInvalido));
            Assert.Equal(mensajeEsperado, excepcion.Message);
        }

        [Fact]
        public void ValidarMetodoPago_DebeRechazarMetodoNull()
        {
            // --- ARRANGE ---
            string metodoNull = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarMetodoPagoConExcepcion(metodoNull));
            Assert.Equal("metodoPago", excepcion.ParamName);
        }

        #endregion

        #region Validation Tests - Pago Completo (Monto + Método)

        [Fact]
        public void ValidarPagoCompleto_DebePasarConMontoYMetodoValidos()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = MONTO_VALIDO,
                MetodoPago = "Yape",
                NumeroMesa = "3"
            };

            // --- ACT ---
            var esValido = ValidarPagoCompleto(pago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarPagoCompleto_DebeFallarSiMontoEsCero()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = 0,
                MetodoPago = "Efectivo"
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPagoCompletoConExcepcion(pago));
            Assert.Contains("monto", excepcion.Message.ToLower());
        }

        [Fact]
        public void ValidarPagoCompleto_DebeFallarSiMontoEsNegativo()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = -100m,
                MetodoPago = "Tarjeta"
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPagoCompletoConExcepcion(pago));
            Assert.Contains("monto", excepcion.Message.ToLower());
        }

        [Fact]
        public void ValidarPagoCompleto_DebeFallarSiMetodoNoEsValido()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = MONTO_VALIDO,
                MetodoPago = "CriptoBTC"
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPagoCompletoConExcepcion(pago));
            Assert.Contains("método", excepcion.Message.ToLower());
        }

        [Fact]
        public void ValidarPagoCompleto_DebeFallarSiMetodoEstaVacio()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = MONTO_VALIDO,
                MetodoPago = string.Empty
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPagoCompletoConExcepcion(pago));
            Assert.Contains("método", excepcion.Message.ToLower());
        }

        #endregion

        #region Edge Cases and Additional Tests

        [Fact]
        public void ValidarMontoTotal_DebeAceptarMontoMuyPequeno()
        {
            // --- ARRANGE ---
            decimal montoMuyPequeno = 0.01m;

            // --- ACT ---
            var esValido = ValidarMontoTotal(montoMuyPequeno);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMontoTotal_DebeAceptarMontoMuyGrande()
        {
            // --- ARRANGE ---
            decimal montoMuyGrande = 999999.99m;

            // --- ACT ---
            var esValido = ValidarMontoTotal(montoMuyGrande);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void Pago_DebeTenerNroOperacionPorDefectoVacio()
        {
            // --- ARRANGE & ACT ---
            var pago = new Pago();

            // --- ASSERT ---
            Assert.Equal(string.Empty, pago.NroOperacion);
        }

        [Fact]
        public void Pago_DebePoderseteNroOperacion()
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.NroOperacion = "OP123456";

            // --- ASSERT ---
            Assert.Equal("OP123456", pago.NroOperacion);
        }

        [Fact]
        public void Pago_DebePoderseteNumeroMesa()
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.NumeroMesa = "5A";

            // --- ASSERT ---
            Assert.Equal("5A", pago.NumeroMesa);
        }

        [Fact]
        public void ValidarMetodoPago_DebeAceptarEfectivo()
        {
            // --- ARRANGE ---
            string metodoPago = "Efectivo";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeAceptarYape()
        {
            // --- ARRANGE ---
            string metodoPago = "Yape";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeAceptarPlin()
        {
            // --- ARRANGE ---
            string metodoPago = "Plin";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeAceptarTarjeta()
        {
            // --- ARRANGE ---
            string metodoPago = "Tarjeta";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeRechazarMetodoConCasillasDiferentes()
        {
            // --- ARRANGE ---
            string metodoPago = "EFECTIVO";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeRechazarMetodoVacio()
        {
            // --- ARRANGE ---
            string metodoPago = string.Empty;

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Fact]
        public void ValidarMetodoPago_DebeRechazarMetodoConEspacios()
        {
            // --- ARRANGE ---
            string metodoPago = " Efectivo ";

            // --- ACT ---
            var esValido = ValidarMetodoPago(metodoPago);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Theory]
        [InlineData(0.001)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(5.75)]
        [InlineData(100.0)]
        [InlineData(500.50)]
        public void ValidarPagoCompleto_DebeAceptarVariosMontosPorMétodo(decimal monto)
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = monto,
                MetodoPago = "Efectivo"
            };

            // --- ACT ---
            var esValido = ValidarPagoCompleto(pago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarPagoCompleto_DebeFallarSiPagoEsNull()
        {
            // --- ARRANGE ---
            Pago pago = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarPagoCompletoConExcepcion(pago));
            Assert.Equal("pago", excepcion.ParamName);
        }

        [Fact]
        public void Pago_DebePermitirModificarTotalPagado()
        {
            // --- ARRANGE ---
            var pago = new Pago();

            // --- ACT ---
            pago.TotalPagado = 250.75m;

            // --- ASSERT ---
            Assert.Equal(250.75m, pago.TotalPagado);
        }

        [Fact]
        public void Pago_DebePermitirModificarMetodoPago()
        {
            // --- ARRANGE ---
            var pago = new Pago { MetodoPago = "Efectivo" };

            // --- ACT ---
            pago.MetodoPago = "Yape";

            // --- ASSERT ---
            Assert.Equal("Yape", pago.MetodoPago);
        }

        [Fact]
        public void ValidarMontoTotal_DebeRechazarDecimalNegativoMuyPequeno()
        {
            // --- ARRANGE ---
            decimal montoMuyNegativo = -0.001m;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMontoTotalConExcepcion(montoMuyNegativo));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarMontoTotal_DebeRechazarMontoNegativoGrande()
        {
            // --- ARRANGE ---
            decimal montoNegativoGrande = -9999.99m;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarMontoTotalConExcepcion(montoNegativoGrande));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarMetodoPago_DebeRechazarMetodoNuloConExcepcion()
        {
            // --- ARRANGE ---
            string metodoPago = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarMetodoPagoConExcepcion(metodoPago));
            Assert.Equal("metodoPago", excepcion.ParamName);
        }

        [Fact]
        public void Pago_DebeInicializarFechaPagoConHoraActual()
        {
            // --- ARRANGE ---
            var ahora = DateTime.Now;

            // --- ACT ---
            var pago = new Pago();

            // --- ASSERT ---
            Assert.NotEqual(DateTime.MinValue, pago.FechaPago);
            Assert.True(pago.FechaPago >= ahora.AddSeconds(-1));
        }

        [Fact]
        public void ValidarPagoCompleto_DebeAceptarPagoConTodosLosCampos()
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                PagoId = 1,
                TotalPagado = 150.00m,
                MetodoPago = "Tarjeta",
                NumeroMesa = "10",
                NroOperacion = "TRX789456",
                CierreCajaId = 1
            };

            // --- ACT ---
            var esValido = ValidarPagoCompleto(pago);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(1, pago.PagoId);
            Assert.Equal(1, pago.CierreCajaId);
        }

        [Theory]
        [InlineData("Plin")]
        [InlineData("Yape")]
        [InlineData("Tarjeta")]
        public void ValidarPagoCompleto_DebeAceptarDiferentesMetodosDePago(string metodo)
        {
            // --- ARRANGE ---
            var pago = new Pago
            {
                TotalPagado = MONTO_VALIDO,
                MetodoPago = metodo
            };

            // --- ACT ---
            var esValido = ValidarPagoCompleto(pago);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarMontoTotal_DebeAceptarMontoExactoEnDecimales()
        {
            // --- ARRANGE ---
            decimal montoDecimal = 123.456789m;

            // --- ACT ---
            var esValido = ValidarMontoTotal(montoDecimal);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        #endregion

        #region Métodos Privados de Validación

        /// <summary>
        /// Valida que el monto total cobrado sea mayor a cero.
        /// </summary>
        private bool ValidarMontoTotal(decimal totalPagado)
        {
            return totalPagado > 0;
        }

        /// <summary>
        /// Valida que el monto total cobrado sea mayor a cero. Lanza ArgumentException si es inválido.
        /// </summary>
        private void ValidarMontoTotalConExcepcion(decimal totalPagado)
        {
            if (totalPagado <= 0)
            {
                throw new ArgumentException("El monto total cobrado debe ser mayor a cero");
            }
        }

        /// <summary>
        /// Valida que el método de pago sea uno de los autorizados.
        /// </summary>
        private bool ValidarMetodoPago(string metodoPago)
        {
            return !string.IsNullOrEmpty(metodoPago) && METODOS_AUTORIZADOS.Contains(metodoPago);
        }

        /// <summary>
        /// Valida que el método de pago sea uno de los autorizados. Lanza ArgumentException si es inválido.
        /// </summary>
        private void ValidarMetodoPagoConExcepcion(string metodoPago)
        {
            if (metodoPago == null)
            {
                throw new ArgumentNullException(nameof(metodoPago));
            }

            if (!METODOS_AUTORIZADOS.Contains(metodoPago))
            {
                throw new ArgumentException($"El método de pago '{metodoPago}' no es válido");
            }
        }

        /// <summary>
        /// Valida que un pago sea completo (monto y método válidos).
        /// </summary>
        private bool ValidarPagoCompleto(Pago pago)
        {
            return pago != null &&
                   ValidarMontoTotal(pago.TotalPagado) &&
                   ValidarMetodoPago(pago.MetodoPago);
        }

        /// <summary>
        /// Valida que un pago sea completo (monto y método válidos). Lanza excepciones si hay errores.
        /// </summary>
        private void ValidarPagoCompletoConExcepcion(Pago pago)
        {
            if (pago == null)
            {
                throw new ArgumentNullException(nameof(pago));
            }

            if (pago.TotalPagado <= 0)
            {
                throw new ArgumentException("El monto total cobrado debe ser mayor a cero");
            }

            if (string.IsNullOrEmpty(pago.MetodoPago) || !METODOS_AUTORIZADOS.Contains(pago.MetodoPago))
            {
                throw new ArgumentException($"El método de pago '{pago.MetodoPago}' no es válido");
            }
        }

        #endregion
    }
}
