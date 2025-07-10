using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<object>> GetDepartments([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                IQueryable<Department> query = _context.Departments
                    .Where(d => d.ParentDepartmentId == null && d.DeletedAt == null)
                    .Include(d => d.ChildDepartments);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination - but for tree structure, we might want to return all for now
                // since pagination with tree structure is complex
                var departments = await query
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                // Filter child departments to exclude deleted ones
                foreach (var dept in departments)
                {
                    if (dept.ChildDepartments != null)
                    {
                        dept.ChildDepartments = dept.ChildDepartments.Where(cd => cd.DeletedAt == null).ToList();
                    }
                }

                // Return paginated response format
                return Ok(new
                {
                    data = departments,
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

        // GET: api/Departments/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            try
            {
                // Lấy tất cả departments dạng flat list
                return await _context.Departments
                    .Where(d => d.DeletedAt == null)
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.ParentDepartment)
                    .Include(d => d.ChildDepartments)
                    .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);

                if (department == null)
                {
                    return NotFound();
                }

                return department;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // PUT: api/Departments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.Id)
            {
                return BadRequest();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
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

        // POST: api/Departments
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới đơn vị");
            }
        }

        // SOFT DELETE: api/Departments/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound();
                }
                if (department.DeletedAt != null)
                {
                    return BadRequest("Đơn vị đã bị xóa mềm.");
                }
                department.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa mềm đơn vị");
            }
        }

        // BULK SOFT DELETE: api/Departments/bulk-soft-delete
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var departments = await _context.Departments.Where(d => ids.Contains(d.Id) && d.DeletedAt == null).ToListAsync();
                if (departments.Count == 0)
                    return NotFound("Không tìm thấy đơn vị nào để xóa mềm.");
                foreach (var dept in departments)
                {
                    dept.DeletedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                return Ok(new { softDeleted = departments.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa mềm nhiều đơn vị: {ex.Message}");
            }
        }

        // PERMANENT DELETE: api/Departments/permanent-delete/5
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound();
                }
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa vĩnh viễn đơn vị");
            }
        }

        // BULK PERMANENT DELETE: api/Departments/bulk-permanent-delete
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var departments = await _context.Departments.Where(d => ids.Contains(d.Id)).ToListAsync();
                if (departments.Count == 0)
                    return NotFound("Không tìm thấy đơn vị nào để xóa vĩnh viễn.");
                _context.Departments.RemoveRange(departments);
                await _context.SaveChangesAsync();
                return Ok(new { permanentlyDeleted = departments.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi xóa vĩnh viễn nhiều đơn vị: {ex.Message}");
            }
        }

        // BULK RESTORE: api/Departments/bulk-restore
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách id không hợp lệ.");
            try
            {
                var departments = await _context.Departments.Where(d => ids.Contains(d.Id) && d.DeletedAt != null).ToListAsync();
                if (departments.Count == 0)
                    return NotFound("Không tìm thấy đơn vị nào để khôi phục.");
                foreach (var dept in departments)
                {
                    dept.DeletedAt = null;
                }
                await _context.SaveChangesAsync();
                return Ok(new { restored = departments.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi khi khôi phục nhiều đơn vị: {ex.Message}");
            }
        }

        // GET: api/Departments/list
        [HttpGet("list")]
        public async Task<ActionResult<object>> GetDepartmentList([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                var query = _context.Departments
                    .Where(d => d.DeletedAt == null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var departments = await query
                    .OrderBy(d => d.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = departments,
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

        // GET: api/Departments/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedDepartments([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            try
            {
                var query = _context.Departments
                    .Where(d => d.DeletedAt != null);

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var departments = await query
                    .OrderBy(d => d.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Return paginated response format
                return Ok(new
                {
                    data = departments,
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

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}