using System.ComponentModel.DataAnnotations;

namespace DTOs.Proveedor
{
    public record ProveedorDto(
        int ID,
        string Nombre,
        string Direccion,
        string RUC
    );

    public record ProveedorCreateUpdateDto(
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [StringLength(150)]
        string Nombre,

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(250)]
        string Direccion,

        [Required(ErrorMessage = "El RUC es obligatorio.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "El RUC debe tener entre 10 y 13 caracteres.")]
        string RUC
    );
}