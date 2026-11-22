using System.ComponentModel.DataAnnotations;

namespace DTOs.Lote
{
    public record LoteDto(
        int IDProducto,
        string NombreProducto,
        int IDProveedor,
        string NombreProveedor,
        int Precio,
        int Stock
    );

    public record LoteResumenDto(
        int IDProveedor,
        string NombreProveedor,
        int Precio,
        int Stock
    );

    public record LoteCreateDto(
        [Required]
        int IDProducto,

        [Required]
        int IDProveedor,

        [Range(0, int.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        int Precio,

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        int Stock
    );

    public record LoteUpdateDto(
        [Range(0, int.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        int Precio,

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        int Stock
    );
}