using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GastroPro.Domain.Entities
{
    public class Pago
    {
        [Key]
        public int PagoId { get; set; }

        [Required]
        public string NumeroMesa { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagado { get; set; }

        [Required]
        public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Yape, Plin, Tarjeta

        public string NroOperacion { get; set; } = string.Empty; // Opcional para Yape/Tarjeta

        // Vincula el pago al ID del cierre de caja activo
        public int CierreCajaId { get; set; }
        public virtual CierreCaja? CierreCaja { get; set; }
    }
}