using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThesesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ThesesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Theses
        [HttpGet]
        public async Task<ActionResult<object>> GetTheses(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string search = "",
            [FromQuery] DateTime? submissionDate = null
        )
        {
            try
            {
                var query = _context.Theses
                    .Where(t => t.DeletedAt == null) // Filter out soft-deleted
                    .Include(t => t.Student)
                    .Include(t => t.Supervisor)
                    .Include(t => t.Examiner)
                    .Include(t => t.AcademicYear)
                    .Include(t => t.Semester)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t =>
                        t.Title.Contains(search) ||
                        (t.Student != null && t.Student.FullName.Contains(search)) ||
                        (t.Supervisor != null && t.Supervisor.Name.Contains(search))
                    );
                }

                if (submissionDate.HasValue)
                {
                    // Filter by date only (ignore time)
                    var date = submissionDate.Value.Date;
                    query = query.Where(t => t.SubmissionDate.Date == date);
                }

                var total = await query.CountAsync();
                var theses = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(t => new ThesisDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        StudentId = t.StudentId,
                        StudentName = t.Student != null ? t.Student.FullName : null,
                        StudentCode = t.Student != null ? t.Student.StudentCode : null,
                        SupervisorId = t.SupervisorId,
                        SupervisorName = t.Supervisor != null ? t.Supervisor.Name : null,
                        SupervisorEmail = t.Supervisor != null ? t.Supervisor.Email : null,
                        ExaminerId = t.ExaminerId,
                        ExaminerName = t.Examiner != null ? t.Examiner.Name : null,
                        ExaminerEmail = t.Examiner != null ? t.Examiner.Email : null,
                        AcademicYearId = t.AcademicYearId,
                        AcademicYearName = t.AcademicYear != null ? t.AcademicYear.Name : null,
                        SemesterId = t.SemesterId,
                        SemesterName = t.Semester != null ? t.Semester.Name : null,
                        SubmissionDate = t.SubmissionDate,
                        Status = t.Status,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { data = theses, total });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Theses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ThesisDto>> GetThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses
                    .Where(t => t.Id == id && t.DeletedAt == null) // Check for soft-delete
                    .Include(t => t.Student)
                    .Include(t => t.Supervisor)
                    .Include(t => t.Examiner)
                    .Include(t => t.AcademicYear)
                    .Include(t => t.Semester)
                    .Select(t => new ThesisDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        StudentId = t.StudentId,
                        StudentName = t.Student != null ? t.Student.FullName : null,
                        StudentCode = t.Student != null ? t.Student.StudentCode : null,
                        SupervisorId = t.SupervisorId,
                        SupervisorName = t.Supervisor != null ? t.Supervisor.Name : null,
                        SupervisorEmail = t.Supervisor != null ? t.Supervisor.Email : null,
                        ExaminerId = t.ExaminerId,
                        ExaminerName = t.Examiner != null ? t.Examiner.Name : null,
                        ExaminerEmail = t.Examiner != null ? t.Examiner.Email : null,
                        AcademicYearId = t.AcademicYearId,
                        AcademicYearName = t.AcademicYear != null ? t.AcademicYear.Name : null,
                        SemesterId = t.SemesterId,
                        SemesterName = t.Semester != null ? t.Semester.Name : null,
                        SubmissionDate = t.SubmissionDate,
                        Status = t.Status,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (thesis == null)
                {
                    return NotFound();
                }

                return thesis;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Theses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutThesis(int id, CreateThesisDto thesisDto)
        {
            try
            {
                var existingThesis = await _context.Theses.FindAsync(id);
                if (existingThesis == null)
                {
                    return NotFound();
                }

                // Validate that referenced entities exist
                var student = await _context.Students.FindAsync(thesisDto.StudentId);
                if (student == null)
                {
                    return BadRequest("Sinh viên không tồn tại");
                }

                var supervisor = await _context.Lecturers.FindAsync(thesisDto.SupervisorId);
                if (supervisor == null)
                {
                    return BadRequest("Giảng viên hướng dẫn không tồn tại");
                }

                if (thesisDto.ExaminerId.HasValue)
                {
                    var examiner = await _context.Lecturers.FindAsync(thesisDto.ExaminerId.Value);
                    if (examiner == null)
                    {
                        return BadRequest("Giảng viên phản biện không tồn tại");
                    }
                }

                var academicYear = await _context.AcademicYears.FindAsync(thesisDto.AcademicYearId);
                if (academicYear == null)
                {
                    return BadRequest("Năm học không tồn tại");
                }

                var semester = await _context.Semesters.FindAsync(thesisDto.SemesterId);
                if (semester == null)
                {
                    return BadRequest("Học kỳ không tồn tại");
                }

                // Update the thesis entity
                existingThesis.Title = thesisDto.Title;
                existingThesis.Description = thesisDto.Description;
                existingThesis.StudentId = thesisDto.StudentId;
                existingThesis.SupervisorId = thesisDto.SupervisorId;
                existingThesis.ExaminerId = thesisDto.ExaminerId;
                existingThesis.AcademicYearId = thesisDto.AcademicYearId;
                existingThesis.SemesterId = thesisDto.SemesterId;
                existingThesis.SubmissionDate = thesisDto.SubmissionDate;
                existingThesis.Status = thesisDto.Status;
                existingThesis.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi cập nhật dữ liệu");
            }
        }

        // POST: api/Theses
        [HttpPost]
        public async Task<ActionResult<ThesisDto>> PostThesis(CreateThesisDto thesisDto)
        {
            try
            {
                // Validate that referenced entities exist
                var student = await _context.Students.FindAsync(thesisDto.StudentId);
                if (student == null)
                {
                    return BadRequest("Sinh viên không tồn tại");
                }

                var supervisor = await _context.Lecturers.FindAsync(thesisDto.SupervisorId);
                if (supervisor == null)
                {
                    return BadRequest("Giảng viên hướng dẫn không tồn tại");
                }

                if (thesisDto.ExaminerId.HasValue)
                {
                    var examiner = await _context.Lecturers.FindAsync(thesisDto.ExaminerId.Value);
                    if (examiner == null)
                    {
                        return BadRequest("Giảng viên phản biện không tồn tại");
                    }
                }

                var academicYear = await _context.AcademicYears.FindAsync(thesisDto.AcademicYearId);
                if (academicYear == null)
                {
                    return BadRequest("Năm học không tồn tại");
                }

                var semester = await _context.Semesters.FindAsync(thesisDto.SemesterId);
                if (semester == null)
                {
                    return BadRequest("Học kỳ không tồn tại");
                }

                // Create the thesis entity
                var thesis = new Thesis
                {
                    Title = thesisDto.Title,
                    Description = thesisDto.Description,
                    StudentId = thesisDto.StudentId,
                    SupervisorId = thesisDto.SupervisorId,
                    ExaminerId = thesisDto.ExaminerId,
                    AcademicYearId = thesisDto.AcademicYearId,
                    SemesterId = thesisDto.SemesterId,
                    SubmissionDate = thesisDto.SubmissionDate,
                    Status = thesisDto.Status ?? "Draft",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Theses.Add(thesis);
                await _context.SaveChangesAsync();

                // Load the full thesis with related entities for the response
                var createdThesis = await _context.Theses
                    .Include(t => t.Student)
                    .Include(t => t.Supervisor)
                    .Include(t => t.Examiner)
                    .Include(t => t.AcademicYear)
                    .Include(t => t.Semester)
                    .Where(t => t.Id == thesis.Id)
                    .Select(t => new ThesisDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        StudentId = t.StudentId,
                        StudentName = t.Student != null ? t.Student.FullName : null,
                        StudentCode = t.Student != null ? t.Student.StudentCode : null,
                        SupervisorId = t.SupervisorId,
                        SupervisorName = t.Supervisor != null ? t.Supervisor.Name : null,
                        SupervisorEmail = t.Supervisor != null ? t.Supervisor.Email : null,
                        ExaminerId = t.ExaminerId,
                        ExaminerName = t.Examiner != null ? t.Examiner.Name : null,
                        ExaminerEmail = t.Examiner != null ? t.Examiner.Email : null,
                        AcademicYearId = t.AcademicYearId,
                        AcademicYearName = t.AcademicYear != null ? t.AcademicYear.Name : null,
                        SemesterId = t.SemesterId,
                        SemesterName = t.Semester != null ? t.Semester.Name : null,
                        SubmissionDate = t.SubmissionDate,
                        Status = t.Status,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetThesis), new { id = thesis.Id }, createdThesis);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi tạo mới khóa luận: {ex.Message}");
            }
        }

        // SOFT DELETE: api/Theses/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses.FindAsync(id);
                if (thesis == null)
                {
                    return NotFound();
                }
                thesis.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa mềm khóa luận");
            }
        }

        // PERMANENT DELETE: api/Theses/permanent-delete/5
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
                if (thesis == null)
                {
                    return NotFound();
                }
                _context.Theses.Remove(thesis);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa vĩnh viễn khóa luận");
            }
        }

        // RESTORE: api/Theses/restore/5
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> RestoreThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
                if (thesis == null)
                {
                    return NotFound();
                }
                thesis.DeletedAt = null;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi khôi phục khóa luận");
            }
        }

        // GET DELETED: api/Theses/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedTheses([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                IQueryable<Thesis> query = _context.Theses
                    .Where(t => t.DeletedAt != null)
                    .Include(t => t.Student)
                    .Include(t => t.Supervisor);

                if (!string.IsNullOrWhiteSpace(search))
                {
                     query = query.Where(t =>
                        t.Title.Contains(search) ||
                        (t.Student != null && t.Student.FullName.Contains(search))
                    );
                }

                var total = await query.CountAsync();
                var theses = await query
                    .OrderByDescending(t => t.DeletedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(t => new ThesisDto {
                        Id = t.Id,
                        Title = t.Title,
                        StudentName = t.Student != null ? t.Student.FullName : null,
                        SupervisorName = t.Supervisor != null ? t.Supervisor.Name : null,
                        Status = t.Status,
                        DeletedAt = t.DeletedAt
                    })
                    .ToListAsync();

                return Ok(new { data = theses, total });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi lấy danh sách khóa luận đã xóa");
            }
        }

        // BULK SOFT DELETE
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");
            try
            {
                var theses = await _context.Theses.Where(t => ids.Contains(t.Id)).ToListAsync();
                foreach (var thesis in theses)
                {
                    thesis.DeletedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                return Ok(new { softDeleted = theses.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa mềm nhiều khóa luận: {ex.Message}");
            }
        }

        // BULK RESTORE
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");
            try
            {
                var theses = await _context.Theses.IgnoreQueryFilters().Where(t => ids.Contains(t.Id)).ToListAsync();
                foreach (var thesis in theses)
                {
                    thesis.DeletedAt = null;
                }
                await _context.SaveChangesAsync();
                return Ok(new { restored = theses.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi khôi phục nhiều khóa luận: {ex.Message}");
            }
        }
        
        // BULK PERMANENT DELETE
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");
            try
            {
                var theses = await _context.Theses.IgnoreQueryFilters().Where(t => ids.Contains(t.Id)).ToListAsync();
                _context.Theses.RemoveRange(theses);
                await _context.SaveChangesAsync();
                return Ok(new { permanentlyDeleted = theses.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa vĩnh viễn nhiều khóa luận: {ex.Message}");
            }
        }

        // DELETE: api/Theses/5 (This is now replaced by soft-delete and permanent-delete)
        // I'll comment it out to avoid confusion, but it could be removed entirely.
        /*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses.FindAsync(id);
                if (thesis == null)
                {
                    return NotFound();
                }

                _context.Theses.Remove(thesis);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa khóa luận");
            }
        }
        */

        private bool ThesisExists(int id)
        {
            return _context.Theses.Any(e => e.Id == id && e.DeletedAt == null);
        }
    }
}