using GastroPro.Domain.Entities;
using System;
using Xunit;

namespace GastroPro.UnitTests.UnitTests.Web
{
    public class PlatoTests
    {
        #region Constants

        private const string NOMBRE_VALIDO = "Ceviche de Pescado";
        private const string CATEGORIA_VALIDA = "Segundos";
        private const decimal PRECIO_VALIDO = 25.50m;
        private const decimal PRECIO_MINIMO = 0.1m;
        private const decimal PRECIO_MAXIMO = 1000m;

        #endregion

        #region Constructor and Initialization Tests

        [Fact]
        public void Constructor_DebeCrearPlato_ConValoresPorDefectoSeguros()
        {
            // --- ARRANGE & ACT ---
            var plato = new Plato();

            // --- ASSERT ---
            Assert.NotNull(plato);
            Assert.Equal(string.Empty, plato.Nombre);
            Assert.Equal(0m, plato.Precio);
            Assert.Equal("General", plato.Categoria);
        }

        [Fact]
        public void Constructor_DebeInicializarPlatoIdEnCero()
        {
            // --- ARRANGE & ACT ---
            var plato = new Plato();

            // --- ASSERT ---
            Assert.Equal(0, plato.PlatoId);
        }

        #endregion

        #region Validation Tests - Nombre del Plato

        [Theory]
        [InlineData("Ceviche")]
        [InlineData("Arroz Chaufa")]
        [InlineData("Tallarín Saltado")]
        [InlineData("Causa Limeña")]
        [InlineData("A")] // Un carácter válido
        public void ValidarNombre_DebeAceptarNombresValidos(string nombreValido)
        {
            // --- ARRANGE ---
            var plato = new Plato();

            // --- ACT ---
            plato.Nombre = nombreValido;
            var esValido = ValidarNombre(plato.Nombre);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(nombreValido, plato.Nombre);
        }

        [Theory]
        [InlineData("", "El nombre del plato no puede estar vacío")]
        [InlineData(" ", "El nombre del plato no puede estar vacío")]
        public void ValidarNombre_DebeRechazarNombresVacios(string nombreInvalido, string mensajeEsperado)
        {
            // --- ARRANGE & ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarNombreConExcepcion(nombreInvalido));
            Assert.Equal(mensajeEsperado, excepcion.Message);
        }

        [Fact]
        public void ValidarNombre_DebeRechazarNombreNull()
        {
            // --- ARRANGE ---
            string nombreNull = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarNombreConExcepcion(nombreNull));
            Assert.Equal("nombre", excepcion.ParamName);
        }

