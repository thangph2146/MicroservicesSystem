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
        public async Task<ActionResult<Internship>> PostInternship(Internship internship)
        {
            try
            {
                _context.Internships.Add(internship);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetInternship), new { id = internship.Id }, internship);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới đợt thực tập");
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