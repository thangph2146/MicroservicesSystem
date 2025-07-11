using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternshipsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InternshipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Internships
        [HttpGet]
        public async Task<ActionResult<object>> GetInternships(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10, 
            [FromQuery] string search = "")
        {
            var query = _context.Internships
                .Where(i => i.DeletedAt == null)
                .Include(i => i.Student)
                .Include(i => i.Partner)
                .Include(i => i.AcademicYear)
                .Include(i => i.Semester)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                // Note: The 'Title' property does not exist on the Internship model.
                // Assuming you want to search by student name or partner name.
                // Please adjust the model if a 'Title' is needed.
                query = query.Where(i => (i.Student != null && i.Student.Name.Contains(search)) ||
                                         (i.Partner != null && i.Partner.Name.Contains(search)));
            }
            
            var totalCount = await query.CountAsync();

            var internships = await query
                .OrderByDescending(i => i.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return Ok(new 
            {
                data = internships,
                total = totalCount,
                page,
                limit
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Internship>>> GetAllInternships()
        {
            return await _context.Internships
                .Where(i => i.DeletedAt == null)
                .OrderByDescending(i => i.Id)
                .ToListAsync();
        }

        // GET: api/Internships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Internship>> GetInternship(int id)
        {
            var internship = await _context.Internships
                .Where(i => i.Id == id && i.DeletedAt == null)
                .Include(i => i.Student)
                .Include(i => i.Partner)
                .Include(i => i.AcademicYear)
                .Include(i => i.Semester)
                .FirstOrDefaultAsync();

            if (internship == null)
            {
                return NotFound();
            }

            return internship;
        }
        
        // POST: api/Internships
        [HttpPost]
        public async Task<ActionResult<Internship>> PostInternship(CreateInternshipDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _context.Users.FindAsync(createDto.StudentId);
            if (student == null) return BadRequest(new { message = $"Sinh viên với ID {createDto.StudentId} không tồn tại" });

            var partner = await _context.Partners.FindAsync(createDto.PartnerId);
            if (partner == null) return BadRequest(new { message = $"Đối tác với ID {createDto.PartnerId} không tồn tại" });

            var academicYear = await _context.AcademicYears.FindAsync(createDto.AcademicYearId);
            if (academicYear == null) return BadRequest(new { message = $"Năm học với ID {createDto.AcademicYearId} không tồn tại" });
            
            var semester = await _context.Semesters.FindAsync(createDto.SemesterId);
            if (semester == null) return BadRequest(new { message = $"Học kỳ với ID {createDto.SemesterId} không tồn tại" });

            var existingInternship = await _context.Internships
                .AnyAsync(i => i.StudentId == createDto.StudentId && 
                               i.AcademicYearId == createDto.AcademicYearId && 
                               i.SemesterId == createDto.SemesterId &&
                               i.DeletedAt == null);
            if (existingInternship)
            {
                return BadRequest(new { message = "Sinh viên đã có thực tập trong năm học và học kỳ này" });
            }

            var internship = new Internship
            {
                StudentId = createDto.StudentId,
                PartnerId = createDto.PartnerId,
                AcademicYearId = createDto.AcademicYearId,
                SemesterId = createDto.SemesterId,
                Grade = createDto.Grade,
                ReportUrl = createDto.ReportUrl,
                DeletedAt = null // Ensure not deleted on creation
            };

            _context.Internships.Add(internship);
            await _context.SaveChangesAsync();
            
            var createdInternship = await _context.Internships
                .Include(i => i.Student)
                .Include(i => i.Partner)
                .Include(i => i.AcademicYear)
                .Include(i => i.Semester)
                .FirstOrDefaultAsync(i => i.Id == internship.Id);

            return CreatedAtAction(nameof(GetInternship), new { id = internship.Id }, createdInternship);
        }

        // PUT: api/Internships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInternship(int id, InternshipUpdateDto updateDto)
        {
            var existingInternship = await _context.Internships.FindAsync(id);

            if (existingInternship == null || existingInternship.DeletedAt != null)
            {
                return NotFound("Kỳ thực tập không tồn tại hoặc đã bị xóa.");
            }
            
            // Update properties from DTO if they are provided
            if (updateDto.StudentId.HasValue) existingInternship.StudentId = updateDto.StudentId.Value;
            if (updateDto.PartnerId.HasValue) existingInternship.PartnerId = updateDto.PartnerId.Value;
            if (updateDto.AcademicYearId.HasValue) existingInternship.AcademicYearId = updateDto.AcademicYearId.Value;
            if (updateDto.SemesterId.HasValue) existingInternship.SemesterId = updateDto.SemesterId.Value;
            if (updateDto.Grade.HasValue) existingInternship.Grade = updateDto.Grade.Value;
            if (updateDto.ReportUrl != null) existingInternship.ReportUrl = updateDto.ReportUrl;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InternshipExists(id))
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
        
        // SOFT DELETE: api/internships/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteInternship(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null) return NotFound();
            if (internship.DeletedAt != null) return BadRequest("Kỳ thực tập đã được xóa.");

            internship.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // GET: api/internships/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedInternships([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            var query = _context.Internships
                .Where(i => i.DeletedAt != null)
                 .Include(i => i.Student)
                .Include(i => i.Partner)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(search))
            {
                 query = query.Where(i => (i.Student != null && i.Student.Name.Contains(search)) ||
                                         (i.Partner != null && i.Partner.Name.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var internships = await query
                .OrderByDescending(i => i.DeletedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
            
            return Ok(new { data = internships, total = totalCount, page, limit });
        }
        
        // BULK SOFT DELETE: api/internships/bulk-soft-delete
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");

            var internships = await _context.Internships.Where(i => ids.Contains(i.Id) && i.DeletedAt == null).ToListAsync();
            if (internships.Count == 0) return NotFound("Không tìm thấy kỳ thực tập hợp lệ để xóa.");

            foreach (var internship in internships)
            {
                internship.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã xóa thành công {internships.Count} kỳ thực tập."});
        }
        
        // BULK RESTORE: api/internships/bulk-restore
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");
            
            var internships = await _context.Internships.Where(i => ids.Contains(i.Id) && i.DeletedAt != null).ToListAsync();
            if (internships.Count == 0) return NotFound("Không tìm thấy kỳ thực tập hợp lệ để khôi phục.");
            
            foreach (var internship in internships)
            {
                internship.DeletedAt = null;
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã khôi phục thành công {internships.Count} kỳ thực tập."});
        }

        // BULK PERMANENT DELETE: api/internships/bulk-permanent-delete
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");

            var internships = await _context.Internships.Where(r => ids.Contains(r.Id)).ToListAsync();
            if (internships.Count == 0) return NotFound("Không tìm thấy kỳ thực tập hợp lệ để xóa vĩnh viễn.");

            _context.Internships.RemoveRange(internships);
            
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã xóa vĩnh viễn {internships.Count} kỳ thực tập." });
        }

        // Replaces the old DELETE endpoint
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteInternship(int id)
        {
            var internship = await _context.Internships.FindAsync(id);
            if (internship == null)
            {
                return NotFound();
            }

            _context.Internships.Remove(internship);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InternshipExists(int id)
        {
            return _context.Internships.Any(e => e.Id == id && e.DeletedAt == null);
        }
    }
}