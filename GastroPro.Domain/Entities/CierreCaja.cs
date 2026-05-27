using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GastroPro.Domain.Entities
{
    public class CierreCaja
    {
        [Key]
        public int CierreCajaId { get; set; }

        public DateTime FechaApertura { get; set; } = DateTime.Now;

        public DateTime? FechaCierre { get; set; } // Quedará NULL mientras el día esté abierto

        [Required]
        public string Estado { get; set; } = "Abierto"; // "Abierto" o "Cerrado"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalVendido { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
