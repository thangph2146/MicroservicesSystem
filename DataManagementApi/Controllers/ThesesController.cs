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
        public async Task<ActionResult<IEnumerable<Thesis>>> GetTheses()
        {
            try
            {
                return await _context.Theses
                    .Include(t => t.Student)
                    .Include(t => t.AcademicYear)
                    .Include(t => t.Semester)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Theses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Thesis>> GetThesis(int id)
        {
            try
            {
                var thesis = await _context.Theses
                    .Include(t => t.Student)
                    .Include(t => t.AcademicYear)
                    .Include(t => t.Semester)
                    .FirstOrDefaultAsync(t => t.Id == id);

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
        public async Task<IActionResult> PutThesis(int id, Thesis thesis)
        {
            if (id != thesis.Id)
            {
                return BadRequest();
            }

            _context.Entry(thesis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThesisExists(id))
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

        // POST: api/Theses
        [HttpPost]
        public async Task<ActionResult<Thesis>> PostThesis(Thesis thesis)
        {
            try
            {
                _context.Theses.Add(thesis);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetThesis), new { id = thesis.Id }, thesis);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới khóa luận");
            }
        }

        // DELETE: api/Theses/5
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

        private bool ThesisExists(int id)
        {
            return _context.Theses.Any(e => e.Id == id);
        }
    }
} 