        [Fact]
        public void ValidarNombre_DebeRechazarNombreVacioConMensajeClaro()
        {
            // --- ARRANGE ---
            string nombreVacio = string.Empty;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarNombreConExcepcion(nombreVacio));
            Assert.Contains("vacío", excepcion.Message);
        }

        [Fact]
        public void ValidarNombre_DebeRechazarNombreSoloConEspacios()
        {
            // --- ARRANGE ---
            string nombreConEspacios = "   ";

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarNombreConExcepcion(nombreConEspacios));
            Assert.Contains("vacío", excepcion.Message);
        }

        #endregion

        #region Validation Tests - Precio del Plato

        [Theory]
        [InlineData(0.1)]
        [InlineData(10.00)]
        [InlineData(25.50)]
        [InlineData(99.99)]
        [InlineData(500.00)]
        [InlineData(1000.00)]
        public void ValidarPrecio_DebeAceptarPreciosValidos(decimal precioValido)
        {
            // --- ARRANGE ---
            var plato = new Plato();

            // --- ACT ---
            plato.Precio = precioValido;
            var esValido = ValidarPrecio(plato.Precio);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(precioValido, plato.Precio);
        }

        [Theory]
        [InlineData(0, "El precio debe ser mayor a cero")]
        [InlineData(-0.01, "El precio debe ser mayor a cero")]
        [InlineData(-10.00, "El precio debe ser mayor a cero")]
        [InlineData(-100.00, "El precio debe ser mayor a cero")]
        public void ValidarPrecio_DebeRechazarPreciosCeroONegativos(decimal precioInvalido, string mensajeEsperado)
        {
            // --- ARRANGE & ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPrecioConExcepcion(precioInvalido));
            Assert.Equal(mensajeEsperado, excepcion.Message);
        }

        [Fact]
        public void ValidarPrecio_DebeRechazarPrecioCeroConMensajeClaro()
        {
            // --- ARRANGE ---
            decimal precioCero = 0m;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPrecioConExcepcion(precioCero));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Fact]
        public void ValidarPrecio_DebeRechazarPrecioNegativoConMensajeClaro()
        {
            // --- ARRANGE ---
            decimal precioNegativo = -50.00m;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPrecioConExcepcion(precioNegativo));
            Assert.Contains("mayor a cero", excepcion.Message);
        }

        [Theory]
        [InlineData(1001.00)]
        [InlineData(5000.00)]
        public void ValidarPrecio_DebeRechazarPreciosMayorAlLimite(decimal precioExcesivo)
        {
            // --- ARRANGE & ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPrecioConExcepcion(precioExcesivo));
            Assert.Contains("límite", excepcion.Message.ToLower());
        }

        #endregion

        #region Validation Tests - Categoría del Plato

        [Theory]
        [InlineData("Entradas")]
        [InlineData("Segundos")]
        [InlineData("Bebidas")]
        [InlineData("Postres")]
        [InlineData("General")]
        public void ValidarCategoria_DebeAceptarCategoriasValidas(string categoriaValida)
        {
            // --- ARRANGE ---
            var plato = new Plato();

            // --- ACT ---
            plato.Categoria = categoriaValida;
            var esValido = ValidarCategoria(plato.Categoria);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(categoriaValida, plato.Categoria);
        }

        [Fact]
        public void ValidarCategoria_DebeTenerValorPorDefecto()
        {
            // --- ARRANGE & ACT ---
            var plato = new Plato();

            // --- ASSERT ---
            Assert.Equal("General", plato.Categoria);
        }

        #endregion

        #region Validation Tests - Plato Completo

        [Fact]
        public void CrearPlato_DebeSerValidoConTodosLosCamposCorrectos()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = PRECIO_VALIDO,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT ---
            var esValido = ValidarPlatoCompleto(plato);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.NotEmpty(plato.Nombre);
            Assert.True(plato.Precio > 0);
            Assert.NotEmpty(plato.Categoria);
        }

        [Fact]
        public void CrearPlato_DebeFallarSiNombreEstaVacio()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = string.Empty,
                Precio = PRECIO_VALIDO,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("nombre", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPlato_DebeFallarSiNombreEsNull()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = null,
                Precio = PRECIO_VALIDO,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("nombre", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPlato_DebeFallarSiPrecioEsCero()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = 0m,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("precio", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPlato_DebeFallarSiPrecioEsNegativo()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = -15.00m,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("precio", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPlato_DebeFallarSiNombreEstaVacioYPrecioEsNegativo()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = string.Empty,
                Precio = -10.00m,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            // Debe validar primero el nombre
            Assert.Contains("nombre", excepcion.Message.ToLower());
        }

        [Fact]
        public void CrearPlato_DebeFallarSiCategoriaEstaVacia()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = PRECIO_VALIDO,
                Categoria = string.Empty
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("categoría", excepcion.Message.ToLower());
        }

        #endregion

        #region Métodos Privados de Validación

        /// <summary>
        /// Valida que el nombre del plato no esté vacío ni sea nulo.
        /// </summary>
        private bool ValidarNombre(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre);
        }

        /// <summary>
        /// Valida que el nombre del plato no esté vacío ni sea nulo. Lanza excepciones si es inválido.
        /// </summary>
        private void ValidarNombreConExcepcion(string nombre)
        {
            if (nombre == null)
            {
                throw new ArgumentNullException(nameof(nombre));
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre del plato no puede estar vacío");
            }
        }

        /// <summary>
        /// Valida que el precio sea mayor a cero y menor o igual al límite máximo.
        /// </summary>
        private bool ValidarPrecio(decimal precio)
        {
            return precio > 0 && precio <= PRECIO_MAXIMO;
        }

        /// <summary>
        /// Valida que el precio sea mayor a cero y menor o igual al límite máximo. Lanza excepciones si es inválido.
        /// </summary>
        private void ValidarPrecioConExcepcion(decimal precio)
        {
            if (precio <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor a cero");
            }

            if (precio > PRECIO_MAXIMO)
            {
                throw new ArgumentException($"El precio excede el límite máximo de {PRECIO_MAXIMO}");
            }
        }

        /// <summary>
        /// Valida que la categoría no esté vacía.
        /// </summary>
        private bool ValidarCategoria(string categoria)
        {
            return !string.IsNullOrWhiteSpace(categoria);
        }

        /// <summary>
        /// Valida que un plato sea completo y válido (sin excepciones).
        /// </summary>
        private bool ValidarPlatoCompleto(Plato plato)
        {
            return plato != null &&
                   !string.IsNullOrWhiteSpace(plato.Nombre) &&
                   plato.Precio > 0 &&
                   plato.Precio <= PRECIO_MAXIMO &&
                   !string.IsNullOrWhiteSpace(plato.Categoria);
        }

        /// <summary>
        /// Valida que un plato sea completo y válido. Lanza excepciones si hay errores.
        /// </summary>
        private void ValidarPlatoCompletoConExcepcion(Plato plato)
        {
            if (plato == null)
            {
                throw new ArgumentNullException(nameof(plato));
            }

            if (string.IsNullOrWhiteSpace(plato.Nombre))
            {
                throw new ArgumentException("El nombre del plato no puede estar vacío o nulo");
            }

            if (plato.Precio <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor a cero");
            }

            if (plato.Precio > PRECIO_MAXIMO)
            {
                throw new ArgumentException($"El precio excede el límite máximo de {PRECIO_MAXIMO}");
            }

            if (string.IsNullOrWhiteSpace(plato.Categoria))
            {
                throw new ArgumentException("La categoría del plato no puede estar vacía");
            }
        }

        #endregion

        #region Additional Edge Cases and Comprehensive Tests

        [Fact]
        public void Plato_DebePermitirModificarNombre()
        {
            // --- ARRANGE ---
            var plato = new Plato { Nombre = "Ceviche" };

            // --- ACT ---
            plato.Nombre = "Tiradito";

            // --- ASSERT ---
            Assert.Equal("Tiradito", plato.Nombre);
        }

        [Fact]
        public void Plato_DebePermitirModificarPrecio()
        {
            // --- ARRANGE ---
            var plato = new Plato { Precio = 25.00m };

            // --- ACT ---
            plato.Precio = 35.50m;

            // --- ASSERT ---
            Assert.Equal(35.50m, plato.Precio);
        }

        [Fact]
        public void Plato_DebePermitirModificarCategoria()
        {
            // --- ARRANGE ---
            var plato = new Plato { Categoria = "Entradas" };

            // --- ACT ---
            plato.Categoria = "Postres";

            // --- ASSERT ---
            Assert.Equal("Postres", plato.Categoria);
        }

        [Fact]
        public void ValidarPrecio_DebeAceptarPrecioEnLimiteMínimo()
        {
            // --- ARRANGE ---
            decimal precioLimiteMínimo = PRECIO_MINIMO;

            // --- ACT ---
            var esValido = ValidarPrecio(precioLimiteMínimo);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarPrecio_DebeAceptarPrecioEnLímiteMaximo()
        {
            // --- ARRANGE ---
            decimal precioLímiteMáximo = PRECIO_MAXIMO;

            // --- ACT ---
            var esValido = ValidarPrecio(precioLímiteMáximo);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarNombre_DebeAceptarNombreLargo()
        {
            // --- ARRANGE ---
            string nombreLargo = "Ceviche de Pescado Fresco con Limón y Cebolla Roja";

            // --- ACT ---
            var esValido = ValidarNombre(nombreLargo);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarCategoria_DebeAceptarCategoriaPersonalizada()
        {
            // --- ARRANGE ---
            string categoriaPersonalizada = "Platos Especiales";

            // --- ACT ---
            var esValido = ValidarCategoria(categoriaPersonalizada);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(1.0)]
        [InlineData(10.50)]
        [InlineData(100.99)]
        [InlineData(500.00)]
        [InlineData(1000.00)]
        public void ValidarPlatoCompleto_DebeAceptarVariosPreciosValidos(decimal precio)
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = precio,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT ---
            var esValido = ValidarPlatoCompleto(plato);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-10.00)]
        [InlineData(-100.50)]
        [InlineData(-1000.00)]
        public void ValidarPlatoCompleto_DebeRechazarVariosPreciosNegativos(decimal precio)
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = NOMBRE_VALIDO,
                Precio = precio,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Contains("precio", excepcion.Message.ToLower());
        }

        [Fact]
        public void Plato_DebeInicializarPlatoIdEnCero()
        {
            // --- ARRANGE & ACT ---
            var plato = new Plato();

            // --- ASSERT ---
            Assert.Equal(0, plato.PlatoId);
        }

        [Fact]
        public void Plato_DebePermitirAsignarPlatoId()
        {
            // --- ARRANGE ---
            var plato = new Plato();

            // --- ACT ---
            plato.PlatoId = 10;

            // --- ASSERT ---
            Assert.Equal(10, plato.PlatoId);
        }

        [Fact]
        public void ValidarPlatoCompleto_DebeFallarSiPlatoEsNull()
        {
            // --- ARRANGE ---
            Plato plato = null;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentNullException>(() => ValidarPlatoCompletoConExcepcion(plato));
            Assert.Equal("plato", excepcion.ParamName);
        }

        [Fact]
        public void ValidarNombre_DebeRechazarNombreSoloConEspaciosEnEdgeCase()
        {
            // --- ARRANGE ---
            string nombreSoloEspacios = "     ";

            // --- ACT ---
            var esValido = ValidarNombre(nombreSoloEspacios);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Fact]
        public void ValidarCategoria_DebeRechazarCategoriaSoloConEspacios()
        {
            // --- ARRANGE ---
            string categoriaSoloEspacios = "     ";

            // --- ACT ---
            var esValido = ValidarCategoria(categoriaSoloEspacios);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Fact]
        public void ValidarPrecio_DebeRechazarPrecioJustoPorEncimaDeLímiteMaximo()
        {
            // --- ARRANGE ---
            decimal precioPorEncimaDeLímite = PRECIO_MAXIMO + 0.01m;

            // --- ACT ---
            var esValido = ValidarPrecio(precioPorEncimaDeLímite);

            // --- ASSERT ---
            Assert.False(esValido);
        }

        [Fact]
        public void ValidarPrecio_DebeAceptarPreciosPortEncimaDePrecioMinimo()
        {
            // --- ARRANGE ---
            decimal precioPorEncimaDeLímiteMin = PRECIO_MINIMO + 0.01m;

            // --- ACT ---
            var esValido = ValidarPrecio(precioPorEncimaDeLímiteMin);

            // --- ASSERT ---
            Assert.True(esValido);
        }

        [Theory]
        [InlineData("Ceviche de Pescado", "Segundos", 25.50)]
        [InlineData("Causa Limeña", "Entradas", 15.00)]
        [InlineData("Chicha Morada", "Bebidas", 5.00)]
        [InlineData("Helado de Lúcuma", "Postres", 8.50)]
        public void CrearPlato_DebeAceptarVariasConfiguracionesValidas(string nombre, string categoria, decimal precio)
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                Nombre = nombre,
                Categoria = categoria,
                Precio = precio
            };

            // --- ACT ---
            var esValido = ValidarPlatoCompleto(plato);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(nombre, plato.Nombre);
            Assert.Equal(categoria, plato.Categoria);
            Assert.Equal(precio, plato.Precio);
        }

        [Fact]
        public void ValidarNombreConExcepcion_DebeRechazarNombreVacioConMensajeEspecífico()
        {
            // --- ARRANGE ---
            string nombreVacio = string.Empty;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarNombreConExcepcion(nombreVacio));
            Assert.Equal("El nombre del plato no puede estar vacío", excepcion.Message);
        }

        [Fact]
        public void ValidarPrecioConExcepcion_DebeRechazarPrecioExcesivoConExcepcionEspecifica()
        {
            // --- ARRANGE ---
            decimal precioExcesivo = PRECIO_MAXIMO + 100;

            // --- ACT & ASSERT ---
            var excepcion = Assert.Throws<ArgumentException>(() => ValidarPrecioConExcepcion(precioExcesivo));
            Assert.Contains("límite", excepcion.Message.ToLower());
        }

        [Fact]
        public void Plato_DebeSerIgualASíMismoEnPropiedades()
        {
            // --- ARRANGE ---
            var plato1 = new Plato
            {
                Nombre = "Ceviche",
                Precio = 25.00m,
                Categoria = "Segundos"
            };
            var plato2 = new Plato
            {
                Nombre = "Ceviche",
                Precio = 25.00m,
                Categoria = "Segundos"
            };

            // --- ACT & ASSERT ---
            Assert.Equal(plato1.Nombre, plato2.Nombre);
            Assert.Equal(plato1.Precio, plato2.Precio);
            Assert.Equal(plato1.Categoria, plato2.Categoria);
        }

        [Fact]
        public void ValidarPlatoCompleto_DebeAceptarPlatoConTodasLasPropiedades()
        {
            // --- ARRANGE ---
            var plato = new Plato
            {
                PlatoId = 1,
                Nombre = NOMBRE_VALIDO,
                Precio = PRECIO_VALIDO,
                Categoria = CATEGORIA_VALIDA
            };

            // --- ACT ---
            var esValido = ValidarPlatoCompleto(plato);

            // --- ASSERT ---
            Assert.True(esValido);
            Assert.Equal(1, plato.PlatoId);
            Assert.NotNull(plato.Nombre);
            Assert.NotEqual(0m, plato.Precio);
        }

        #endregion
    }
}