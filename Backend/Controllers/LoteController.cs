using Database;
using DTOs.Lote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/lotes")]
[ApiController]
public class LoteController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<LoteController> _logger;

    public LoteController(DatabaseContext context, ILogger<LoteController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LoteDto>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation("Obteniendo listado completo de lotes");

        var lotes = await _context.Lote
            .AsNoTracking()
            .Select(l => new LoteDto(
                l.IDProducto,
                l.Producto.Nombre,
                l.IDProveedor,
                l.Proveedor.Nombre,
                l.Precio,
                l.Stock))
            .ToListAsync(ct);

        _logger.LogInformation("Se obtuvieron {Cantidad} lotes", lotes.Count);
        return lotes;
    }

    [HttpGet("{idProducto:int}/{idProveedor:int}")]
    [Authorize]
    public async Task<ActionResult<LoteDto>> Get(int idProducto, int idProveedor, CancellationToken ct)
    {
        _logger.LogInformation("Buscando lote para producto {ProductoId} y proveedor {ProveedorId}", idProducto, idProveedor);

        var lote = await _context.Lote
            .AsNoTracking()
            .Where(l => l.IDProducto == idProducto && l.IDProveedor == idProveedor)
            .Select(l => new LoteDto(
                l.IDProducto,
                l.Producto.Nombre,
                l.IDProveedor,
                l.Proveedor.Nombre,
                l.Precio,
                l.Stock))
            .FirstOrDefaultAsync(ct);

        if (lote is null)
        {
            _logger.LogWarning("No se encontró el lote para producto {ProductoId} y proveedor {ProveedorId}", idProducto, idProveedor);
            return NotFound();
        }

        return lote;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LoteDto>> Post([FromBody] LoteCreateDto dto, CancellationToken ct)
    {
        var producto = await _context.Producto
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ID == dto.IDProducto, ct);
        if (producto is null)
        {
            _logger.LogWarning("Intento de crear lote para producto inexistente con ID {ProductoId}", dto.IDProducto);
            return NotFound("El producto especificado no existe.");
        }

        var proveedor = await _context.Proveedor
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.ID == dto.IDProveedor, ct);
        if (proveedor is null)
        {
            _logger.LogWarning("Intento de crear lote con proveedor inexistente con ID {ProveedorId}", dto.IDProveedor);
            return NotFound("El proveedor especificado no existe.");
        }

        var existeLote = await _context.Lote.AnyAsync(
            l => l.IDProducto == dto.IDProducto && l.IDProveedor == dto.IDProveedor, ct);
        if (existeLote)
        {
            _logger.LogWarning("Intento de crear lote duplicado para producto {ProductoId} y proveedor {ProveedorId}", dto.IDProducto, dto.IDProveedor);
            return Conflict("Ya existe un lote para este producto y proveedor.");
        }

        var lote = new Lote
        {
            IDProducto = dto.IDProducto,
            IDProveedor = dto.IDProveedor,
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        _context.Lote.Add(lote);
        await _context.SaveChangesAsync(ct);

        var result = new LoteDto(
            lote.IDProducto,
            producto.Nombre,
            lote.IDProveedor,
            proveedor.Nombre,
            lote.Precio,
            lote.Stock);

        _logger.LogInformation("Lote creado exitosamente: {@Lote}", result);

        return CreatedAtAction(
            nameof(Get),
            new { idProducto = lote.IDProducto, idProveedor = lote.IDProveedor },
            result);
    }

    [HttpPut("{idProducto:int}/{idProveedor:int}")]
    [Authorize]
    public async Task<IActionResult> Put(int idProducto, int idProveedor, [FromBody] LoteUpdateDto dto, CancellationToken ct)
    {
        var lote = await _context.Lote.FindAsync(new object[] { idProducto, idProveedor }, ct);
        if (lote is null)
        {
            _logger.LogWarning("Intento de actualizar lote inexistente para producto {ProductoId} y proveedor {ProveedorId}", idProducto, idProveedor);
            return NotFound();
        }

        lote.Precio = dto.Precio;
        lote.Stock = dto.Stock;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Lote actualizado para producto {ProductoId} y proveedor {ProveedorId} (Precio: {Precio}, Stock: {Stock})", idProducto, idProveedor, lote.Precio, lote.Stock);

        return NoContent();
    }

    [HttpDelete("{idProducto:int}/{idProveedor:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int idProducto, int idProveedor, CancellationToken ct)
    {
        var lote = await _context.Lote.FindAsync(new object[] { idProducto, idProveedor }, ct);
        if (lote is null)
        {
            _logger.LogWarning("Intento de eliminar lote inexistente para producto {ProductoId} y proveedor {ProveedorId}", idProducto, idProveedor);
            return NotFound();
        }

        _context.Lote.Remove(lote);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Lote eliminado para producto {ProductoId} y proveedor {ProveedorId}", idProducto, idProveedor);

        return NoContent();
    }
}