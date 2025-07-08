using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicYearsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AcademicYearsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AcademicYears
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicYear>>> GetAcademicYears()
        {
            try
            {
                return await _context.AcademicYears.ToListAsync();
            }
            catch (Exception)
            {
                // Log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/AcademicYears/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AcademicYear>> GetAcademicYear(int id)
        {
            try
            {
                var academicYear = await _context.AcademicYears.FindAsync(id);

                if (academicYear == null)
                {
                    return NotFound("Không tìm thấy năm học");
                }

                return academicYear;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/AcademicYears/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAcademicYear(int id, AcademicYear academicYear)
        {
            if (id != academicYear.Id)
            {
                return BadRequest("ID không khớp");
            }

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(academicYear.Name))
                {
                    return BadRequest("Tên niên khóa không được để trống");
                }

                if (academicYear.StartDate >= academicYear.EndDate)
                {
                    return BadRequest("Ngày bắt đầu phải trước ngày kết thúc");
                }

                // Check if academic year name already exists (excluding current record)
                var existingYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(y => y.Name == academicYear.Name && y.Id != id);
                if (existingYear != null)
                {
                    return BadRequest("Tên niên khóa đã tồn tại");
                }

                _context.Entry(academicYear).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcademicYearExists(id))
                {
                    return NotFound("Không tìm thấy năm học");
                }
                else
                {
                    throw;
                }
            }
            catch(Exception)
            {
                 return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi cập nhật dữ liệu");
            }

            return NoContent();
        }

        // POST: api/AcademicYears
        [HttpPost]
        public async Task<ActionResult<AcademicYear>> PostAcademicYear(AcademicYear academicYear)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(academicYear.Name))
                {
                    return BadRequest("Tên niên khóa không được để trống");
                }

                if (academicYear.StartDate >= academicYear.EndDate)
                {
                    return BadRequest("Ngày bắt đầu phải trước ngày kết thúc");
                }

                // Check if academic year name already exists
                var existingYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(y => y.Name == academicYear.Name);
                if (existingYear != null)
                {
                    return BadRequest("Tên niên khóa đã tồn tại");
                }

                _context.AcademicYears.Add(academicYear);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAcademicYear), new { id = academicYear.Id }, academicYear);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới năm học");
            }
        }

        // DELETE: api/AcademicYears/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAcademicYear(int id)
        {
            try
            {
                var academicYear = await _context.AcademicYears.FindAsync(id);
                if (academicYear == null)
                {
                    return NotFound("Không tìm thấy năm học");
                }

                _context.AcademicYears.Remove(academicYear);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa năm học");
            }
        }

        private bool AcademicYearExists(int id)
        {
            return _context.AcademicYears.Any(e => e.Id == id);
        }
    }
} 