using Database;
using DTOs.Lote;
using DTOs.Producto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/productos")]
[ApiController]

public class ProductoController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ProductoController> _logger;

    public ProductoController(DatabaseContext context, ILogger<ProductoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation("Obteniendo listado completo de productos");

        var productos = await _context.Producto
            .AsNoTracking()
            .Select(p => new ProductoDto(p.ID, p.Nombre))
            .ToListAsync(ct);

        _logger.LogInformation("Se obtuvieron {Cantidad} productos", productos.Count);
        return productos;
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ProductoDto>> Get(int id, CancellationToken ct)
    {
        _logger.LogInformation("Buscando producto con ID {ProductoId}", id);

        var producto = await _context.Producto
            .AsNoTracking()
            .Where(p => p.ID == id)
            .Select(p => new ProductoDto(p.ID, p.Nombre))
            .FirstOrDefaultAsync(ct);

        if (producto is null)
        {
            _logger.LogWarning("No se encontró el producto con ID {ProductoId}", id);
            return NotFound();
        }

        return producto;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductoDto>> Post([FromBody] ProductoCreateUpdateDto dto, CancellationToken ct)
    {
        var producto = new Producto { Nombre = dto.Nombre };

        _context.Producto.Add(producto);
        await _context.SaveChangesAsync(ct);

        var result = new ProductoDto(producto.ID, producto.Nombre);
        _logger.LogInformation("Producto creado exitosamente: {@Producto}", result);

        return CreatedAtAction(nameof(Get), new { id = producto.ID }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Put(int id, [FromBody] ProductoCreateUpdateDto dto, CancellationToken ct)
    {
        var producto = await _context.Producto.FindAsync(new object[] { id }, ct);
        if (producto is null)
        {
            _logger.LogWarning("Intento de actualizar producto inexistente con ID {ProductoId}", id);
            return NotFound();
        }

        producto.Nombre = dto.Nombre;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Producto con ID {ProductoId} actualizado a '{Nombre}'", id, producto.Nombre);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var producto = await _context.Producto.FindAsync(new object[] { id }, ct);
        if (producto is null)
        {
            _logger.LogWarning("Intento de eliminar producto inexistente con ID {ProductoId}", id);
            return NotFound();
        }

        _context.Producto.Remove(producto);

        try
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Producto con ID {ProductoId} eliminado", id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de integridad al intentar eliminar el producto {ProductoId}", id);
            return Conflict("El producto está referenciado por otros registros.");
        }

        return NoContent();
    }

    [HttpGet("{id:int}/lotes")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LoteResumenDto>>> GetLotes(int id, CancellationToken ct)
    {
        _logger.LogInformation("Obteniendo lotes para el producto con ID {ProductoId}", id);

        var existeProducto = await _context.Producto.AnyAsync(p => p.ID == id, ct);
        if (!existeProducto)
        {
            _logger.LogWarning("No se encontró el producto con ID {ProductoId} al consultar sus lotes", id);
            return NotFound();
        }

        var lotes = await _context.Lote
            .AsNoTracking()
            .Where(l => l.IDProducto == id)
            .Select(l => new LoteResumenDto(l.IDProveedor, l.Proveedor.Nombre, l.Precio, l.Stock))
            .ToListAsync(ct);

        _logger.LogInformation("Se obtuvieron {Cantidad} lotes para el producto {ProductoId}", lotes.Count, id);
        return lotes;
    }

    [HttpPost("{id:int}/lotes")]
    [Authorize]
    public async Task<ActionResult<LoteResumenDto>> PostLotes(int id, [FromBody] LoteCreateDto dto, CancellationToken ct)
    {
        if (id != dto.IDProducto)
        {
            _logger.LogWarning("Discrepancia de IDs al crear lote: ruta {RutaId} vs cuerpo {CuerpoId}", id, dto.IDProducto);
            return BadRequest("El ID de la ruta no coincide con el IDProducto del cuerpo.");
        }

        var existeProducto = await _context.Producto.AnyAsync(p => p.ID == id, ct);
        if (!existeProducto)
        {
            _logger.LogWarning("Intento de crear lote para producto inexistente con ID {ProductoId}", id);
            return NotFound("El producto especificado no existe.");
        }

        var proveedor = await _context.Proveedor
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.ID == dto.IDProveedor, ct);
        if (proveedor is null)
        {
            _logger.LogWarning("Intento de crear lote con proveedor inexistente ID {ProveedorId} para producto {ProductoId}", dto.IDProveedor, id);
            return NotFound("El proveedor especificado no existe.");
        }

        var loteExistente = await _context.Lote.AnyAsync(l => l.IDProducto == id && l.IDProveedor == dto.IDProveedor, ct);
        if (loteExistente)
        {
            _logger.LogWarning("Intento de crear lote duplicado para producto {ProductoId} y proveedor {ProveedorId}", id, dto.IDProveedor);
            return Conflict("Ya existe un lote registrado para este producto y proveedor.");
        }

        var lote = new Lote
        {
            IDProducto = id,
            IDProveedor = dto.IDProveedor,
            Precio = dto.Precio,
            Stock = dto.Stock
        };

        _context.Lote.Add(lote);
        await _context.SaveChangesAsync(ct);

        var result = new LoteResumenDto(lote.IDProveedor, proveedor.Nombre, lote.Precio, lote.Stock);
        _logger.LogInformation("Lote creado para producto {ProductoId} y proveedor {ProveedorId}: {@Lote}", id, dto.IDProveedor, result);

        return CreatedAtAction(nameof(GetLotes), new { id }, result);
    }
}