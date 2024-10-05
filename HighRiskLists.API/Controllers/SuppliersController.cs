using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HighRiskLists.Core.Entities;
using HighRiskLists.Core.DTOs;
using HighRiskLists.Infrastructure.Data;

namespace HighRiskLists.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ApplicationDbContext context, ILogger<SuppliersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/Suppliers
    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierDto supplierDto)
    {
        try
        {
            var supplier = new Supplier
            {
                Name = supplierDto.Name,
                Address = supplierDto.Address,
                TradeName = supplierDto.TradeName,
                TaxId = supplierDto.TaxId,
                PhoneNumber = supplierDto.PhoneNumber,
                Email = supplierDto.Email,
                Website = supplierDto.Website,
                Country = supplierDto.Country,
                AnnualBillingUSD = supplierDto.AnnualBillingUSD,
                CreatedAt = DateTime.UtcNow,
                LastEdited = DateTime.UtcNow
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, MapToDto(supplier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating a supplier");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // GET: api/Suppliers/name/{name}
    [HttpGet("name/{name}")]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliersByName(string name)
    {
        try
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.Name.Contains(name))
                .Select(s => MapToDto(s))
                .ToListAsync();

            if (suppliers.Count == 0)
            {
                return NotFound($"No suppliers found with name containing '{name}'");
            }

            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting suppliers by name");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // DELETE: api/Suppliers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting a supplier");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // Existing GET method for reference
    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }
        return MapToDto(supplier);
    }

    // PUT method for updating a supplier
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, UpdateSupplierDto supplierDto)
    {
        if (id != supplierDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }

        // Update properties
        supplier.Name = supplierDto.Name;
        supplier.Address = supplierDto.Address;
        supplier.TradeName = supplierDto.TradeName;
        supplier.TaxId = supplierDto.TaxId;
        supplier.PhoneNumber = supplierDto.PhoneNumber;
        supplier.Email = supplierDto.Email;
        supplier.Website = supplierDto.Website;
        supplier.Country = supplierDto.Country;
        supplier.AnnualBillingUSD = supplierDto.AnnualBillingUSD;
        supplier.LastEdited = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SupplierExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // GET: api/Suppliers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalSuppliers = await _context.Suppliers.CountAsync();
        var totalPages = (int)Math.Ceiling(totalSuppliers / (double)pageSize);

        var suppliers = await _context.Suppliers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync();

        var response = new
        {
            TotalSuppliers = totalSuppliers,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Suppliers = suppliers
        };

        return Ok(response);
    }

    // GET: api/Suppliers/search
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> SearchSuppliers(
        [FromQuery] string? name,
        [FromQuery] string? country,
        [FromQuery] decimal? minAnnualBilling,
        [FromQuery] decimal? maxAnnualBilling)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(s => s.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(s => s.Country == country);
        }

        if (minAnnualBilling.HasValue)
        {
            query = query.Where(s => s.AnnualBillingUSD >= minAnnualBilling.Value);
        }

        if (maxAnnualBilling.HasValue)
        {
            query = query.Where(s => s.AnnualBillingUSD <= maxAnnualBilling.Value);
        }

        var suppliers = await query.Select(s => MapToDto(s)).ToListAsync();

        return Ok(suppliers);
    }

    private bool SupplierExists(int id)
    {
        return _context.Suppliers.Any(e => e.Id == id);
    }



    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Address = supplier.Address,
            TradeName = supplier.TradeName,
            TaxId = supplier.TaxId,
            PhoneNumber = supplier.PhoneNumber,
            Email = supplier.Email,
            Website = supplier.Website,
            Country = supplier.Country,
            AnnualBillingUSD = supplier.AnnualBillingUSD,
            LastEdited = supplier.LastEdited,
            CreatedAt = supplier.CreatedAt
        };
    }
}