using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HighRiskLists.Core.DTOs;
using HighRiskLists.Infrastructure.Data;
using HighRiskLists.Core.Entities;

namespace HighRiskLists.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .Select(s => MapToDto(s))
            .ToListAsync();
        return Ok(suppliers);
    }

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

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierDto supplierDto)
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
            LastEdited = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, MapToDto(supplier));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, UpdateSupplierDto supplierDto)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }

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
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
        {
            return NotFound();
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return NoContent();
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