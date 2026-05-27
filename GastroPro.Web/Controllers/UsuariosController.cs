using Microsoft.AspNetCore.Mvc;
using GastroPro.Domain.Interfaces;
using GastroPro.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace GastroPro.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsuariosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Listado y lógica de inicialización segura
        public async Task<IActionResult> Index()
        {
            var usuarios = await _unitOfWork.GetUsuariosAsync();
            var listaUsuarios = usuarios.ToList();

            // 🛑 BLINDAJE: Si la base de datos está vacía, creamos los perfiles iniciales
            if (!listaUsuarios.Any())
            {
                await _unitOfWork.AddUsuarioAsync(new Usuario { Nombre = "Keyla Gutierrez", Rol = "Administrador", Contrasena = "admin123" });
                await _unitOfWork.AddUsuarioAsync(new Usuario { Nombre = "Juan (Mozo 1)", Rol = "Mozo" });
                await _unitOfWork.AddUsuarioAsync(new Usuario { Nombre = "Roxana Palomino", Rol = "Cajero", Contrasena = "caja123" });
                await _unitOfWork.CompleteAsync();

                // Recargamos la lista limpia
                usuarios = await _unitOfWork.GetUsuariosAsync();
                listaUsuarios = usuarios.ToList();
            }

            return View(listaUsuarios);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if ((usuario.Rol == "Administrador" || usuario.Rol == "Cajero") && string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                ModelState.AddModelError("Contrasena", $"La contraseña es obligatoria para el rol {usuario.Rol}.");
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.AddUsuarioAsync(usuario);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // 🟢 GET: /Usuarios/Edit/5
        // Carga la pantalla de edición con los datos actuales del usuario seleccionado
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _unitOfWork.GetUsuarioByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // 🟢 POST: /Usuarios/Edit/5
        // Recibe los datos modificados desde el formulario y los impacta en SQL Server
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuarioModificado)
        {
            if (id != usuarioModificado.UsuarioId)
            {
                return NotFound();
            }

            // Validamos que los roles con privilegios sigan teniendo una contraseña establecida
            if ((usuarioModificado.Rol == "Administrador" || usuarioModificado.Rol == "Cajero") && string.IsNullOrWhiteSpace(usuarioModificado.Contrasena))
            {
                ModelState.AddModelError("Contrasena", $"La contraseña no puede quedar vacía para el rol {usuarioModificado.Rol}.");
            }

            if (ModelState.IsValid)
            {
                // Invocamos la actualización a través de nuestro Unit of Work
                _unitOfWork.UpdateUsuario(usuarioModificado);
                await _unitOfWork.CompleteAsync(); // Hacemos el commit definitivo en SQL Server

                return RedirectToAction(nameof(Index));
            }

            return View(usuarioModificado);
        }

        // POST: /Usuarios/CambiarUsuarioActivo
        [HttpPost]
        public async Task<IActionResult> CambiarUsuarioActivo(int id, string? passwordInput)
        {
            var usuario = await _unitOfWork.GetUsuarioByIdAsync(id);
            if (usuario == null) return NotFound();

            // VALIDACIÓN: Verificación rigurosa de contraseñas para roles con privilegios financieros
            if (usuario.Rol == "Administrador" || usuario.Rol == "Cajero")
            {
                var claveIngresada = passwordInput?.Trim();

                // ⚡ SOLUCIÓN AL NULL ADAPTATIVA: Asigna clave por defecto según el rol si está vacío en BD
                var clavePorDefecto = usuario.Rol == "Administrador" ? "admin123" : "caja123";
                var claveBaseDatos = string.IsNullOrEmpty(usuario.Contrasena) ? clavePorDefecto : usuario.Contrasena.Trim();

                if (claveBaseDatos != claveIngresada)
                {
                    TempData["ErrorPassword"] = $"Contraseña incorrecta para acceder como {usuario.Nombre}.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(usuario.Contrasena))
                {
                    usuario.Contrasena = clavePorDefecto;
                    await _unitOfWork.CompleteAsync();
                }
            }

            // Guardamos las credenciales de identidad en la sesión web activa
            HttpContext.Session.SetString("UsuarioActivo", usuario.Nombre);
            HttpContext.Session.SetString("RolActivo", usuario.Rol);

            // 🔀 REDIRECCIONAMIENTO INTELIGENTE CORREGIDO: Evita el error 404 de "Mesas"
            if (usuario.Rol == "Cajero")
            {
                return RedirectToAction("Index", "Pagos");
            }
            else if (usuario.Rol == "Mozo")
            {
                // ✅ CAMBIO CLAVE: Redirigimos al Home/Index para que cargue el menú operativo del Mozo sin bloqueos
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Usuarios/CerrarSesion
        public IActionResult CerrarSesion()
        {
            // 🧼 Limpiamos por completo las variables de identidad en el servidor
            HttpContext.Session.Remove("UsuarioActivo");
            HttpContext.Session.Remove("RolActivo");

            // Destruimos la sesión por completo por seguridad perimetral
            HttpContext.Session.Clear();

            // Redirigimos directo al Index de Usuarios para obligar a elegir un nuevo rol
            return RedirectToAction("Index", "Usuarios");
        }
    }
}