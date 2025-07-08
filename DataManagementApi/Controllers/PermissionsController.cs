using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            try
            {
                return await _context.Permissions
                    .Include(p => p.RolePermissions)
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu");
            }
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.RolePermissions)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (permission == null)
                {
                    return NotFound();
                }

                return permission;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu");
            }
        }

        // PUT: api/Permissions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, UpdatePermissionRequest request)
        {
            try
            {
                var existingPermission = await _context.Permissions.FindAsync(id);
                if (existingPermission == null)
                {
                    return NotFound();
                }

                // Cập nhật các thuộc tính
                existingPermission.Name = request.Name;
                existingPermission.Description = request.Description;
                existingPermission.Module = request.Module;

                _context.Entry(existingPermission).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionExists(id))
                {
                    return NotFound();
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
        }

        // POST: api/Permissions
        [HttpPost]
        public async Task<ActionResult<Permission>> PostPermission(CreatePermissionRequest request)
        {
            try
            {
                var permission = new Permission
                {
                    Name = request.Name,
                    Description = request.Description,
                    Module = request.Module
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permission);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới permission");
            }
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem permission có đang được sử dụng bởi role nào không
                if (await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == id))
                {
                    return BadRequest("Không thể xóa permission này vì nó đang được sử dụng bởi một số role.");
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa permission");
            }
        }

        // GET: api/Permissions/by-module/{module}
        [HttpGet("by-module/{module}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByModule(string module)
        {
            try
            {
                return await _context.Permissions
                    .Where(p => p.Module == module)
                    .Include(p => p.RolePermissions)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu");
            }
        }

        // GET: api/Permissions/modules
        [HttpGet("modules")]
        public async Task<ActionResult<IEnumerable<string>>> GetModules()
        {
            try
            {
                return await _context.Permissions
                    .Select(p => p.Module)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu");
            }
        }

        private bool PermissionExists(int id)
        {
            return _context.Permissions.Any(e => e.Id == id);
        }
    }
} 