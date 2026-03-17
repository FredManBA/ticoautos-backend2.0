using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicoAutos.Application.DTOs.Vehicles;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VehiclesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Public endpoint: returns paginated and filtered list of vehicles.
    /// GET /api/vehicles?brand=Toyota&minPrice=5000&maxPrice=15000&page=1&pageSize=10
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] VehicleFilterRequest filter)
    {
        IQueryable<Vehicle> query = _unitOfWork.Vehicles.GetQueryable()
            .Include(v => v.Owner);

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(v => v.Brand.ToLower().Contains(filter.Brand.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Model))
            query = query.Where(v => v.Model.ToLower().Contains(filter.Model.ToLower()));

        if (filter.MinYear.HasValue)
            query = query.Where(v => v.Year >= filter.MinYear.Value);

        if (filter.MaxYear.HasValue)
            query = query.Where(v => v.Year <= filter.MaxYear.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(v => v.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(v => v.Price <= filter.MaxPrice.Value);

        if (filter.IsSold.HasValue)
            query = query.Where(v => v.IsSold == filter.IsSold.Value);

        var totalCount = await query.CountAsync();

        var vehicles = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<VehicleResponseDto>
        {
            Items = _mapper.Map<IEnumerable<VehicleResponseDto>>(vehicles),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Public endpoint: returns full detail of a single vehicle by ID.
    /// GET /api/vehicles/5
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _unitOfWork.Vehicles.GetQueryable()
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null) return NotFound();
        return Ok(_mapper.Map<VehicleResponseDto>(vehicle));
    }

    /// <summary>
    /// Protected: creates a new vehicle listing.
    /// POST /api/vehicles
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        // Extraer el userId del JWT claim
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { message = "Token inválido." });

        var vehicle = _mapper.Map<Vehicle>(request);
        vehicle.OwnerId = userId;

        await _unitOfWork.Vehicles.AddAsync(vehicle);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id },
            _mapper.Map<VehicleResponseDto>(vehicle));
    }

    /// <summary>
    /// Protected: updates an existing vehicle.
    /// PUT /api/vehicles/5
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");

        var existing = await _unitOfWork.Vehicles.GetByIdAsync(id);
        if (existing is null) return NotFound();

        _mapper.Map(request, existing);
        _unitOfWork.Vehicles.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return Ok(_mapper.Map<VehicleResponseDto>(existing));
    }

    /// <summary>
    /// Protected: deletes a vehicle listing.
    /// DELETE /api/vehicles/5
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
        if (vehicle is null) return NotFound();

        _unitOfWork.Vehicles.Delete(vehicle);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Protected: marks a vehicle as sold.
    /// PATCH /api/vehicles/5/sold
    /// </summary>
    [HttpPatch("{id}/sold")]
    [Authorize]
    public async Task<IActionResult> MarkAsSold(int id)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
        if (vehicle is null) return NotFound();

        vehicle.IsSold = true;
        _unitOfWork.Vehicles.Update(vehicle);
        await _unitOfWork.SaveChangesAsync();

        return Ok(_mapper.Map<VehicleResponseDto>(vehicle));
    }

    /// <summary>
    /// Returns vehicles owned by the authenticated user.
    /// GET /api/vehicles/my
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var vehicles = await _unitOfWork.Vehicles.GetQueryable()
            .Include(v => v.Owner)
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answer)
            .Where(v => v.OwnerId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<VehicleResponseDto>>(vehicles));
    }
}