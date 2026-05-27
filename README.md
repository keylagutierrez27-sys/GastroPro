# GastroPro: Sistema Integrado de Control Digital de Comandas y Gestión de Pagos
 
> Desarrollado por **Keyla Jhazym Gutiérrez Gutiérrez** — IS 489: Pruebas y Aseguramiento de la Calidad
> Universidad Nacional San Cristóbal de Huamanga · Ayacucho, Perú · 2026
 
---
 
## Tabla de Contenidos
 
1. [Introducción](#1-introducción)
2. [Objetivos Estratégicos](#2-objetivos-estratégicos)
3. [Público Objetivo](#3-público-objetivo)
4. [Alcance y Limitaciones](#4-alcance-y-limitaciones)
5. [Tecnologías Utilizadas](#5-tecnologías-utilizadas)
6. [Arquitectura del Sistema](#6-arquitectura-del-sistema)
7. [Metodología de Desarrollo](#7-metodología-de-desarrollo)
8. [Requisitos del Sistema](#8-requisitos-del-sistema)
9. [Estructura del Proyecto](#9-estructura-del-proyecto)
10. [Flujo Operativo en Vivo](#10-flujo-operativo-en-vivo)
11. [Cobertura de Código y Pruebas Unitarias](#11-cobertura-de-código-y-pruebas-unitarias)
12. [Problemas Encontrados y Soluciones](#12-problemas-encontrados-y-soluciones)
---
 
## 1. Introducción
 
En el sector gastronómico actual, la eficiencia operativa y el control preciso de las transacciones financieras son pilares fundamentales para la sostenibilidad de cualquier negocio. La gestión manual de comandas, el cierre de caja y la facturación son procesos propensos a errores humanos, duplicidad de registros y falta de visibilidad en tiempo real sobre el flujo de efectivo.
 
**GastroPro** nace como respuesta técnica a esta necesidad: automatizar y centralizar el flujo operativo de un restaurante. Construida sobre ASP.NET Core en .NET 10, la plataforma implementa un modelo de datos robusto que vincula la toma de pedidos, el seguimiento de mesas y la conciliación de cierres de caja bajo una estructura de integridad referencial estricta.
 
El sistema funciona como un núcleo operativo donde se entrelazan la atención en salón y el control financiero. Su valor diferencial reside en el módulo de facturación y arqueo de caja: cada transacción monetaria se vincula a un identificador único de jornada financiera (`CierreCajaId`), permitiendo que el administrador audite de forma transparente el total vendido, mitigando errores humanos y pérdidas de capital en el cuadre diario.
 
| Métrica | Valor |
|---|---|
| Pruebas unitarias ejecutadas | 298 |
| Errores en suite | 0 |
| Cobertura del dominio | 100% |
| Cobertura global | >90% |
| Capas desacopladas | 3 |
 
---
 
## 2. Objetivos Estratégicos
 
**Objetivo General:** Desarrollar e implementar una aplicación web de gestión comercial y control financiero bajo el enfoque de Clean Architecture y DDD en .NET 10, con el fin de automatizar los procesos operativos, optimizar el control de inventarios y garantizar la consistencia e integridad de los flujos de caja del negocio gastronómico.
 
**Objetivos Específicos:**
 
- Diseñar una arquitectura desacoplada de tres capas en .NET 10, asegurando la inversión de dependencias para que las reglas de negocio permanezcan aisladas de los componentes de infraestructura.
- Implementar el modelo relacional en SQL Server 2022 mediante Entity Framework Core Code-First, configurando explícitamente restricciones de llaves foráneas e integridad referencial para evitar transacciones financieras huérfanas.
- Desarrollar la interfaz bajo el patrón MVC con control perimetral de seguridad basado en roles (Administrador, Cajero, Mozo) y manejo de sesiones seguras.
- Garantizar la calidad del software mediante pruebas unitarias con xUnit bajo el patrón AAA, empleando ingeniería asistida por IA para la identificación proactiva de datos nulos o anómalos.
- Optimizar el flujo de trabajo operativo (control de mesas, comandas digitales, registro de consumos y emisión de reportes de pago), reduciendo tiempos de atención y mitigando fugas de capital.
---
 
## 3. Público Objetivo
 
GastroPro contempla dos perfiles de usuario claramente diferenciados:
 
**Usuarios Operativos (Personal de Salón y Caja)**
Mozos y cajeros del establecimiento. Los mozos interactúan con la interfaz táctil para la apertura de mesas y el envío de pedidos a cocina. El cajero utiliza el sistema como herramienta de cobro y arqueo continuo, con un entorno optimizado para horas punta.
 
**Usuarios Administradores (Propietarios y Gerentes)**
Perfil estratégico con acceso exclusivo a la configuración de seguridad, gestión de cuentas de usuario, manipulación del catálogo de platos y auditoría analítica de los cierres de caja para la toma de decisiones financieras.
 
---
 
## 4. Alcance y Limitaciones
 
### Funcionalidades Implementadas
 
| Módulo | Descripción |
|---|---|
| **Seguridad y Autenticación** | Control de acceso por roles (Administrador, Mozo, Cajero) mediante sesiones seguras en el servidor. |
| **Gestión de Salón y Pedidos** | Registro y actualización en tiempo real de consumos por mesa, con seguimiento del estado de preparación. |
| **Facturación y Medios de Pago** | Procesamiento de pagos mediante Efectivo, Yape, Plin y Tarjeta, con captura obligatoria del número de operación para transacciones electrónicas. |
| **Auditoría y Cierre de Caja** | Consolidación de ingresos por turno, con restricción de transacciones a jornadas previamente aperturadas y cálculo automático del total vendido. |
 
### Funcionalidades Fuera del Alcance (Versión Actual)
 
- **Gestión de Inventario en Tiempo Real:** El modelo de datos contempla la relación Platos-Insumos, pero no ejecuta la deducción automática de stock al confirmar una comanda.
- **Integración con Pasarelas Bancarias Externas:** Los pagos se gestionan internamente; no existe conexión directa con APIs de tarjetas de crédito.
- **Facturación Electrónica (SUNAT/SRI):** La arquitectura modular está preparada para esta implementación, pero no incluida en esta versión.
- **Dashboard de Analítica Predictiva:** Los datos están estructurados para análisis, pero no existe un motor de ML integrado en la interfaz.
- **Sincronización Offline:** La aplicación requiere conexión estable con el servidor SQL; no implementa estrategias offline-first.
---
 
## 5. Tecnologías Utilizadas
 
| Componente | Tecnología |
|---|---|
| Plataforma | .NET 10 |
| Lenguaje | C# |
| IDE | Visual Studio 2026 |
| Framework Web | ASP.NET Core MVC |
| ORM | Entity Framework Core (Code-First) |
| Base de Datos | SQL Server 2022 + SSMS |
| UI | Bootstrap 5 (Mobile-First) |
| Vistas | Razor Pages (.cshtml) |
| Framework de Pruebas | xUnit (.NET 10) |
| Mocking | Moq |
| Asistencia IA | GitHub Copilot + Gemini IA |
 
---
 
## 6. Arquitectura del Sistema
 
GastroPro implementa **Clean Architecture** combinada con **Domain-Driven Design (DDD)**, lo que garantiza que la lógica de negocio permanezca completamente aislada de los detalles técnicos (base de datos, framework web). Esto permite que el sistema evolucione ante cambios tecnológicos sin comprometer las reglas fundamentales del dominio.
 
### Capas del Sistema
 
```
┌─────────────────────────────────────────────┐
│              GastroPro.Web (MVC)            │  ← Capa de Presentación
│   Controllers · Views (Razor) · Bootstrap   │
└────────────────────┬────────────────────────┘
                     │ depende de
┌────────────────────▼────────────────────────┐
│           GastroPro.Domain (Núcleo)         │  ← Lógica de Negocio (DDD)
│   Entidades: Pago · Pedido · CierreCaja     │
│   Plato · Usuario · Interfaz IUnitOfWork    │
└────────────────────┬────────────────────────┘
                     │ implementado por
┌────────────────────▼────────────────────────┐
│        GastroPro.Infrastructure             │  ← Persistencia de Datos
│   EF Core · GastroProDbContext              │
│   Repository Pattern · Unit of Work        │
│   Migraciones → SQL Server 2022            │
└─────────────────────────────────────────────┘
                     ↕ validación continua
┌─────────────────────────────────────────────┐
│          GastroPro.XUnitTests               │  ← Suite de Pruebas
│   298 pruebas · Cobertura global >90%       │
└─────────────────────────────────────────────┘
```
 
### Patrones de Diseño Clave
 
**Unit of Work:** Agrupa múltiples operaciones (vincular Pago, Pedido y Turno) en una transacción indivisible, ejecutando rollback automático ante cualquier falla parcial. Este patrón es crítico para la integridad financiera del sistema.
 
**Repository Pattern:** Abstrae el acceso a datos, permitiendo que los tests usen Mocks sin depender de una conexión real a SQL Server.
 
**MVC:** La capa de presentación actúa como una interfaz delgada que delega toda la complejidad a las capas inferiores, facilitando el testing y el mantenimiento.
 
---
 
## 7. Metodología de Desarrollo
 
### Marco de Gestión: Scrum
 
El proyecto se ejecutó mediante la metodología Scrum, con ciclos de desarrollo cortos (Sprints) enfocados en entregables funcionales:
 
- **Sprint 1:** Dominio y Entidades (Capa de Negocio)
- **Sprint 2:** Infraestructura, Repositorios y Persistencia
- **Sprint 3:** Interfaz de Usuario, Validación y Pruebas
La integración de GitHub Copilot y Gemini IA se realizó dentro de los rituales ágiles, acelerando la codificación y la validación de arquitectura sin sacrificar el criterio humano del equipo de desarrollo.
 
### Guía de Construcción (Prompts Utilizados)
 
**Fase 1 — Dominio:** Creación de entidades (Pago, Pedido, CierreCaja, Plato, Usuario) e interfaz `IUnitOfWork` con explicación de la lógica de cada propiedad.
 
**Fase 2 — Persistencia:** Configuración del `GastroProDbContext`, implementación del `UnitOfWork` y ejecución de migraciones iniciales con EF Core.
 
**Fase 3 — Controladores:** Implementación del `PagosController` con lógica de validación de turno activo, persistencia del pago, marcado de pedidos como "Pagado" y manejo de errores con `try-catch`.
 
**Fase 4 — Vistas:** Diseño de `CobrarMesa.cshtml` con Bootstrap 5, configuración de `_ValidationScriptsPartial` y paso del `PagoId` para renderizar la boleta de pago.
 
**Fase 5 — Auditoría de Código:** Revisión de modelos para propiedades de navegación nullable y uso correcto de `ModelState.Remove`.
 
**Fase 6 — Pruebas Unitarias:** Generación de tests para `HomeController`, `PedidosController`, `PlatosController`, `UsuariosController`, y las entidades de dominio `CierreCaja`, `Pago`, `Pedido` y `Plato`.
 
---
 
## 8. Requisitos del Sistema
 
### Requisitos Funcionales
 
| ID | Requisito |
|---|---|
| RF01 | Autenticación con credenciales protegidas y validación de roles. |
| RF02 | Gestión de estados de pedidos vinculados a número de mesa y cantidad de platos. |
| RF03 | Registro de pagos con monto, método (Efectivo/Yape/Plin/Tarjeta) y número de operación. |
| RF04 | Control de Cierre de Caja: cada pago vinculado a un `CierreCajaId` activo. |
| RF05 | Mantenimiento del catálogo de platos (nombre, precio, categoría). |
| RF06 | Control perimetral de roles: vistas de administración restringidas al rol Administrador. |
| RF07 | Cálculo automático de totales basado en cantidad de platos y precios del catálogo. |
| RF08 | Gestión de sesiones activas durante toda la navegación del usuario autenticado. |
 
### Requisitos No Funcionales
 
| ID | Requisito |
|---|---|
| RNF01 | Integridad referencial mediante llaves foráneas físicas entre `Pagos` y `CierresCaja`. |
| RNF02 | Programación defensiva: precisión `decimal(18,2)` y rechazo de montos negativos o nulos. |
| RNF03 | Arquitectura desacoplada en tres capas (Dominio, Infraestructura, Presentación). |
| RNF04 | Calidad de código auditada mediante xUnit con patrón AAA. |
| RNF05 | Persistencia con Entity Framework Core como ORM para SQL Server 2022. |
| RNF06 | Resiliencia ante errores de persistencia y conflictos de concurrencia. |
| RNF07 | Entidades de dominio con reglas de negocio puras, sin lógica de presentación ni acceso a datos. |
 
---
 
## 9. Estructura del Proyecto
 
```
GastroPro/
├── GastroPro.Domain/               # Núcleo de negocio (DDD)
│   ├── Entities/
│   │   ├── Pago.cs
│   │   ├── Pedido.cs
│   │   ├── CierreCaja.cs
│   │   ├── Plato.cs
│   │   └── Usuario.cs
│   └── Interfaces/
│       └── IUnitOfWork.cs
│
├── GastroPro.Infrastructure/       # Persistencia y acceso a datos
│   ├── Data/
│   │   └── GastroProDbContext.cs
│   ├── Repositories/
│   │   └── UnitOfWork.cs
│   └── Migrations/                 # Historial de cambios en BD
│
├── GastroPro.Web/                  # Capa de presentación (MVC)
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── PagosController.cs
│   │   ├── PedidosController.cs
│   │   ├── PlatosController.cs
│   │   └── UsuariosController.cs
│   ├── Views/
│   │   ├── Home/
│   │   ├── Pagos/
│   │   ├── Pedidos/
│   │   ├── Platos/
│   │   ├── Usuarios/
│   │   └── Shared/
│   ├── Models/
│   │   └── ErrorViewModel.cs
│   └── appsettings.json
│
└── GastroPro.XUnitTests/           # Suite de pruebas automatizadas
    ├── UnitTests.Domain/           # Tests de entidades (92 tests)
    │   ├── CierreCajaTests.cs
    │   ├── PagoTests.cs
    │   ├── PedidoTests.cs
    │   └── PlatoTests.cs
    └── UnitTests.Web/              # Tests de controladores (206 tests)
        ├── HomeControllerTests.cs
        ├── PagosControllerTests.cs
        ├── PedidosControllerTests.cs
        ├── PlatosControllerTests.cs
        └── UsuariosControllerTests.cs
```
 
---
 
## 10. Flujo Operativo en Vivo
 
El flujo de GastroPro replica la dinámica real de un establecimiento gastronómico de inicio a fin:
 
```
1. APERTURA DE JORNADA
   └─ Administrador/Cajero abre un turno → se genera un CierreCajaId activo
      Sin este paso, el sistema bloquea el registro de cualquier pago.
 
2. LEVANTAMIENTO DE PEDIDOS
   └─ Mozo selecciona platos del catálogo y los asigna a una mesa activa
   └─ La app persiste en SQL Server con validación decimal(18,2)
 
3. PROCESAMIENTO DE PAGO
   └─ Sistema verifica consumos pendientes y que el monto sea > 0
   └─ Cajero selecciona método: Efectivo / Yape / Plin / Tarjeta
   └─ Para pagos digitales: número de operación obligatorio
   └─ El pago queda vinculado al CierreCajaId del turno activo
 
4. CIERRE DE CAJA
   └─ Consolida el total vendido por método de pago
   └─ Valida que no existan registros huérfanos entre Pagos y CierresCaja
   └─ Genera reporte de ingresos del día para auditoría del administrador
```
 
---
 
## 11. Cobertura de Código y Pruebas Unitarias
 
La suite de pruebas de GastroPro alcanzó **298 pruebas ejecutadas con 0 errores** en 38.3 segundos.
 
### Resultados de Cobertura
 
| Módulo | Cobertura (%Blocks) | Cobertura (%Lines) |
|---|---|---|
| **gastropro.domain.dll** | **100%** | **100%** |
| gastropro.unittests.dll | 98.3% | 96.9% |
| gastropro.web.dll | 51.4% | 51.7% |
| **Global** | **91.5%** | **90.9%** |
 
### Estrategia de Pruebas
 
Las pruebas utilizan el patrón **AAA (Arrange-Act-Assert)** con **Mocks (Moq)** para aislar la lógica del negocio de la base de datos, garantizando tests rápidos, deterministas y precisos sin requerir conexión a SQL Server.
 
La generación de escenarios fue asistida por **Gemini IA**, cubriendo casos de borde que podrían pasar desapercibidos en una revisión manual.
 
### Escenarios Críticos Evaluados
 
**Seguridad y Control de Acceso**
Verificación de que el sistema concede acceso únicamente con credenciales válidas y aplica restricción perimetral estricta ante contraseñas incorrectas o nulas.
 
**Validación Defensiva de Montos**
El sistema rechaza montos cero o negativos en el registro de pagos, previniendo errores de cálculo en el cierre de caja.
 
**Integridad de Pasarelas de Pago**
Validación de que los medios de pago (Yape, Plin, Efectivo, Tarjeta) cumplan los formatos esperados antes de ser persistidos en la base de datos.
 
**Control de Roles (PlatosController)**
Si el rol en sesión es "Mozo", el acceso a `Create`, `Edit` y `Delete` se deniega y redirige a `Index`. Si es "Administrador", el CRUD completo está habilitado.
 
**Integridad de Dominio (sin Moq)**
Tests directos sobre las entidades `CierreCaja`, `Pago`, `Pedido` y `Plato` para verificar reglas de negocio puras: sumas de pasarelas, lanzamiento de excepciones ante valores inválidos, y cambios de estado de mesas.
 
---
 
## 12. Problemas Encontrados y Soluciones
 
### Error 547 — Integridad Referencial
 
**Problema:** SQL Server rechazaba eliminaciones de órdenes cuando existían dependencias activas en la tabla de Pagos, generando el error de restricción de clave foránea 547.
 
**Solución:** Se implementó "eliminación lógica" en lugar de física en el Dominio, y se configuraron las claves foráneas con `ON DELETE NO ACTION`, forzando al sistema a validar la inexistencia de dependencias desde la capa de aplicación antes de cualquier operación de limpieza.
 
### Pérdida de Precisión Decimal
 
**Problema:** Los valores monetarios perdían precisión al ser procesados con tipos de punto flotante, generando discrepancias en los arqueos de caja finales.
 
**Solución:** Redefinición de todas las entidades financieras usando `decimal(18,2)` mediante una migración de base de datos, garantizando precisión contable exacta en cada transacción.
 
### Errores CS0246 y CS0103 en Migraciones
 
**Problema:** Errores de compilación en archivos de migración por falta de directivas de referencia necesarias para `MigrationBuilder` y `ReferentialAction`.
 
**Solución:** Limpieza profunda del archivo de migración, corrección de directivas (`using Microsoft.EntityFrameworkCore.Migrations;`), y recompilación tras una Limpieza de Solución en Visual Studio 2026.
 
### Fallo en Redirección Post-Transacción
 
**Problema:** Tras confirmar el pago, el sistema ejecutaba la transacción correctamente pero fallaba al redirigir a la vista de la boleta.
 
**Solución:** Ajuste del método `HttpPost` en el controlador para usar `RedirectToAction` con el `PagoId` como parámetro explícito, asegurando un flujo continuo sin interrupciones tras la confirmación de la venta.
 
---
 
*© 2026 · GastroPro — Sistema de Gestión Gastronómica · Desarrollado por Keyla Jhazym Gutiérrez Gutiérrez*