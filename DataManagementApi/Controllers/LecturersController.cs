using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataManagementApi.Data;
using DataManagementApi.Models;

[Route("api/[controller]")]
[ApiController]
public class LecturersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LecturersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/lecturers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LecturerDto>>> GetLecturers(
        [FromQuery] int? departmentId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = _context.Lecturers
                .Include(l => l.Department)
                .AsQueryable();

            // Filter by department if provided
            if (departmentId.HasValue)
            {
                query = query.Where(l => l.DepartmentId == departmentId.Value);
            }

            // Filter by search term if provided (search in name, email, specialization)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(l => 
                    l.Name.ToLower().Contains(searchTerm) ||
                    l.Email.ToLower().Contains(searchTerm) ||
                    (l.Specialization != null && l.Specialization.ToLower().Contains(searchTerm)));
            }

            // Filter by active status if provided
            if (isActive.HasValue)
            {
                query = query.Where(l => l.IsActive == isActive.Value);
            }

            var lecturers = await query
                .OrderBy(l => l.Name)
                .Select(l => new LecturerDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Email = l.Email,
                    PhoneNumber = l.PhoneNumber,
                    DepartmentId = l.DepartmentId,
                    DepartmentName = l.Department != null ? l.Department.Name : null,
                    AcademicRank = l.AcademicRank,
                    Degree = l.Degree,
                    Specialization = l.Specialization,
                    AvatarUrl = l.AvatarUrl,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .ToListAsync();
            
            return Ok(lecturers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // GET: api/lecturers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LecturerDto>> GetLecturer(int id)
    {
        try
        {
            var lecturer = await _context.Lecturers
                .Include(l => l.Department)
                .Where(l => l.Id == id)
                .Select(l => new LecturerDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Email = l.Email,
                    PhoneNumber = l.PhoneNumber,
                    DepartmentId = l.DepartmentId,
                    DepartmentName = l.Department != null ? l.Department.Name : null,
                    AcademicRank = l.AcademicRank,
                    Degree = l.Degree,
                    Specialization = l.Specialization,
                    AvatarUrl = l.AvatarUrl,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (lecturer == null)
            {
                return NotFound(new { message = "Lecturer not found" });
            }

            return Ok(lecturer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // POST: api/lecturers
    [HttpPost]
    public async Task<ActionResult<LecturerDto>> CreateLecturer(Lecturer lecturer)
    {
        try
        {
            if (await _context.Lecturers.AnyAsync(l => l.Email == lecturer.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            lecturer.CreatedAt = DateTime.UtcNow;
            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            // Reload with department info
            var createdLecturer = await _context.Lecturers
                .Include(l => l.Department)
                .Where(l => l.Id == lecturer.Id)
                .Select(l => new LecturerDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Email = l.Email,
                    PhoneNumber = l.PhoneNumber,
                    DepartmentId = l.DepartmentId,
                    DepartmentName = l.Department != null ? l.Department.Name : null,
                    AcademicRank = l.AcademicRank,
                    Degree = l.Degree,
                    Specialization = l.Specialization,
                    AvatarUrl = l.AvatarUrl,
                    IsActive = l.IsActive,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetLecturer), new { id = lecturer.Id }, createdLecturer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // PUT: api/lecturers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLecturer(int id, Lecturer lecturer)
    {
        try
        {
            if (id != lecturer.Id)
            {
                return BadRequest(new { message = "Id mismatch" });
            }

            var existingLecturer = await _context.Lecturers.FindAsync(id);
            if (existingLecturer == null)
            {
                return NotFound(new { message = "Lecturer not found" });
            }

            if (await _context.Lecturers.AnyAsync(l => l.Email == lecturer.Email && l.Id != id))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            existingLecturer.Name = lecturer.Name;
            existingLecturer.Email = lecturer.Email;
            existingLecturer.PhoneNumber = lecturer.PhoneNumber;
            existingLecturer.DepartmentId = lecturer.DepartmentId;
            existingLecturer.AcademicRank = lecturer.AcademicRank;
            existingLecturer.Degree = lecturer.Degree;
            existingLecturer.Specialization = lecturer.Specialization;
            existingLecturer.AvatarUrl = lecturer.AvatarUrl;
            existingLecturer.IsActive = lecturer.IsActive;
            existingLecturer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // DELETE: api/lecturers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLecturer(int id)
    {
        try
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound(new { message = "Lecturer not found" });
            }

            _context.Lecturers.Remove(lecturer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}