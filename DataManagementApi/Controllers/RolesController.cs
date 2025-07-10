using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<object>> GetRoles(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 10, 
            [FromQuery] string search = "")
        {
            var query = _context.Roles
                .Where(r => r.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.Contains(search) || (r.Description != null && r.Description.Contains(search)));
            }
            
            var totalCount = await query.CountAsync();

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return Ok(new 
            {
                data = roles,
                total = totalCount,
                page,
                limit
            });
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            return await _context.Roles
                .Where(r => r.DeletedAt == null)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id && r.DeletedAt == null)
                .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(r => r.RoleMenus).ThenInclude(rm => rm.Menu)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            // Ensure DeletedAt is not set on creation
            role.DeletedAt = null;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }
        
        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, Role role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }
            
            var existingRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingRole == null || existingRole.DeletedAt != null)
            {
                return NotFound("Vai trò không tồn tại hoặc đã bị xóa.");
            }
            
            // Preserve the original DeletedAt value
            role.DeletedAt = existingRole.DeletedAt;

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        
        // SOFT DELETE: api/roles/soft-delete/5
        [HttpPost("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            if (role.DeletedAt != null) return BadRequest("Vai trò đã được xóa.");

            role.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        // GET: api/roles/deleted
        [HttpGet("deleted")]
        public async Task<ActionResult<object>> GetDeletedRoles([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
        {
            var query = _context.Roles
                .Where(r => r.DeletedAt != null)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.Contains(search) || (r.Description != null && r.Description.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var roles = await query
                .OrderByDescending(r => r.DeletedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
            
            return Ok(new { data = roles, total = totalCount, page, limit });
        }
        
        // BULK SOFT DELETE: api/roles/bulk-soft-delete
        [HttpPost("bulk-soft-delete")]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");

            var roles = await _context.Roles.Where(r => ids.Contains(r.Id) && r.DeletedAt == null).ToListAsync();
            if (roles.Count == 0) return NotFound("Không tìm thấy vai trò hợp lệ để xóa.");

            foreach (var role in roles)
            {
                role.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã xóa thành công {roles.Count} vai trò."});
        }
        
        // BULK RESTORE: api/roles/bulk-restore
        [HttpPost("bulk-restore")]
        public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");
            
            var roles = await _context.Roles.Where(r => ids.Contains(r.Id) && r.DeletedAt != null).ToListAsync();
            if (roles.Count == 0) return NotFound("Không tìm thấy vai trò hợp lệ để khôi phục.");
            
            foreach (var role in roles)
            {
                role.DeletedAt = null;
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã khôi phục thành công {roles.Count} vai trò."});
        }

        // BULK PERMANENT DELETE: api/roles/bulk-permanent-delete
        [HttpPost("bulk-permanent-delete")]
        public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Danh sách ID không hợp lệ.");

            var roles = await _context.Roles
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            if (roles.Count == 0) return NotFound("Không tìm thấy vai trò hợp lệ để xóa vĩnh viễn.");

            _context.Roles.RemoveRange(roles);
            
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã xóa vĩnh viễn {roles.Count} vai trò." });
        }

        // Replaces the old DELETE endpoint
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id && e.DeletedAt == null);
        }
    }
} 