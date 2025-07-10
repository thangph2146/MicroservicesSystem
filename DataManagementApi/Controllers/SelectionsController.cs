using DataManagementApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelectionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const int MaxItems = 100;

        public SelectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("academic-years")]
        public async Task<IActionResult> GetAcademicYears([FromQuery] string? search)
        {
            var query = _context.AcademicYears.AsQueryable().Where(ay => ay.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(ay => ay.Name.ToLower().Contains(lowerSearch));
            }

            var years = await query
                .Select(ay => new { ay.Id, ay.Name })
                .Take(MaxItems)
                .ToListAsync();
            return Ok(years);
        }

        [HttpGet("semesters")]
        public async Task<IActionResult> GetSemesters([FromQuery] string? search)
        {
            var query = _context.Semesters.AsQueryable().Where(s => s.DeletedAt == null);
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(lowerSearch));
            }

            var semesters = await query
                .Select(s => new { s.Id, s.Name })
                .Take(MaxItems)
                .ToListAsync();
            return Ok(semesters);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] string? search)
        {
            var query = _context.Students.AsQueryable().Where(s => s.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(s => s.FullName.ToLower().Contains(lowerSearch) || s.StudentCode.ToLower().Contains(lowerSearch));
            }
            
            var students = await query
                .Select(s => new { s.Id, Name = s.FullName }) // Use FullName for consistency
                .Take(MaxItems)
                .ToListAsync();
            return Ok(students);
        }

        [HttpGet("lecturers")]
        public async Task<IActionResult> GetLecturers([FromQuery] string? search)
        {
            var query = _context.Lecturers.AsQueryable().Where(l => l.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(l => l.Name.ToLower().Contains(lowerSearch) || (l.Email != null && l.Email.ToLower().Contains(lowerSearch)));
            }

            var lecturers = await query
                .Select(l => new { l.Id, l.Name })
                .Take(MaxItems)
                .ToListAsync();
            return Ok(lecturers);
        }
    }
} 