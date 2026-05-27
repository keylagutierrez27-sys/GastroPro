using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GastroPro.Domain.Entities
{
    public class Plato
    {
        [Key]
        public int PlatoId { get; set; }

        [Required(ErrorMessage = "El nombre del plato es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.1, 1000, ErrorMessage = "El precio debe ser un valor válido")]
        public decimal Precio { get; set; }

        public string Categoria { get; set; } = "General"; // Ej: Entradas, Segundos, Bebidas
    }
}
