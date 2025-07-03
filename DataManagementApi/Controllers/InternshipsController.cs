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
            // Log request for debugging
            Console.WriteLine($"Received internship creation request with: StudentId={createDto.StudentId}, PartnerId={createDto.PartnerId}, AcademicYearId={createDto.AcademicYearId}, SemesterId={createDto.SemesterId}, Grade={createDto.Grade}");
            
            try
            {
                // Basic validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if related entities exist
                var student = await _context.Users.FindAsync(createDto.StudentId);
                if (student == null)
                {
                    return BadRequest(new { message = $"Sinh viên với ID {createDto.StudentId} không tồn tại" });
                }

                var partner = await _context.Partners.FindAsync(createDto.PartnerId);
                if (partner == null)
                {
                    return BadRequest(new { message = $"Đối tác với ID {createDto.PartnerId} không tồn tại" });
                }

                var academicYear = await _context.AcademicYears.FindAsync(createDto.AcademicYearId);
                if (academicYear == null)
                {
                    return BadRequest(new { message = $"Năm học với ID {createDto.AcademicYearId} không tồn tại" });
                }

                var semester = await _context.Semesters.FindAsync(createDto.SemesterId);
                if (semester == null)
                {
                    return BadRequest(new { message = $"Học kỳ với ID {createDto.SemesterId} không tồn tại" });
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

                // Create new internship with the provided fields
                var internship = new Internship
                {
                    StudentId = createDto.StudentId,
                    PartnerId = createDto.PartnerId,
                    AcademicYearId = createDto.AcademicYearId,
                    SemesterId = createDto.SemesterId,
                    Grade = createDto.Grade,
                    ReportUrl = createDto.ReportUrl
                };

                _context.Internships.Add(internship);
                
                try
                {
                    await _context.SaveChangesAsync();
                    
                    // Reload the created entity with related data
                    var createdInternship = await _context.Internships
                        .Include(i => i.Student)
                        .Include(i => i.Partner)
                        .Include(i => i.AcademicYear)
                        .Include(i => i.Semester)
                        .FirstOrDefaultAsync(i => i.Id == internship.Id);
                        
                    return CreatedAtAction(nameof(GetInternship), new { id = internship.Id }, createdInternship);
                }
                catch (DbUpdateException dbEx)
                {
                    // Log detailed error information
                    Console.Error.WriteLine($"Database error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.Error.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                        Console.Error.WriteLine($"Inner exception stack trace: {dbEx.InnerException.StackTrace}");
                        
                        // Return the specific database error message to help with debugging
                        return StatusCode(StatusCodes.Status500InternalServerError, 
                            new { message = "Lỗi cơ sở dữ liệu", details = dbEx.InnerException.Message });
                    }
                    
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new { message = "Lỗi cơ sở dữ liệu", details = dbEx.Message });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Exception in PostInternship: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Check for inner exception and return detailed information
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.Error.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new { message = "Lỗi khi tạo mới đợt thực tập", details = ex.InnerException.Message });
                }
                
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

        private bool InternshipExists(int id)
        {
            return _context.Internships.Any(e => e.Id == id);
        }
    }
}