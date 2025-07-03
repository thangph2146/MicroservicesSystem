using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemestersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SemestersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Semesters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Semester>>> GetSemesters()
        {
            try
            {
                return await _context.Semesters.Include(s => s.AcademicYear).ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Semesters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Semester>> GetSemester(int id)
        {
            try
            {
                var semester = await _context.Semesters.Include(s => s.AcademicYear)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (semester == null)
                {
                    return NotFound();
                }

                return semester;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Semesters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSemester(int id, CreateSemesterDto semesterDto)
        {
            try
            {
                var semester = await _context.Semesters.FindAsync(id);
                if (semester == null)
                {
                    return NotFound();
                }

                // Validate academic year exists
                var academicYearExists = await _context.AcademicYears.AnyAsync(ay => ay.Id == semesterDto.AcademicYearId);
                if (!academicYearExists)
                {
                    return BadRequest(new { message = "Năm học không tồn tại" });
                }

                semester.Name = semesterDto.Name;
                semester.AcademicYearId = semesterDto.AcademicYearId;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi cập nhật dữ liệu");
            }
        }

        // POST: api/Semesters
        [HttpPost]
        public async Task<ActionResult<Semester>> PostSemester(CreateSemesterDto semesterDto)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"Creating semester: {semesterDto?.Name}, AcademicYearId: {semesterDto?.AcademicYearId}");
                
                // Validate input
                if (semesterDto == null)
                {
                    return BadRequest(new { message = "Dữ liệu học kỳ không hợp lệ" });
                }
                
                if (string.IsNullOrWhiteSpace(semesterDto.Name))
                {
                    return BadRequest(new { message = "Tên học kỳ không được để trống" });
                }

                // Validate academic year exists
                var academicYear = await _context.AcademicYears.FindAsync(semesterDto.AcademicYearId);
                if (academicYear == null)
                {
                    return BadRequest(new { message = "Năm học không tồn tại" });
                }

                // Check for duplicate semester name in the same academic year
                var existingSemester = await _context.Semesters
                    .AnyAsync(s => s.Name == semesterDto.Name && s.AcademicYearId == semesterDto.AcademicYearId);
                if (existingSemester)
                {
                    return BadRequest(new { message = "Học kỳ đã tồn tại trong năm học này" });
                }

                var semester = new Semester
                {
                    Name = semesterDto.Name,
                    AcademicYearId = semesterDto.AcademicYearId
                };

                _context.Semesters.Add(semester);
                await _context.SaveChangesAsync();

                // Load the created semester with academic year
                var createdSemester = await _context.Semesters
                    .Include(s => s.AcademicYear)
                    .FirstOrDefaultAsync(s => s.Id == semester.Id);

                return CreatedAtAction(nameof(GetSemester), new { id = semester.Id }, createdSemester);
            }
            catch (DbUpdateException dbEx)
            {
                // Log the detailed exception
                Console.Error.WriteLine($"DB update error: {dbEx}");
                return BadRequest(new { message = "Lỗi cơ sở dữ liệu: không thể tạo học kỳ." });
            }
            catch (Exception ex)
            {
                // Log the detailed exception
                Console.Error.WriteLine($"Unhandled error: {ex}");
                return StatusCode(500, new { message = "Lỗi khi tạo mới học kỳ", details = ex.Message });
            }
        }

        // DELETE: api/Semesters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            try
            {
                var semester = await _context.Semesters.FindAsync(id);
                if (semester == null)
                {
                    return NotFound();
                }

                _context.Semesters.Remove(semester);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa học kỳ");
            }
        }

        private bool SemesterExists(int id)
        {
            return _context.Semesters.Any(e => e.Id == id);
        }
    }
}