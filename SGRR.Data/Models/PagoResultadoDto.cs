namespace SGRR.Data.Models;

public sealed record PagoResultadoDto(
    bool Aprobado,
    string Mensaje,
    Guid? PagoId = null);