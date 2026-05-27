using System.ComponentModel.DataAnnotations;

namespace GastroPro.Domain.Entities
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "Mozo"; // Administrador, Mozo, Cajero

        // CAMPO NUEVO: Contraseña opcional (solo obligatoria para el Administrador)
        [DataType(DataType.Password)]
        public string? Contrasena { get; set; }
        public bool EstaActivo { get; set; } = true;
    }
}