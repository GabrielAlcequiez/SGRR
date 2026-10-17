using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Ordenes;

public class ItemOrdenDto
{
    public Guid MenuItemId { get; set; }

    // 0 indica que el producto no fue seleccionado y se ignora al crear la orden.
    [Range(0, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100.")]
    public int Cantidad { get; set; }
}
