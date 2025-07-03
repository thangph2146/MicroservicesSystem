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
        public async Task<ActionResult<IEnumerable<Internship>>> GetInternships()
        {
            try
            {
                return await _context.Internships
                    .Include(i => i.Student)
                    .Include(i => i.Partner)
                    .Include(i => i.AcademicYear)
                    .Include(i => i.Semester)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Internships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Internship>> GetInternship(int id)
        {
            try
            {
                var internship = await _context.Internships
                    .Include(i => i.Student)
                    .Include(i => i.Partner)
                    .Include(i => i.AcademicYear)
                    .Include(i => i.Semester)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (internship == null)
                {
                    return NotFound();
                }

                return internship;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Internships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInternship(int id, Internship internship)
        {
            if (id != internship.Id)
            {
                return BadRequest();
            }

            _context.Entry(internship).State = EntityState.Modified;

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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi cập nhật dữ liệu");
            }

            return NoContent();
        }

        // POST: api/Internships
        [HttpPost]
        public async Task<ActionResult<Internship>> PostInternship(CreateInternshipDto createDto)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Debug log to check what's happening
                Console.WriteLine($"Checking for user ID: {createDto.StudentId}");
                
                // Check if related entities exist - use Users instead of Students
                var userExists = await _context.Users.AnyAsync(u => u.Id == createDto.StudentId);
                Console.WriteLine($"User exists: {userExists}");
                
                if (!userExists)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại" });
                }

                var partnerExists = await _context.Partners.AnyAsync(p => p.Id == createDto.PartnerId);
                if (!partnerExists)
                {
                    return BadRequest(new { message = "Đối tác không tồn tại" });
                }

                var academicYearExists = await _context.AcademicYears.AnyAsync(ay => ay.Id == createDto.AcademicYearId);
                if (!academicYearExists)
                {
                    return BadRequest(new { message = "Năm học không tồn tại" });
                }

                var semesterExists = await _context.Semesters.AnyAsync(s => s.Id == createDto.SemesterId);
                if (!semesterExists)
                {
                    return BadRequest(new { message = "Học kỳ không tồn tại" });
                }

                // Check for duplicate internship
                var existingInternship = await _context.Internships
                    .AnyAsync(i => i.StudentId == createDto.StudentId && 
                                   i.AcademicYearId == createDto.AcademicYearId && 
                                   i.SemesterId == createDto.SemesterId);
                if (existingInternship)
                {
                    return BadRequest(new { message = "Sinh viên đã có thực tập trong năm học và học kỳ này" });
                }

                // Create the internship entity
                var internship = new Internship
                {
                    StudentId = createDto.StudentId,
                    PartnerId = createDto.PartnerId,
                    AcademicYearId = createDto.AcademicYearId,
                    SemesterId = createDto.SemesterId,
                    ReportUrl = createDto.ReportUrl,
                    Grade = createDto.Grade
                };

                _context.Internships.Add(internship);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdInternship = await _context.Internships
                    .Include(i => i.Student)
                    .Include(i => i.Partner)
                    .Include(i => i.AcademicYear)
                    .Include(i => i.Semester)
                    .FirstOrDefaultAsync(i => i.Id == internship.Id);

                return CreatedAtAction(nameof(GetInternship), new { id = internship.Id }, createdInternship);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Lỗi khi tạo mới đợt thực tập", details = ex.Message });
            }
        }

        // DELETE: api/Internships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInternship(int id)
        {
            try
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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa đợt thực tập");
            }
        }

        // DEBUG endpoint - remove in production
        [HttpGet("debug/user/{id}")]
        public async Task<ActionResult> DebugUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                var userExists = await _context.Users.AnyAsync(u => u.Id == id);
                
                return Ok(new { 
                    userId = id,
                    userExists = userExists,
                    user = user,
                    allUsers = await _context.Users.Select(u => u.Id).ToListAsync()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private bool InternshipExists(int id)
        {
            return _context.Internships.Any(e => e.Id == id);
        }
    }
}