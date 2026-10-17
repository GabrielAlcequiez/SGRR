namespace SGRR.Web.Models
{
    public class OrdenAnticipada
    {
        public int Id { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}