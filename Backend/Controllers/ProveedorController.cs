using Database;
using DTOs.Proveedor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/proveedores")]
[ApiController]
public class ProveedorController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ProveedorController> _logger;

    public ProveedorController(DatabaseContext context, ILogger<ProveedorController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation("Obteniendo listado completo de proveedores");

        var proveedores = await _context.Proveedor
            .AsNoTracking()
            .Select(p => new ProveedorDto(p.ID, p.Nombre, p.Direccion, p.RUC))
            .ToListAsync(ct);

        _logger.LogInformation("Se obtuvieron {Cantidad} proveedores", proveedores.Count);
        return proveedores;
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ProveedorDto>> Get(int id, CancellationToken ct)
    {
        _logger.LogInformation("Buscando proveedor con ID {ProveedorId}", id);

        var proveedor = await _context.Proveedor
            .AsNoTracking()
            .Where(p => p.ID == id)
            .Select(p => new ProveedorDto(p.ID, p.Nombre, p.Direccion, p.RUC))
            .FirstOrDefaultAsync(ct);

        if (proveedor is null)
        {
            _logger.LogWarning("No se encontró el proveedor con ID {ProveedorId}", id);
            return NotFound();
        }

        return proveedor;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProveedorDto>> Post([FromBody] ProveedorCreateUpdateDto dto, CancellationToken ct)
    {
        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            Direccion = dto.Direccion,
            RUC = dto.RUC
        };

        _context.Proveedor.Add(proveedor);
        await _context.SaveChangesAsync(ct);

        var result = new ProveedorDto(proveedor.ID, proveedor.Nombre, proveedor.Direccion, proveedor.RUC);
        _logger.LogInformation("Proveedor creado exitosamente: {@Proveedor}", result);

        return CreatedAtAction(nameof(Get), new { id = proveedor.ID }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Put(int id, [FromBody] ProveedorCreateUpdateDto dto, CancellationToken ct)
    {
        var proveedor = await _context.Proveedor.FindAsync(new object[] { id }, ct);
        if (proveedor is null)
        {
            _logger.LogWarning("Intento de actualizar proveedor inexistente con ID {ProveedorId}", id);
            return NotFound();
        }

        proveedor.Nombre = dto.Nombre;
        proveedor.Direccion = dto.Direccion;
        proveedor.RUC = dto.RUC;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Proveedor con ID {ProveedorId} actualizado (RUC: {RUC})", id, proveedor.RUC);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var proveedor = await _context.Proveedor.FindAsync(new object[] { id }, ct);
        if (proveedor is null)
        {
            _logger.LogWarning("Intento de eliminar proveedor inexistente con ID {ProveedorId}", id);
            return NotFound();
        }

        _context.Proveedor.Remove(proveedor);

        try
        {
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Proveedor con ID {ProveedorId} eliminado", id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de integridad al intentar eliminar el proveedor {ProveedorId}", id);
            return Conflict("El proveedor está referenciado por otros registros.");
        }

        return NoContent();
    }
}