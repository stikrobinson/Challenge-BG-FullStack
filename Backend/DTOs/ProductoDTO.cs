using System.ComponentModel.DataAnnotations;
using DTOs.Lote;

namespace DTOs.Producto
{
    public record ProductoDto(
        int ID,
        string Nombre
    );

    public record ProductoDetalleDto(
        int ID,
        string Nombre,
        List<LoteResumenDto> Lotes
    );

    public record ProductoCreateUpdateDto(
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        string Nombre
    );
}