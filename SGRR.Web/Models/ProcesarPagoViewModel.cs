using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SGRR.Web.ViewModels;

public sealed class ProcesarPagoViewModel : IValidatableObject
{
    public Guid OrdenAnticipadaId { get; set; }

    public string PurchaseCode { get; set; } = string.Empty;

    public decimal MontoTotal { get; set; }

    [Required(ErrorMessage = "Selecciona un método de pago.")]
    [RegularExpression(
        "^(Credito|Debito)$",
        ErrorMessage = "Selecciona un método de pago válido.")]
    [Display(Name = "Método de pago")]
    public string MetodoPago { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
    [StringLength(23, ErrorMessage = "El número de tarjeta es demasiado largo.")]
    [NumeroTarjetaValida]
    [Display(Name = "Número de tarjeta")]
    public string NumeroTarjeta { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de expiración es obligatoria.")]
    [RegularExpression(
        "^(0[1-9]|1[0-2])/[0-9]{2}$",
        ErrorMessage = "Introduce la fecha con el formato MM/AA.")]
    [Display(Name = "Fecha de expiración (MM/AA)")]
    public string FechaExpiracion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CVV es obligatorio.")]
    [RegularExpression(
        "^[0-9]{3,4}$",
        ErrorMessage = "El CVV debe contener 3 o 4 dígitos.")]
    [DataType(DataType.Password)]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!DateTime.TryParseExact(
                FechaExpiracion,
                "MM/yy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
        {
            yield break;
        }

        var hoy = DateTime.UtcNow;
        var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);

        if (fecha < inicioMesActual)
        {
            yield return new ValidationResult(
                "La tarjeta está caducada.",
                [nameof(FechaExpiracion)]);
        }
    }
}

public sealed class NumeroTarjetaValidaAttribute : ValidationAttribute
{
    public NumeroTarjetaValidaAttribute()
    {
        ErrorMessage = "Introduce un número de tarjeta válido.";
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is not string numeroTarjeta || string.IsNullOrWhiteSpace(numeroTarjeta))
        {
            return ValidationResult.Success;
        }

        var digitos = new string(numeroTarjeta
            .Where(caracter => !char.IsWhiteSpace(caracter) && caracter != '-')
            .ToArray());

        if (digitos.Length is < 13 or > 19 ||
            digitos.Any(caracter => caracter is < '0' or > '9'))
        {
            return new ValidationResult(ErrorMessage);
        }

        var suma = 0;
        var duplicar = false;

        for (var indice = digitos.Length - 1; indice >= 0; indice--)
        {
            var digito = digitos[indice] - '0';

            if (duplicar)
            {
                digito *= 2;
                if (digito > 9)
                {
                    digito -= 9;
                }
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return suma % 10 == 0
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}