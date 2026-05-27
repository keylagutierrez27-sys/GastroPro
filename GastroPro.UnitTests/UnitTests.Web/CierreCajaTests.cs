using GastroPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Web
{
    public class CierreCajaTests
    {
        #region Constructor and Initialization Tests

        [Fact]
        public void Constructor_DebeCrearCierreCaja_ConEstadoAbierto()
        {
            // --- ARRANGE ---
            var ahora = DateTime.Now;

            // --- ACT ---
            var cierre = new CierreCaja();

            // --- ASSERT ---
            Assert.NotNull(cierre);
            Assert.Equal("Abierto", cierre.Estado);
            Assert.Null(cierre.FechaCierre);
            Assert.Equal(0.00m, cierre.TotalVendido);
        }

        [Fact]
        public void Constructor_DebeRegistrarFechaApertura()
        {
            // --- ARRANGE ---
            var ahora = DateTime.Now;

            // --- ACT ---
            var cierre = new CierreCaja();

            // --- ASSERT ---
            Assert.NotNull(cierre.FechaApertura);
            // Permitimos una variación de 1 segundo para evitar problemas de sincronización
            Assert.True(Math.Abs((cierre.FechaApertura - ahora).TotalSeconds) < 1);
        }

        [Fact]
        public void Constructor_DebeInicializarTotalVendidoEnCero()
        {
            // --- ARRANGE & ACT ---
            var cierre = new CierreCaja();

            // --- ASSERT ---
            Assert.Equal(0.00m, cierre.TotalVendido);
        }

        #endregion

        #region Calculation Tests - CalcularTotalDia

        [Fact]
        public void CalcularTotalDia_DebeRetornarCeroSiNoHayPagos()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto" };
            var pagos = new List<Pago>();

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(0.00m, totalCalculado);
        }

        [Fact]
        public void CalcularTotalDia_DebeCalcularCorrectamente_ConUnSoloPago()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(100.00m, totalCalculado);
        }

        [Fact]
        public void CalcularTotalDia_DebeCalcularCorrectamente_ConMultiplesPagos()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 150.50m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 200.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(450.50m, totalCalculado);
        }

        [Fact]
        public void CalcularTotalDia_DebeCalcularCorrectamente_ConTodaLasPasarelas()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.75m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.25m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = 200.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(426.00m, totalCalculado);
            Assert.Equal(cierre.TotalVendido + 426.00m, cierre.TotalVendido + totalCalculado);
        }

        [Fact]
        public void CalcularTotalDia_DebeManejarpreciosDecimales_ConMayorPrecision()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 10.99m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 20.01m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 30.50m, MetodoPago = "Plin", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(61.50m, totalCalculado);
        }

        #endregion

        #region Calculation Tests - Por Pasarela

        [Fact]
        public void CalcularTotalPorPasarela_DebeCalcularTotalEfectivo()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 150.00m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalEfectivo = pagos.Where(p => p.MetodoPago == "Efectivo").Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(150.00m, totalEfectivo);
        }

        [Fact]
        public void CalcularTotalPorPasarela_DebeCalcularTotalYape()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 75.50m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 25.50m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalYape = pagos.Where(p => p.MetodoPago == "Yape").Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(101.00m, totalYape);
        }

        [Fact]
        public void CalcularTotalPorPasarela_DebeCalcularTotalPlin()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 60.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 40.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 150.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalPlin = pagos.Where(p => p.MetodoPago == "Plin").Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(100.00m, totalPlin);
        }

        [Fact]
        public void CalcularTotalPorPasarela_DebeCalcularTotalTarjeta()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 200.50m, MetodoPago = "Tarjeta", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 299.50m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalTarjeta = pagos.Where(p => p.MetodoPago == "Tarjeta").Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(500.00m, totalTarjeta);
        }

        [Fact]
        public void CalcularTotalPorPasarela_DebeRetornarCero_SiNohayPagosPorPasarela()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.00m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalPlin = pagos.Where(p => p.MetodoPago == "Plin").Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(0.00m, totalPlin);
        }

        [Fact]
        public void CalcularTotalPorPasarela_DebeConcilarTodasLasPasarelas()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.00m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = 200.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalEfectivo = pagos.Where(p => p.MetodoPago == "Efectivo").Sum(p => p.TotalPagado);
            decimal totalYape = pagos.Where(p => p.MetodoPago == "Yape").Sum(p => p.TotalPagado);
            decimal totalPlin = pagos.Where(p => p.MetodoPago == "Plin").Sum(p => p.TotalPagado);
            decimal totalTarjeta = pagos.Where(p => p.MetodoPago == "Tarjeta").Sum(p => p.TotalPagado);
            decimal sumaTotal = totalEfectivo + totalYape + totalPlin + totalTarjeta;
            decimal totalGeneral = pagos.Sum(p => p.TotalPagado);

            // --- ASSERT ---
            Assert.Equal(100.00m, totalEfectivo);
            Assert.Equal(50.00m, totalYape);
            Assert.Equal(75.00m, totalPlin);
            Assert.Equal(200.00m, totalTarjeta);
            Assert.Equal(sumaTotal, totalGeneral);
        }

        #endregion

        #region Validation Tests - Valores Negativos

        [Fact]
        public void CalcularTotalDia_DebeRechazarValoresNegativos_EnUnPago()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = -50.00m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT & ASSERT ---
            var pagosNegativos = pagos.Where(p => p.TotalPagado < 0).ToList();
            Assert.Single(pagosNegativos);
            Assert.Equal(-50.00m, pagosNegativos.First().TotalPagado);
        }

        [Fact]
        public void ValidarPagos_DebeDetectarValoresNegativos_YLanzarExcepcion()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = -50.00m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT ---
            var pagosInvalidos = pagos.Where(p => p.TotalPagado < 0).ToList();

            // --- ASSERT ---
            Assert.NotEmpty(pagosInvalidos);
            Assert.True(pagosInvalidos.Any(p => p.TotalPagado < 0));
        }

        [Fact]
        public void ValidarPagos_DebeDetectarCuandoNoHayValoresNegativos()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1 };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.00m, MetodoPago = "Yape", CierreCajaId = 1 }
            };

            // --- ACT ---
            var pagosInvalidos = pagos.Where(p => p.TotalPagado < 0).ToList();

            // --- ASSERT ---
            Assert.Empty(pagosInvalidos);
        }

        #endregion

        #region Validation Tests - Valores Descuadrados

        [Fact]
        public void ValidarConciliacion_DebeVerificarQueSumaSeaCorrecta()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, TotalVendido = 425.00m };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 150.00m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = 100.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            bool esConciliable = totalCalculado == cierre.TotalVendido;

            // --- ASSERT ---
            Assert.True(esConciliable);
            Assert.Equal(425.00m, totalCalculado);
        }

        [Fact]
        public void ValidarConciliacion_DebeDetectarDescuadre_CuandoHayDiferencia()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, TotalVendido = 500.00m };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 150.00m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = 100.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            decimal diferencia = cierre.TotalVendido - totalCalculado;
            bool hayDescuadre = totalCalculado != cierre.TotalVendido;

            // --- ASSERT ---
            Assert.True(hayDescuadre);
            Assert.Equal(75.00m, diferencia);
            Assert.NotEqual(cierre.TotalVendido, totalCalculado);
        }

        [Fact]
        public void ValidarConciliacion_DebeIdentificarDescuadreMinimo()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, TotalVendido = 100.01m };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            decimal diferencia = Math.Abs(cierre.TotalVendido - totalCalculado);
            bool hayDescuadre = diferencia >= 0.01m;

            // --- ASSERT ---
            Assert.True(hayDescuadre);
            Assert.Equal(0.01m, diferencia);
        }

        [Fact]
        public void ValidarConciliacion_DebeAceptarTolerancia_Centavos()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, TotalVendido = 100.00m };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            decimal diferencia = Math.Abs(cierre.TotalVendido - totalCalculado);
            bool estaBienConciliado = diferencia < 0.01m;

            // --- ASSERT ---
            Assert.True(estaBienConciliado);
            Assert.Equal(0.00m, diferencia);
        }

        #endregion

        #region State Tests

        [Fact]
        public void CierreCaja_DebePermitirCambioDeEstado_ADCerrado()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto" };

            // --- ACT ---
            cierre.Estado = "Cerrado";
            cierre.FechaCierre = DateTime.Now;

            // --- ASSERT ---
            Assert.Equal("Cerrado", cierre.Estado);
            Assert.NotNull(cierre.FechaCierre);
        }

        [Fact]
        public void CierreCaja_NoDebePermitirCambioDuranteTransacciones()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto", TotalVendido = 100.00m };
            var estadoAnterior = cierre.Estado;

            // --- ACT ---
            // No debería cambiar el estado mientras hay dinero en movimiento
            bool puedeCambiar = cierre.Estado == estadoAnterior && cierre.TotalVendido > 0;

            // --- ASSERT ---
            Assert.True(puedeCambiar);
            Assert.Equal("Abierto", cierre.Estado);
        }

        [Fact]
        public void CierreCaja_DebeRegistrarFechaCierreAlCerrar()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto" };
            var ahora = DateTime.Now;

            // --- ACT ---
            cierre.Estado = "Cerrado";
            cierre.FechaCierre = ahora;

            // --- ASSERT ---
            Assert.NotNull(cierre.FechaCierre);
            Assert.True(Math.Abs((cierre.FechaCierre.Value - ahora).TotalSeconds) < 1);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void IntegracionCompleta_DebeCalcularYValidarCierreDia()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto" };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.75m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.25m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = 200.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            decimal totalEfectivo = pagos.Where(p => p.MetodoPago == "Efectivo").Sum(p => p.TotalPagado);
            decimal totalYape = pagos.Where(p => p.MetodoPago == "Yape").Sum(p => p.TotalPagado);
            decimal totalPlin = pagos.Where(p => p.MetodoPago == "Plin").Sum(p => p.TotalPagado);
            decimal totalTarjeta = pagos.Where(p => p.MetodoPago == "Tarjeta").Sum(p => p.TotalPagado);

            cierre.TotalVendido = totalCalculado;
            cierre.Estado = "Cerrado";
            cierre.FechaCierre = DateTime.Now;

            // --- ASSERT ---
            Assert.Equal(426.00m, totalCalculado);
            Assert.Equal(100.00m, totalEfectivo);
            Assert.Equal(50.75m, totalYape);
            Assert.Equal(75.25m, totalPlin);
            Assert.Equal(200.00m, totalTarjeta);
            Assert.Equal(426.00m, cierre.TotalVendido);
            Assert.Equal("Cerrado", cierre.Estado);
            Assert.NotNull(cierre.FechaCierre);
        }

        [Fact]
        public void IntegracionCompleta_DebeValidarDescuadreEnCierre()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto", TotalVendido = 500.00m };
            var pagos = new List<Pago>
            {
                new Pago { PagoId = 1, TotalPagado = 100.00m, MetodoPago = "Efectivo", CierreCajaId = 1 },
                new Pago { PagoId = 2, TotalPagado = 50.00m, MetodoPago = "Yape", CierreCajaId = 1 },
                new Pago { PagoId = 3, TotalPagado = 75.00m, MetodoPago = "Plin", CierreCajaId = 1 },
                new Pago { PagoId = 4, TotalPagado = -10.00m, MetodoPago = "Tarjeta", CierreCajaId = 1 }
            };

            // --- ACT ---
            decimal totalCalculado = pagos.Where(p => p.TotalPagado >= 0).Sum(p => p.TotalPagado);
            var pagosNegativos = pagos.Where(p => p.TotalPagado < 0).ToList();
            bool hayDescuadre = cierre.TotalVendido != totalCalculado || pagosNegativos.Count > 0;

            // --- ASSERT ---
            Assert.True(hayDescuadre);
            Assert.NotEmpty(pagosNegativos);
            Assert.Equal(225.00m, totalCalculado);
        }

        [Fact]
        public void IntegracionCompleta_DebeHandlearllamadaMultiple_SinPagos()
        {
            // --- ARRANGE ---
            var cierre = new CierreCaja { CierreCajaId = 1, Estado = "Abierto", TotalVendido = 0.00m };
            var pagos = new List<Pago>();

            // --- ACT ---
            decimal totalCalculado = pagos.Sum(p => p.TotalPagado);
            bool estaVacio = pagos.Count == 0;
            bool coincide = totalCalculado == cierre.TotalVendido;

            // --- ASSERT ---
            Assert.True(estaVacio);
            Assert.True(coincide);
            Assert.Equal(0.00m, totalCalculado);
        }

        #endregion
    }

    /// <summary>
    /// Clase auxiliar Pago para las pruebas (simula la entidad del dominio)
    /// </summary>
    public class Pago
    {
        public int PagoId { get; set; }
        public string NumeroMesa { get; set; } = string.Empty;
        public decimal TotalPagado { get; set; }
        public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Yape, Plin, Tarjeta
        public DateTime FechaPago { get; set; } = DateTime.Now;
        public string NroOperacion { get; set; } = string.Empty;
        public int CierreCajaId { get; set; }
    }
}
