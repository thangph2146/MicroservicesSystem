using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<object>> GetStudents([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                IQueryable<Student> query = _context.Students
                    .Where(s => s.DeletedAt == null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.FullName.Contains(search) || s.StudentCode.Contains(search) || s.Email.Contains(search));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var students = await query
                    .OrderBy(s => s.FullName)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = students,
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

        // GET: api/Students/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

                if (student == null)
                {
                    return NotFound();
                }

                return student;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, StudentUpdateDto studentDto)
        {
            var existingStudent = await _context.Students.FindAsync(id);

            if (existingStudent == null || existingStudent.DeletedAt != null)
            {
                return NotFound();
            }

            // Check if department exists if provided
            if (studentDto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(studentDto.DepartmentId.Value);
                if (department == null)
                {
                    return BadRequest("Khoa không tồn tại.");
                }
            }

            // Manually map properties
            existingStudent.StudentCode = studentDto.StudentCode;
            existingStudent.FullName = studentDto.FullName;
            existingStudent.DateOfBirth = studentDto.DateOfBirth;
            existingStudent.Email = studentDto.Email;
            existingStudent.PhoneNumber = studentDto.PhoneNumber;
            existingStudent.DepartmentId = studentDto.DepartmentId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi cập nhật dữ liệu");
            }

            return NoContent();
        }

        // POST: api/Students
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới sinh viên");
            }
        }

        // SOFT DELETE: api/Students/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteStudent(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound();
                }
                if (student.DeletedAt != null)
                {
                    return BadRequest("Sinh viên đã bị xóa mềm.");
                }
                student.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa mềm sinh viên");
            }
        }

        // BULK SOFT DELETE: api/Students/bulk-soft-delete
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var students = await _context.Students.Where(s => ids.Contains(s.Id) && s.DeletedAt == null).ToListAsync();
                if (students.Count == 0)
                    return NotFound("Không tìm thấy sinh viên nào để xóa mềm.");
                foreach (var student in students)
                {
                    student.DeletedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                return Ok(new { softDeleted = students.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa mềm nhiều sinh viên: {ex.Message}");
            }
        }

        // PERMANENT DELETE: api/Students/permanent-delete/5
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteStudent(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound();
                }
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa vĩnh viễn sinh viên");
            }
        }

        // BULK PERMANENT DELETE: api/Students/bulk-permanent-delete
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var students = await _context.Students.Where(s => ids.Contains(s.Id)).ToListAsync();
                if (students.Count == 0)
                    return NotFound("Không tìm thấy sinh viên nào để xóa vĩnh viễn.");
                _context.Students.RemoveRange(students);
                await _context.SaveChangesAsync();
                return Ok(new { permanentlyDeleted = students.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa vĩnh viễn nhiều sinh viên: {ex.Message}");
            }
        }

        // BULK RESTORE: api/Students/bulk-restore
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var students = await _context.Students.Where(s => ids.Contains(s.Id) && s.DeletedAt != null).ToListAsync();
                if (students.Count == 0)
                    return NotFound("Không tìm thấy sinh viên nào để khôi phục.");
                foreach (var student in students)
                {
                    student.DeletedAt = null;
                }
                await _context.SaveChangesAsync();
                return Ok(new { restored = students.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi khôi phục nhiều sinh viên: {ex.Message}");
            }
        }

        // GET: api/Students/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedStudents([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                var query = _context.Students
                    .Where(s => s.DeletedAt != null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.FullName.Contains(search) || s.StudentCode.Contains(search) || s.Email.Contains(search));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var students = await query
                    .OrderBy(s => s.FullName)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = students,
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

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
} 