using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/lecturers")]
    [ApiController]
    public class LecturersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LecturersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Lecturers
        [HttpGet]
        public async Task<ActionResult<object>> GetLecturers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                IQueryable<Lecturer> query = _context.Lecturers
                    .Include(l => l.Department)
                    .Where(l => l.DeletedAt == null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(l => l.Name.Contains(search) || l.Email.Contains(search) || (l.Specialization != null && l.Specialization.Contains(search)));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var lecturers = await query
                    .OrderBy(l => l.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = lecturers,
                    total = totalCount,
                    page = page,
                    limit = limit
                });
            }
            catch (Exception ex)
            {
                // Log the exception details here if you have a logger
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi truy xuất dữ liệu từ cơ sở dữ liệu: {ex.Message}");
            }
        }

        // GET: api/Lecturers/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Lecturer>>> GetAllLecturers()
        {
            try
            {
                return await _context.Lecturers
                    .Where(d => d.DeletedAt == null)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }
        
        // GET: api/Lecturers/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Lecturer>> GetLecturer(int id)
        {
            try
            {
                var lecturer = await _context.Lecturers
                    .Include(l => l.Department)
                    .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null);

                if (lecturer == null)
                {
                    return NotFound();
                }

                return lecturer;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Lecturers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLecturer(int id, LecturerUpdateDto lecturerDto)
        {
            var existingLecturer = await _context.Lecturers.FindAsync(id);
            if (existingLecturer == null)
            {
                return NotFound();
            }

            if (await _context.Lecturers.AnyAsync(l => l.Email == lecturerDto.Email && l.Id != id))
            {
                return BadRequest(new { message = "Email already exists" });
            }
            
            existingLecturer.Name = lecturerDto.Name;
            existingLecturer.Email = lecturerDto.Email;
            existingLecturer.PhoneNumber = lecturerDto.PhoneNumber;
            existingLecturer.DepartmentId = lecturerDto.DepartmentId;
            existingLecturer.AcademicRank = lecturerDto.AcademicRank;
            existingLecturer.Degree = lecturerDto.Degree;
            existingLecturer.Specialization = lecturerDto.Specialization;
            existingLecturer.AvatarUrl = lecturerDto.AvatarUrl;
            existingLecturer.IsActive = lecturerDto.IsActive;
            existingLecturer.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LecturerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi cập nhật dữ liệu: {ex.Message}");
            }

            var updatedLecturer = await _context.Lecturers.Include(l => l.Department).FirstOrDefaultAsync(l => l.Id == id);
            return Ok(updatedLecturer);
        }

        // POST: api/Lecturers
        [HttpPost]
        public async Task<ActionResult<Lecturer>> PostLecturer(Lecturer lecturer)
        {
            try
            {
                if (await _context.Lecturers.AnyAsync(l => l.Email == lecturer.Email))
                {
                    return BadRequest("Email đã tồn tại.");
                }

                lecturer.CreatedAt = DateTime.UtcNow;
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLecturer), new { id = lecturer.Id }, lecturer);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo mới giảng viên: {ex.Message}");
            }
        }
        
        // SOFT DELETE: api/Lecturers/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteLecturer(int id)
        {
            try
            {
                var lecturer = await _context.Lecturers.FindAsync(id);
                if (lecturer == null)
                {
                    return NotFound();
                }
                if (lecturer.DeletedAt != null)
                {
                    return BadRequest("Giảng viên đã bị xóa mềm.");
                }
                lecturer.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa mềm giảng viên");
            }
        }

        // BULK SOFT DELETE: api/Lecturers/bulk-soft-delete
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var lecturers = await _context.Lecturers.Where(d => ids.Contains(d.Id) && d.DeletedAt == null).ToListAsync();
                if (lecturers.Count == 0)
                    return NotFound("Không tìm thấy giảng viên nào để xóa mềm.");
                foreach (var lecturer in lecturers)
                {
                    lecturer.DeletedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                return Ok(new { softDeleted = lecturers.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa mềm nhiều giảng viên: {ex.Message}");
            }
        }
        
        // PERMANENT DELETE: api/Lecturers/permanent-delete/5
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteLecturer(int id)
        {
            try
            {
                var lecturer = await _context.Lecturers.FindAsync(id);
                if (lecturer == null)
                {
                    return NotFound();
                }
                _context.Lecturers.Remove(lecturer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa vĩnh viễn giảng viên");
            }
        }

        // BULK PERMANENT DELETE: api/Lecturers/bulk-permanent-delete
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var lecturers = await _context.Lecturers.Where(d => ids.Contains(d.Id)).ToListAsync();
                if (lecturers.Count == 0)
                    return NotFound("Không tìm thấy giảng viên nào để xóa vĩnh viễn.");
                _context.Lecturers.RemoveRange(lecturers);
                await _context.SaveChangesAsync();
                return Ok(new { permanentlyDeleted = lecturers.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa vĩnh viễn nhiều giảng viên: {ex.Message}");
            }
        }

        // BULK RESTORE: api/Lecturers/bulk-restore
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var lecturers = await _context.Lecturers.Where(d => ids.Contains(d.Id) && d.DeletedAt != null).ToListAsync();
                if (lecturers.Count == 0)
                    return NotFound("Không tìm thấy giảng viên nào để khôi phục.");
                foreach (var lecturer in lecturers)
                {
                    lecturer.DeletedAt = null;
                }
                await _context.SaveChangesAsync();
                return Ok(new { restored = lecturers.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi khôi phục nhiều giảng viên: {ex.Message}");
            }
        }

        // GET: api/Lecturers/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedLecturers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                var query = _context.Lecturers
                    .Include(l => l.Department)
                    .Where(d => d.DeletedAt != null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(l => l.Name.Contains(search) || l.Email.Contains(search) || (l.Specialization != null && l.Specialization.Contains(search)));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var lecturers = await query
                    .OrderBy(d => d.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = lecturers,
                    total = totalCount,
                    page = page,
                    limit = limit
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.Id == id);
        }
    }
}