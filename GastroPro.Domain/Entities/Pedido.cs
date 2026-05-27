using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GastroPro.Domain.Entities
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio")]
        public string NumeroMesa { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Range(1, 100, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        public string Estado { get; set; } = "Pendiente"; // Pendiente, En Cocina, Entregado

        // --- RELACIÓN CON LA ENTIDAD PLATO ---
        [Required]
        public int PlatoId { get; set; }

        public Plato? Plato { get; set; } // Propiedad de navegación
    }
}
