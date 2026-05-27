using GastroPro.Domain.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Web
{
    public class PedidoTests
    {
        #region Constants
        private const string NUMERO_MESA_VALIDO = "5";
        private const int CANTIDAD_VALIDA = 2;
        private const string ESTADO_DISPONIBLE = "Disponible";
        private const string ESTADO_OCUPADO = "Ocupado";
        private const string ESTADO_PENDIENTE = "Pendiente";
        private const string ESTADO_EN_COCINA = "En Cocina";
        private const string ESTADO_ENTREGADO = "Entregado";
        #endregion

        #region Constructor and Initialization Tests

        [Fact]
        public void Constructor_DebeCrearPedido_ConValoresPorDefectoSeguros()
        {
            // --- ARRANGE & ACT ---
            var pedido = new Pedido();

            // --- ASSERT ---
            Assert.NotNull(pedido);
            Assert.Equal(string.Empty, pedido.NumeroMesa);
            Assert.Equal(0, pedido.Cantidad);
            Assert.Equal(ESTADO_PENDIENTE, pedido.Estado);
            Assert.True(Math.Abs((pedido.FechaHora - DateTime.Now).TotalSeconds) < 1);
        }

        [Fact]
        public void Constructor_DebeRegistrarFechaYHoraExacta()
        {
            // --- ARRANGE ---
            var ahora = DateTime.Now;

            // --- ACT ---
            var pedido = new Pedido();

            // --- ASSERT ---
            Assert.True(Math.Abs((pedido.FechaHora - ahora).TotalSeconds) < 1);
        }

        #endregion

        #region Estado de Mesa Tests

        [Fact]
        public void AbrirComanda_DebecambiarEstadoMesaDeDisponibleAOcupado()
        {
            // --- ARRANGE ---
            var mesa = ESTADO_DISPONIBLE;
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = CANTIDAD_VALIDA,
                Estado = ESTADO_PENDIENTE,
                PlatoId = 1
            };

            // --- ACT ---
            mesa = AbrirComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(ESTADO_OCUPADO, mesa);
            Assert.Equal(NUMERO_MESA_VALIDO, pedido.NumeroMesa);
        }

        [Theory]
        [InlineData(ESTADO_DISPONIBLE, ESTADO_OCUPADO)]
        public void AbrirComanda_DebeTransicionarCorrectamenteLosEstadosDeMesa(string estadoInicial, string estadoEsperado)
        {
            // --- ARRANGE ---
            var mesa = estadoInicial;

            // --- ACT ---
            mesa = AbrirComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(estadoEsperado, mesa);
        }

        [Fact]
        public void CerrarComanda_DebeChangeEstadoMesaDeOcupadoADisponible()
        {
            // --- ARRANGE ---
            var mesa = ESTADO_OCUPADO;
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Estado = ESTADO_ENTREGADO
            };

            // --- ACT ---
            mesa = CerrarComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(ESTADO_DISPONIBLE, mesa);
        }

        [Theory]
        [InlineData(ESTADO_PENDIENTE)]
        [InlineData(ESTADO_EN_COCINA)]
        [InlineData(ESTADO_ENTREGADO)]
        public void ActualizarEstadoPedido_DebeRegistrarCambiosDeEstado(string nuevoEstado)
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = CANTIDAD_VALIDA,
                Estado = ESTADO_PENDIENTE
            };

            // --- ACT ---
            pedido.Estado = nuevoEstado;

            // --- ASSERT ---
            Assert.Equal(nuevoEstado, pedido.Estado);
        }

        #endregion

        #region Cantidad y Validación de Platos Tests

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void AgregarPlato_DebeAceptarCantidadesMayoresACero(int cantidadValida)
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                PlatoId = 1
            };

            // --- ACT ---
            pedido.Cantidad = cantidadValida;
            var esValido = ValidarCantidad(pedido.Cantidad);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(cantidadValida, pedido.Cantidad);
        }

        [Theory]
        [InlineData(0, "La cantidad debe ser mayor a cero")]
        [InlineData(-1, "La cantidad debe ser mayor a cero")]
        [InlineData(-10, "La cantidad debe ser mayor a cero")]
        public void AgregarPlato_DebeRechazarCantidadesMenorOIgualACero(int cantidadInvalida, string mensajeEsperado)
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                PlatoId = 1
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarCantidadConExcepcion(cantidadInvalida));
            Assert.Equal(mensajeEsperado, excepcion.Message);
        }

        [Fact]
        public void AgregarPlato_DebeRechazarCantidadCeroConMensajeClaro()
        {
            // --- ARRANGE ---
            int cantidadCero = 0;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarCantidadConExcepcion(cantidadCero));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void AgregarPlato_DebeRechazarCantidadNegativaConMensajeClaro()
        {
            // --- ARRANGE ---
            int cantidadNegativa = -5;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarCantidadConExcepcion(cantidadNegativa));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void AgregarPlato_DebeRechazarPedidoSiCantidadEsInvalida()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = -3,
                PlatoId = 1,
                Estado = ESTADO_PENDIENTE
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("cantidad", excepcion.Message.ToLower());
        }

        #endregion

        #region Validación Integral del Pedido Tests

        [Fact]
        public void CrearPedido_DebeSerValidoConTodosLosCamposCorrectos()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = CANTIDAD_VALIDA,
                Estado = ESTADO_PENDIENTE,
                PlatoId = 1
            };

            // --- ACT ---
            var esValido = ValidarPedidoCompleto(pedido);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.NotEmpty(pedido.NumeroMesa);
            Assert.True(pedido.Cantidad > 0);
            Assert.NotEmpty(pedido.Estado);
            Assert.True(pedido.PlatoId > 0);
        }

        [Fact]
        public void CrearPedido_DebeFallarSiMesaEstaVacia()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = string.Empty,
                Cantidad = CANTIDAD_VALIDA,
                PlatoId = 1
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("mesa", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPedido_DebeFallarSiPlatoIdEsInvalido()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = CANTIDAD_VALIDA,
                PlatoId = 0
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("plato", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPedido_DebeFallarSiCantidadEsCeroYMesaValida()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = 0,
                PlatoId = 1
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("cantidad", excepcion.Message.ToLower());
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public void Pedido_DebePermitirModificarNumeroMesa()
        {
            // --- ARRANGE ---
            var pedido = new Pedido { NumeroMesa = "1" };

            // --- ACT ---
            pedido.NumeroMesa = "10";

            // --- ASSERT ---
            Assert.Equal("10", pedido.NumeroMesa);
        }

        [Fact]
        public void Pedido_DebePermitirModificarCantidad()
        {
            // --- ARRANGE ---
            var pedido = new Pedido { Cantidad = 1 };

            // --- ACT ---
            pedido.Cantidad = 5;

            // --- ASSERT ---
            Assert.Equal(5, pedido.Cantidad);
        }

        [Fact]
        public void Pedido_DebePermitirModificarEstado()
        {
            // --- ARRANGE ---
            var pedido = new Pedido { Estado = ESTADO_PENDIENTE };

            // --- ACT ---
            pedido.Estado = ESTADO_EN_COCINA;

            // --- ASSERT ---
            Assert.Equal(ESTADO_EN_COCINA, pedido.Estado);
        }

        [Fact]
        public void ValidarCantidad_DebeAceptarCantidadUnitaria()
        {
            // --- ARRANGE ---
            int cantidad = 1;

            // --- ACT ---
            var esValido = ValidarCantidad(cantidad);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarCantidad_DebeAceptarCantidadMuyAlta()
        {
            // --- ARRANGE ---
            int cantidad = 1000;

            // --- ACT ---
            var esValido = ValidarCantidad(cantidad);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Theory]
        [InlineData(ESTADO_PENDIENTE)]
        [InlineData(ESTADO_EN_COCINA)]
        [InlineData(ESTADO_ENTREGADO)]
        public void ActualizarEstadoPedido_DebeAceptarTodosLosEstadosValidos(string estado)
        {
            // --- ARRANGE ---
            var pedido = new Pedido();

            // --- ACT ---
            pedido.Estado = estado;

            // --- ASSERT ---
            Assert.Equal(estado, pedido.Estado);
        }

        [Theory]
        [InlineData("1", "2", "3")]
        [InlineData("A1", "A2", "A3")]
        [InlineData("Mesa 1", "Mesa 2", "Mesa 3")]
        public void AbrirComanda_DebePermitirVariosNumerosDeMesa(string mesa1, string mesa2, string mesa3)
        {
            // --- ARRANGE ---
            var pedido1 = new Pedido { NumeroMesa = mesa1 };
            var pedido2 = new Pedido { NumeroMesa = mesa2 };
            var pedido3 = new Pedido { NumeroMesa = mesa3 };

            // --- ACT ---
            var estadoMesa1 = AbrirComanda(ESTADO_DISPONIBLE);
            var estadoMesa2 = AbrirComanda(ESTADO_DISPONIBLE);
            var estadoMesa3 = AbrirComanda(ESTADO_DISPONIBLE);

            // --- ASSERT ---
            Assert.Equal(ESTADO_OCUPADO, estadoMesa1);
            Assert.Equal(ESTADO_OCUPADO, estadoMesa2);
            Assert.Equal(ESTADO_OCUPADO, estadoMesa3);
        }

        [Fact]
        public void CerrarComanda_DebeRetornarDisponibleDesdeDiferentesEstados()
        {
            // --- ARRANGE ---
            var mesa = ESTADO_OCUPADO;

            // --- ACT ---
            var estadoFinal = CerrarComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(ESTADO_DISPONIBLE, estadoFinal);
        }

        [Fact]
        public void ValidarPedidoCompleto_DebeAceptarPedidoConTodosLosCamposValidos()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                PedidoId = 1,
                NumeroMesa = "5",
                Cantidad = 3,
                Estado = ESTADO_EN_COCINA,
                PlatoId = 2,
                FechaHora = DateTime.Now
            };

            // --- ACT ---
            var esValido = ValidarPedidoCompleto(pedido);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(1, pedido.PedidoId);
            Assert.Equal(2, pedido.PlatoId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(50)]
        [InlineData(100)]
        public void ValidarCantidad_DebeAceptarDiferentesCantidades(int cantidad)
        {
            // --- ARRANGE & ACT ---
            var esValido = ValidarCantidad(cantidad);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ValidarCantidad_DebeRechazarCantidadesInvalidas(int cantidad)
        {
            // --- ARRANGE & ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarCantidadConExcepcion(cantidad));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarPedidoCompleto_DebeFallarSiMesaEsNull()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = null,
                Cantidad = CANTIDAD_VALIDA,
                PlatoId = 1
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("mesa", excepcion.Message.ToLower());
        }

        [Fact]
        public void ValidarPedidoCompleto_DebeFallarSiPedidoEsNull()
        {
            // --- ARRANGE ---
            Pedido pedido = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Equal("pedido", excepcion.ParamName);
        }

        [Fact]
        public void AbrirComanda_NoDebeModificarEstadoSiNoEsDisponible()
        {
            // --- ARRANGE ---
            var mesa = ESTADO_OCUPADO;

            // --- ACT ---
            var estadoResultante = AbrirComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(ESTADO_OCUPADO, estadoResultante); // No cambia si ya está ocupado
        }

        [Fact]
        public void CerrarComanda_NoDebeModificarEstadoSiNoEsOcupado()
        {
            // --- ARRANGE ---
            var mesa = ESTADO_DISPONIBLE;

            // --- ACT ---
            var estadoResultante = CerrarComanda(mesa);

            // --- ASSERT ---
            Assert.Equal(ESTADO_DISPONIBLE, estadoResultante); // No cambia si ya está disponible
        }

        [Fact]
        public void Pedido_DebeInicializarPedidoIdEnCero()
        {
            // --- ARRANGE & ACT ---
            var pedido = new Pedido();

            // --- ASSERT ---
            Assert.Equal(0, pedido.PedidoId);
        }

        [Fact]
        public void Pedido_DebePermitirAsignarPlatoId()
        {
            // --- ARRANGE ---
            var pedido = new Pedido();

            // --- ACT ---
            pedido.PlatoId = 5;

            // --- ASSERT ---
            Assert.Equal(5, pedido.PlatoId);
        }

        [Theory]
        [InlineData("1", 1, ESTADO_PENDIENTE, 1)]
        [InlineData("2", 2, ESTADO_EN_COCINA, 2)]
        [InlineData("3", 3, ESTADO_ENTREGADO, 3)]
        public void ValidarPedidoCompleto_DebeAceptarVariasConfiguraciones(string mesa, int cantidad, string estado, int platoId)
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = mesa,
                Cantidad = cantidad,
                Estado = estado,
                PlatoId = platoId
            };

            // --- ACT ---
            var esValido = ValidarPedidoCompleto(pedido);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarCantidad_DebeRechazarCantidadExtremamentNegativa()
        {
            // --- ARRANGE ---
            int cantidad = int.MinValue;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarCantidadConExcepcion(cantidad));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarPedidoCompleto_DebeFallarSiEstadoEstaVacio()
        {
            // --- ARRANGE ---
            var pedido = new Pedido
            {
                NumeroMesa = NUMERO_MESA_VALIDO,
                Cantidad = CANTIDAD_VALIDA,
                Estado = string.Empty,
                PlatoId = 1
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPedidoCompletoConExcepcion(pedido));
            Assert.Contains("estado", excepcion.Message.ToLower());
        }

        #endregion

        #region Métodos Privados de Validación

        /// <summary>
        /// Simula la apertura de una comanda y cambio de estado de mesa de Disponible a Ocupado.
        /// </summary>
        private string AbrirComanda(string estadoMesa)
        {
            if (estadoMesa == ESTADO_DISPONIBLE)
            {
                return ESTADO_OCUPADO;
            }
            return estadoMesa;
        }

        /// <summary>
        /// Simula el cierre de una comanda y cambio de estado de mesa de Ocupado a Disponible.
        /// </summary>
        private string CerrarComanda(string estadoMesa)
        {
            if (estadoMesa == ESTADO_OCUPADO)
            {
                return ESTADO_DISPONIBLE;
            }
            return estadoMesa;
        }

        /// <summary>
        /// Valida que la cantidad de platos sea mayor a cero.
        /// </summary>
        private bool ValidarCantidad(int cantidad)
        {
            return cantidad > 0;
        }

        /// <summary>
        /// Valida que la cantidad sea mayor a cero. Lanza ArgumentException si es inválida.
        /// </summary>
        private void ValidarCantidadConExcepcion(int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero");
            }
        }

        /// <summary>
        /// Valida que un pedido sea completo y válido (sin excepciones).
        /// </summary>
        private bool ValidarPedidoCompleto(Pedido pedido)
        {
            return pedido != null &&
                   !string.IsNullOrEmpty(pedido.NumeroMesa) &&
                   pedido.Cantidad > 0 &&
                   !string.IsNullOrEmpty(pedido.Estado) &&
                   pedido.PlatoId > 0;
        }

        /// <summary>
        /// Valida que un pedido sea completo y válido. Lanza excepciones si hay errores.
        /// </summary>
        private void ValidarPedidoCompletoConExcepcion(Pedido pedido)
        {
            if (pedido == null)
            {
                throw new ArgumentNullException(nameof(pedido));
            }

            if (string.IsNullOrEmpty(pedido.NumeroMesa))
            {
                throw new ArgumentException("El número de mesa es obligatorio");
            }

            if (pedido.Cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero");
            }

            if (pedido.PlatoId <= 0)
            {
                throw new ArgumentException("El identificador del plato es inválido");
            }

            if (string.IsNullOrEmpty(pedido.Estado))
            {
                throw new ArgumentException("El estado del pedido es obligatorio");
            }
        }

        #endregion
    }
}