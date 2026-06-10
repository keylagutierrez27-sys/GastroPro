# 📋 Planificación y Diseño Web — GastroPro
### Sistema Integrado de Control Digital de Comandas y Gestión de Pagos

> **Estudiante:** Keyla J. Gutiérrez Gutiérrez · 
> **Asignatura:** IS 489 – Pruebas y Aseguramiento de la Calidad  
> **Docente:** Mg. Richard Zapata Casaverde  
> **Universidad:** UNSCH — Ayacucho, Perú · 2026

---

## Tabla de Contenidos

1. [Planificación Inicial](#1-planificación-inicial)
2. [Prototipos de Interfaz](#2-prototipos-de-interfaz-wireframes)
3. [Flujo de Navegación](#3-flujo-de-navegación)
4. [Cobertura de Pruebas](#4-resumen-de-cobertura-de-pruebas)
5. [Arquitectura Limpia](#5-arquitectura-limpia-clean-architecture)
6. [Bugs Resueltos y Lecciones](#6-bugs-resueltos-y-lecciones-aprendidas)

---

## 1. Planificación Inicial

### 1.1 Identificación del Problema

Los establecimientos gastronómicos de Ayacucho (calderías, restaurantes de menú y fondas) operan con cuadernos físicos para registrar comandas, cajones de efectivo sin respaldo digital y cierres de caja manuales propensos a errores.

**Problemas detectados:**
- Pérdida de comandas escritas a mano
- Descuadres diarios por redondeo manual
- Imposibilidad de auditar un turno anterior
- No se distingue qué pagó cada mesa ni con qué método
- El mozo depende del cajero para saber el precio de un plato

**Propuesta de solución:** GastroPro — plataforma web MVC en .NET 10 que digitaliza el ciclo completo: apertura de turno → comanda → pago → cierre de caja auditado.

---

### 1.2 Cronograma de Desarrollo (Scrum)

| Sprint | Nombre  | Entregables clave |
|--------|--------|--------|-------------------|
| **Sprint 0** | Planificación y Arquitectura | Requisitos, modelo ER, wireframes, configuración del repo |
| **Sprint 1** | Dominio e Infraestructura | Entidades DDD, IUnitOfWork, EF Core Code-First, migraciones, seed |
| **Sprint 2** | Capa Web (Controllers + Views) | Todos los controladores, vistas Razor, Bootstrap 5 Mobile-First |
| **Sprint 3** | Pruebas y Calidad | 298 pruebas xUnit, Code Coverage >90%, resolución de bugs |

---

### 1.3 Stack Tecnológico Justificado

| Capa | Tecnología | Justificación |
|------|-----------|---------------|
| Plataforma | .NET 10 | LTS, alto rendimiento en web |
| Lenguaje | C# | Tipado fuerte, ideal para finanzas |
| IDE | Visual Studio 2026 | Depuración + migraciones integradas |
| Framework Web | ASP.NET Core MVC | Separación clara Modelo-Vista-Controlador |
| ORM | EF Core Code-First | Migraciones automáticas de esquema |
| Base de Datos | SQL Server 2022 | FK, transacciones ACID, SSMS visual |
| UI | Bootstrap 5 | Mobile-first, componentes listos |
| Vistas | Razor (.cshtml) | Integración nativa con C# |
| Pruebas | xUnit + Moq | Estándar industrial .NET |
| IA Asistente | Gemini + GitHub Copilot | Generación de casos de borde |

---

### 1.4 Modelo de Datos (Entidades)

```
┌──────────────────────────────────────────────────────────┐
│  DIAGRAMA ER SIMPLIFICADO                                │
└──────────────────────────────────────────────────────────┘

  [Usuario]                    [Plato]
   PK: UsuarioId                PK: PlatoId
   Nombre                       Nombre
   Contraseña                   Precio decimal(18,2)
   Rol (Admin/Cajero/Mozo)      Categoría
                                Descripción
        │
        │ 1:N
        ▼
  [CierreCaja]  ◄──────────────────────── [Pago]
   PK: CierreCajaId              FK: CierreCajaId (NOT NULL)
   FechaApertura                 PK: PagoId
   FechaCierre                   NumeroMesa
   Estado (Abierto/Cerrado)      FechaPago datetime2
   TotalVendido decimal(18,2)    TotalPagado decimal(18,2)
                                 MetodoPago (Efectivo/Yape/Plin/Tarjeta)
                                 NroOperacion
        │
        │ 1:N
        ▼
  [Pedido]
   PK: PedidoId
   NumeroMesa
   FK: PlatoId
   Cantidad
   Subtotal decimal(18,2)
   Estado (Pendiente / En Cocina / Pagado / Cancelado)
```

> **Regla clave:** Un `Pago` sin `CierreCajaId` es **rechazado por FK**. Garantiza trazabilidad financiera del 100%.

---

### 1.5 Requisitos Funcionales

| Código | Descripción |
|--------|-------------|
| RF01 | Autenticación con credenciales protegidas por rol |
| RF02 | Gestión de estados de pedidos vinculados a mesa |
| RF03 | Registro y validación de pagos (monto > 0 obligatorio) |
| RF04 | Control de Cierre de Caja por turno (`CierreCajaId`) |
| RF05 | Mantenimiento del Catálogo de Platos (CRUD solo Admin) |
| RF06 | Control perimetral de roles (Admin / Cajero / Mozo) |
| RF07 | Cálculo automático de totales por mesa |
| RF08 | Gestión de sesiones de usuario durante la navegación |

---

### 1.6 Requisitos No Funcionales

| Código | Descripción |
|--------|-------------|
| RNF01 | Integridad referencial FK entre `Pagos` ↔ `CierresCaja` |
| RNF02 | Precisión `decimal(18,2)` + rechazo de valores negativos/nulos |
| RNF03 | Arquitectura Clean Architecture en 3 capas desacopladas |
| RNF04 | Cobertura de código ≥ 90% con xUnit (patrón AAA) |
| RNF05 | ORM EF Core para mapeo objeto-relacional con SQL Server 2022 |
| RNF06 | Resiliencia ante conflictos de concurrencia en BD |
| RNF07 | Entidades de dominio puras (sin lógica de infraestructura) |

---

## 2. Prototipos de Interfaz (Wireframes)

> Los wireframes muestran la estructura visual y los elementos de cada pantalla.  
> `[> Texto ]` = botón de acción · `[___]` = campo de entrada · `[▼]` = dropdown

---

### P-01 — Pantalla de Inicio / Selección de Rol

**Ruta:** `/Home/Index` · **Acceso:** Público

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  🔒  🔒  🔒  🔒  🔒              👤 Invitado │
│                                              [Ninguno ▼]│
├─────────────────────────────────────────────────────────┤
│                                                         │
│              ┌─────────────────────────┐               │
│              │                         │               │
│              │         👤🔒             │               │
│              │                         │               │
│              │   Acceso Restringido    │               │
│              │       GastroPro         │               │
│              │                         │               │
│              │  [> Seleccionar Mi      │               │
│              │      Rol Operativo  ]   │               │
│              │                         │               │
│              └─────────────────────────┘               │
│                                                         │
│         © 2026 · GastroPro · Sistema de Gestión        │
└─────────────────────────────────────────────────────────┘
```

**Flujo:** Clic en el botón → dropdown con roles: `Administrador / Cajero / Mozo` → redirige a `/Usuarios/Login?rol=...`

---

### P-02 — Formulario de Login

**Ruta:** `/Usuarios/Login` · **Acceso:** Público

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro                                👤 Invitado  │
├─────────────────────────────────────────────────────────┤
│                                                         │
│         ┌───────────────────────────────────┐          │
│         │  🏠 Iniciar Sesión — GastroPro    │          │
│         │  ─────────────────────────────    │          │
│         │                                   │          │
│         │   Rol seleccionado: [Administrador]│          │
│         │                                   │          │
│         │   Usuario                         │          │
│         │   [_________________________________]         │
│         │                                   │          │
│         │   Contraseña                      │          │
│         │   [_________________________________]         │
│         │                                   │          │
│         │      [>  Ingresar al Sistema  ]   │          │
│         │                                   │          │
│         │   ⚠ Credenciales inválidas        │          │
│         │     (visible solo si hay error)   │          │
│         │                                   │          │
│         └───────────────────────────────────┘          │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Validaciones frontend:** usuario y contraseña obligatorios, mínimo 4 caracteres.  
**Validaciones backend:** busca usuario en BD → compara contraseña → si válido guarda `Session["NombreUsuario"]` y `Session["Rol"]`.

---

### P-03 — Panel Principal (Dashboard post-login)

**Ruta:** `/Home/Dashboard` · **Acceso:** Administrador / Cajero

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Gestionar Platos  📋Ver Pedidos  🗂Ver Caja│
│                                       👤 Keyla [Admin ▼]│
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Bienvenida, Keyla Gutierrez        Rol: Administrador  │
│  ═══════════════════════════════════════════════════    │
│                                                         │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────┐ │
│  │    📋         │  │    🍽         │  │    🗂       │ │
│  │  Ver Pedidos  │  │  Gestionar    │  │  Ver Caja   │ │
│  │  por Mesa     │  │  Platos       │  │  / Turnos   │ │
│  │               │  │               │  │             │ │
│  │  [>  Ir  ]    │  │  [>  Ir  ]    │  │  [>  Ir  ]  │ │
│  └───────────────┘  └───────────────┘  └─────────────┘ │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  Resumen del Turno Actual                       │   │
│  │  ─────────────────────────────────────────────  │   │
│  │  Turno: #0001    Estado: 🟢 ABIERTO             │   │
│  │  Inicio: 27/05/2026  11:51 a.m.                 │   │
│  │  Total recaudado: S/ 50.00                      │   │
│  │  Transacciones:   1 venta                       │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

### P-04 — Gestión de Platos (CRUD)

**Ruta:** `/Platos/Index` · **Acceso:** Administrador (Mozo ve la lista sin botones de edición)

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Platos  📋Pedidos  🗂Caja    👤 Admin ▼   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ══ Catálogo de Platos              [> + Nuevo Plato ]  │
│                                                         │
│  ┌─────┬──────────────────────────┬────────┬─────────┐  │
│  │  ID │ Nombre                   │ Precio │Acciones │  │
│  ├─────┼──────────────────────────┼────────┼─────────┤  │
│  │   1 │ Caldo de Gallina         │ S/14.00│  ✏  🗑 │  │
│  │   2 │ Menú Ejecutivo Combinado │ S/16.00│  ✏  🗑 │  │
│  │   3 │ Mondongo Ayacuchano      │ S/12.00│  ✏  🗑 │  │
│  │   4 │ Chicharrón con Mote      │ S/18.00│  ✏  🗑 │  │
│  │   5 │ Puchero de Res           │ S/15.00│  ✏  🗑 │  │
│  └─────┴──────────────────────────┴────────┴─────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Formulario Crear/Editar** (`/Platos/Create` o `/Platos/Edit/{id}`):

```
  ┌──────────────────────────────────────┐
  │  Nuevo Plato                         │
  │  ──────────────────────────────────  │
  │  Nombre del plato                    │
  │  [___________________________________]│
  │                                      │
  │  Precio (S/)                         │
  │  [___________________________________]│
  │                                      │
  │  Categoría                           │
  │  [__ Caldos ▼ ______________________]│
  │                                      │
  │  Descripción (opcional)              │
  │  [___________________________________]│
  │                                      │
  │    [> Guardar ]     [  Cancelar  ]   │
  └──────────────────────────────────────┘
```

---

### P-05 — Panel de Comandas por Mesa

**Ruta:** `/Pedidos/Index` · **Acceso:** Administrador / Mozo / Cajero

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Platos  📋Pedidos  🗂Caja    👤 Admin ▼   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ══ Panel de Comandas por Mesa   [> ➕ Tomar Pedido ]   │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  🪑 MESA #1                  2 plato(s) solicitado│  │
│  │  ──────────────────────────────────────────────  │  │
│  │  PLATO           PRECIO  CANT  SUBTOTAL  ESTADO   │  │
│  │  Caldo Gallina   S/14.00  x1   S/14.00  🍳En Cocina│ │
│  │  Menú Ejecutivo  S/16.00  x2   S/32.00  ⏳Pendiente│ │
│  │                                                  │  │
│  │  Total: S/ 46.00              [> Cobrar Cuenta ] │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  🪑 MESA #3                  1 plato(s) solicitado│  │
│  │  ──────────────────────────────────────────────  │  │
│  │  Puchero de Res  S/15.00  x1   S/15.00  ⏳Pendiente│ │
│  │                                                  │  │
│  │  Total: S/ 15.00              [> Cobrar Cuenta ] │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Formulario Tomar Nuevo Pedido** (`/Pedidos/Create`):

```
  ┌──────────────────────────────────────┐
  │  ➕ Nuevo Pedido                      │
  │  ──────────────────────────────────  │
  │  Número de Mesa                      │
  │  [__ 1 ▼ ___________________________]│
  │                                      │
  │  Plato                               │
  │  [__ Caldo de Gallina ▼ ____________]│
  │                                      │
  │  Cantidad                            │
  │  [___________________________________]│
  │                                      │
  │    [> Agregar a Comanda ]            │
  └──────────────────────────────────────┘
```

---

### P-06 — Módulo de Cobro (Procesamiento de Pago)

**Ruta:** `/Pagos/CobrarMesa/{mesa}` · **Acceso:** Cajero / Administrador

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Platos  📋Pedidos  🗂Caja    👤 Admin ▼   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ══ Cobrar Mesa #1                                      │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │  RESUMEN DE CONSUMO                               │  │
│  │  ─────────────────────────────────────────────   │  │
│  │  Caldo de Gallina Concentrado    x1    S/ 14.00   │  │
│  │  Menú Ejecutivo Combinado        x2    S/ 32.00   │  │
│  │  ─────────────────────────────────────────────   │  │
│  │  TOTAL A COBRAR:                       S/ 46.00   │  │
│  └───────────────────────────────────────────────────┘  │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │  MÉTODO DE PAGO                                   │  │
│  │  ─────────────────────────────────────────────   │  │
│  │  ( ) 💵 Efectivo                                  │  │
│  │  ( ) 📱 Yape                                      │  │
│  │  ( ) 📱 Plin                                      │  │
│  │  (●) 💳 Tarjeta                                   │  │
│  │                                                   │  │
│  │  Nro. de Operación  (obligatorio si es digital)   │  │
│  │  [_____________________________________________]   │  │
│  │                                                   │  │
│  │  Monto Recibido (S/)                              │  │
│  │  [_____________________________________________]   │  │
│  │                                                   │  │
│  │       [>  Confirmar e Imprimir Boleta  ]          │  │
│  └───────────────────────────────────────────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Validaciones:**
- Monto debe ser `> 0` (RNF02)
- Si `MetodoPago ≠ Efectivo` → `NroOperacion` es obligatorio
- Debe existir un `CierreCajaId` activo (RF04)
- Al confirmar → `RedirectToAction("VerBoleta", new { id = pago.PagoId })`

---

### P-07 — Vista de Boleta

**Ruta:** `/Pagos/VerBoleta/{pagoId}` · **Acceso:** Cajero / Administrador

```
  ╔═══════════════════════════════════════╗
  ║         G A S T R O P R O             ║
  ║    Sistema de Gestión Gastronómica    ║
  ╠═══════════════════════════════════════╣
  ║  Fecha:    27/05/2026  12:07 p.m.     ║
  ║  Mesa:     #1                         ║
  ║  Cajero:   Keyla Gutierrez            ║
  ║  Turno:    #0001                      ║
  ╠═══════════════════════════════════════╣
  ║  DETALLE DE CONSUMO                   ║
  ║  ─────────────────────────────────   ║
  ║  Caldo de Gallina Conc.   x1  S/14.00 ║
  ║  Menú Ejecutivo Comb.     x2  S/32.00 ║
  ╠═══════════════════════════════════════╣
  ║  TOTAL:               S/ 46.00        ║
  ║  Método de Pago:      Tarjeta         ║
  ║  Nro. Operación:  TXN-20260527-001    ║
  ╠═══════════════════════════════════════╣
  ║       ¡Gracias por su visita!         ║
  ║  © 2026 GastroPro — Ayacucho, Perú    ║
  ╚═══════════════════════════════════════╝

      [> 🖨 Imprimir ]    [  Volver al Salón  ]
```

---

### P-08 — Panel de Caja y Cierre de Turno

**Ruta:** `/CierreCaja/Index` · **Acceso:** Administrador / Cajero

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Platos  📋Pedidos  🗂Caja    👤 Admin ▼   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────────┐  ┌───────────────────────────┐   │
│  │ 🟢 DÍA COMERCIAL │  │  🖨 Módulo de Facturación │   │
│  │    ACTIVO        │  │  ─────────────────────    │   │
│  │                  │  │  Cobra comandas pendientes │   │
│  │  S/ 0.00         │  │  del salón.               │   │
│  │                  │  │                           │   │
│  │ Cajero/a:        │  │  [> ⬇ Cobrar una Mesa ]  │   │
│  │  Keyla Gutierrez │  └───────────────────────────┘   │
│  │                  │                                   │
│  │ [> 🔒 Cerrar Día]│  ┌───────────────────────────┐   │
│  │ [> 🖨 Imprimir ] │  │  📊 Balance del Arqueo    │   │
│  └──────────────────┘  │  ─────────────────────    │   │
│                        │  💵 Efectivo:   S/ 50.00   │   │
│                        │  📱 Yape:       S/  0.00   │   │
│                        │  📱 Plin:       S/  0.00   │   │
│                        │  💳 Tarjetas:   S/  0.00   │   │
│                        │  ─────────────────────     │   │
│                        │  Transacciones: 1 venta    │   │
│                        └───────────────────────────┘   │
│                                                         │
│  ══ Detalle de Ventas del Día Abierto                   │
│                                                         │
│  ┌──────┬──────────┬────────────┬───────────┬────────┐  │
│  │ HORA │ MESA     │ MÉTODO     │ NRO. OPER.│ MONTO  │  │
│  ├──────┼──────────┼────────────┼───────────┼────────┤  │
│  │12:07 │ Mesa #1  │ Efectivo   │ TEST-001  │ S/50.00│  │
│  └──────┴──────────┴────────────┴───────────┴────────┘  │
│                                                         │
│  ══ Historial de Días Comerciales Cerrados              │
│                                                         │
│  ┌────────┬──────────────────┬──────────────┬────────┐  │
│  │ID Turno│ Fecha Apertura   │ Fecha Cierre │ Total  │  │
│  ├────────┼──────────────────┼──────────────┼────────┤  │
│  │  #0001 │ 27/05/26 11:51   │ Sigue en cur.│ S/0.00 │  │
│  └────────┴──────────────────┴──────────────┴────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

### P-09 — Gestión de Usuarios

**Ruta:** `/Usuarios/Index` · **Acceso:** SOLO Administrador

```
┌─────────────────────────────────────────────────────────┐
│  GastroPro  ¶Platos  📋Pedidos  🗂Caja    👤 Admin ▼   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ══ Gestión de Usuarios         [> + Nuevo Usuario ]    │
│                                                         │
│  ┌────┬───────────────────┬────────────┬─────────────┐  │
│  │ ID │ Nombre            │ Rol        │ Acciones    │  │
│  ├────┼───────────────────┼────────────┼─────────────┤  │
│  │  1 │ Keyla Gutierrez   │ Admin      │   ✏   🗑   │  │
│  │  2 │ Juan Palomino     │ Cajero     │   ✏   🗑   │  │
│  │  3 │ Rosa Quispe       │ Mozo       │   ✏   🗑   │  │
│  └────┴───────────────────┴────────────┴─────────────┘  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 3. Flujo de Navegación

```
P-01 (Inicio / Selección de Rol)
        │
        └──► [Seleccionar Rol]
                    │
                    ▼
             P-02 (Login)
                    │
         ┌──────────┼──────────┐
         ▼          ▼          ▼
      Admin       Cajero      Mozo
         │          │          │
         └──────────┴──────────┘
                    │
                    ▼
             P-03 (Dashboard)
            /        |        \
           ▼         ▼         ▼
        P-04       P-05       P-08
      (Platos)  (Comandas)   (Caja)
                    │           │
                    ▼           ▼
                 P-06        Cierre
                (Cobro)    de Turno
                    │
                    ▼
                 P-07
               (Boleta)
```

| Pantalla | Roles con Acceso |
|----------|-----------------|
| P-01 Login | Todos (público) |
| P-03 Dashboard | Admin, Cajero |
| P-04 Platos CRUD | **Solo Admin** |
| P-05 Comandas | Admin, Cajero, Mozo |
| P-06 Cobro | Admin, Cajero |
| P-07 Boleta | Admin, Cajero |
| P-08 Caja | Admin, Cajero |
| P-09 Usuarios | **Solo Admin** |

---

## 4. Resumen de Cobertura de Pruebas

| Módulo | Pruebas | % Bloques | % Líneas |
|--------|---------|-----------|----------|
| `gastropro.domain.dll` | 92 | **100.0%** | **100.0%** |
| `gastropro.unittests.dll` | 298 | 98.3% | 96.9% |
| `gastropro.web.dll` | 206 | 51.4% | 51.7% |
| **GLOBAL** | **298** | **91.5%** | **90.9%** |

- ⏱ Tiempo total de ejecución: **38.3 segundos**
- ✅ Errores: **0** · Omitidas: **0** · Advertencias: **0**

---

## 5. Arquitectura Limpia (Clean Architecture)

```
┌─────────────────────────────────────────────────────┐
│                  GastroPro.Web                      │
│  Controllers: Home · Pagos · Pedidos · Platos       │
│               Usuarios · CierreCaja                 │
│  Views: Razor .cshtml  (Bootstrap 5 Mobile-First)   │
│  Models: ViewModels + ErrorViewModel                │
└───────────────────┬─────────────────────────────────┘
                    │  depende de ↓
┌───────────────────▼─────────────────────────────────┐
│            GastroPro.Domain  ◄── NÚCLEO PURO        │
│  Entities: Pago · Pedido · CierreCaja · Plato       │
│            Usuario                                  │
│  Interfaces: IUnitOfWork                            │
│  Cobertura: 100%  (lógica financiera protegida)     │
└───────────────────┬─────────────────────────────────┘
                    │  implementado por ↓
┌───────────────────▼─────────────────────────────────┐
│           GastroPro.Infrastructure                  │
│  Data: GastroProDbContext (EF Core)                 │
│  Repositories: UnitOfWork.cs                        │
│  Migrations: Historial Code-First                   │
│  → SQL Server 2022  (decimal 18,2 · FK · ACID)      │
└─────────────────────────────────────────────────────┘
                    ↕  validado por
┌─────────────────────────────────────────────────────┐
│            GastroPro.XUnitTests                     │
│  Domain Tests (92): CierreCaja · Pago · Pedido      │
│                     Plato  (sin Moq, dominio puro)  │
│  Web Tests  (206): Controllers con Moq              │
│  Patrón: AAA (Arrange · Act · Assert)               │
└─────────────────────────────────────────────────────┘
```

---

## 6. Bugs Resueltos y Lecciones Aprendidas

### 🐛 Bug #1 — Error 547 (Integridad Referencial)

| | |
|--|--|
| **Causa** | Eliminar un `Pedido` con `Pagos` dependientes activos |
| **Efecto** | SQL Server lanzaba `FOREIGN KEY VIOLATION` |
| **Fix** | Eliminación lógica (campo `Estado = "Cancelado"`) + FK configurada como `ON DELETE NO ACTION` |
| **Lección** | Nunca eliminar físicamente en cascada sin validar dependencias desde la capa de aplicación |

---

### 🐛 Bug #2 — Pérdida de Precisión Decimal

| | |
|--|--|
| **Causa** | Entidades financieras con tipo `float` / `double` |
| **Efecto** | `S/46.00` se guardaba como `S/45.9999...` → descuadre en arqueo |
| **Fix** | Migración para cambiar todos los campos monetarios a `decimal(18,2)` |
| **Lección** | En sistemas financieros **nunca usar `float`**. Siempre `decimal` con escala explícita |

---

### 🐛 Bug #3 — CS0246 / CS0103 en Migraciones

| | |
|--|--|
| **Causa** | Directivas `using` faltantes en archivo de migración auto-generado |
| **Efecto** | Build failure → no se podía aplicar `Update-Database` |
| **Fix** | Agregar `using Microsoft.EntityFrameworkCore.Migrations;` + Limpieza de Solución + Rebuild |
| **Lección** | Revisar siempre los archivos de migración auto-generados antes de aplicar cambios a la BD |

---

### 🐛 Bug #4 — Fallo en Redirección Post-Pago

| | |
|--|--|
| **Causa** | `return View()` en lugar de `RedirectToAction` en el método `HttpPost` |
| **Efecto** | Pago guardado correctamente pero pantalla en blanco al finalizar |
| **Fix** | `return RedirectToAction("VerBoleta", new { id = pago.PagoId });` |
| **Lección** | Siempre usar `RedirectToAction` después de un POST exitoso (patrón **PRG: Post-Redirect-Get**) |

---

*© 2026 · GastroPro — Planificación y Diseño Web*  
*Elaborado por: Keyla J. Gutiérrez Gutiérrez · IS 489 · UNSCH · Ayacucho, Perú*
