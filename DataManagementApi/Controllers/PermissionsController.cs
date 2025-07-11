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

		// GET: api/permissions
		[HttpGet]
		public async Task<ActionResult<object>> GetPermissions([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
		{
			try
			{
				var query = _context.Permissions
									.Where(p => p.DeletedAt == null)
									.Include(p => p.RolePermissions!)
									.ThenInclude(rp => rp.Role)
									.AsQueryable();

				if (!string.IsNullOrEmpty(search))
				{
					query = query.Where(p => p.Name.Contains(search) || p.Module.Contains(search));
				}
				
				var total = await query.CountAsync();
				var permissions = await query
					.Skip((page - 1) * limit)
					.Take(limit)
					.ToListAsync();

				return Ok(new {
					data = permissions,
					total,
					page,
					limit
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// GET: api/permissions/deleted
		[HttpGet("deleted")]
		public async Task<ActionResult<object>> GetDeletedPermissions([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "")
		{
			try
			{
				var query = _context.Permissions.Where(p => p.DeletedAt != null).AsQueryable();

				if (!string.IsNullOrEmpty(search))
				{
					query = query.Where(p => p.Name.Contains(search) || p.Module.Contains(search));
				}

				var total = await query.CountAsync();
				var permissions = await query.OrderByDescending(p => p.DeletedAt)
					.Skip((page - 1) * limit)
					.Take(limit)
					.ToListAsync();
				
				return Ok(new {
					data = permissions,
					total,
					page,
					limit
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// GET: api/permissions/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAllPermissions()
        {
            return await _context.Permissions.Where(p => p.DeletedAt == null).OrderBy(p => p.Module).ThenBy(p => p.Name).ToListAsync();
        }

		// GET: api/permissions/modules
		[HttpGet("modules")]
		public async Task<ActionResult<IEnumerable<string>>> GetPermissionModules()
		{
			try
			{
				var modules = await _context.Permissions.Where(p => p.DeletedAt == null).Select(p => p.Module).Distinct().ToListAsync();
				return Ok(modules);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// GET: api/permissions/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Permission>> GetPermission(int id)
		{
			try
			{
				var permission = await _context.Permissions.Include(p => p.RolePermissions!).ThenInclude(rp => rp.Role).FirstOrDefaultAsync(p => p.Id == id);

				if (permission == null || permission.DeletedAt != null)
				{
					return NotFound();
				}

				return permission;
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// PUT: api/permissions/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutPermission(int id, UpdatePermissionData permissionData)
		{
			if (id != permissionData.Id)
			{
				return BadRequest();
			}

			try
			{
				var permission = await _context.Permissions.FindAsync(id);
				if (permission == null)
				{
					return NotFound();
				}

				permission.Name = permissionData.Name;
				permission.Description = permissionData.Description;
				permission.Module = permissionData.Module;

				_context.Entry(permission).State = EntityState.Modified;

				await _context.SaveChangesAsync();
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
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}

			return NoContent();
		}

		// POST: api/permissions
		[HttpPost]
		public async Task<ActionResult<Permission>> PostPermission(CreatePermissionData permissionData)
		{
			try
			{
				var permission = new Permission
				{
					Name = permissionData.Name,
					Description = permissionData.Description,
					Module = permissionData.Module
				};

				_context.Permissions.Add(permission);
				await _context.SaveChangesAsync();

				return CreatedAtAction("GetPermission", new { id = permission.Id }, permission);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// POST: api/permissions/soft-delete/5
		[HttpPost("soft-delete/{id}")]
		public async Task<IActionResult> SoftDeletePermission(int id)
		{
			try
			{
				var permission = await _context.Permissions.FindAsync(id);
				if (permission == null)
				{
					return NotFound();
				}

				permission.DeletedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// POST: api/permissions/restore/5
		[HttpPost("restore/{id}")]
		public async Task<IActionResult> RestorePermission(int id)
		{
			try
			{
				var permission = await _context.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
				if (permission == null)
				{
					return NotFound();
				}

				permission.DeletedAt = null;
				await _context.SaveChangesAsync();

				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
		
		// DELETE: api/permissions/permanent-delete/5
        [HttpDelete("permanent-delete/{id}")]
        public async Task<IActionResult> PermanentDelete(int id)
        {
			try
			{
				var permission = await _context.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
				if (permission == null)
				{
					return NotFound();
				}

				_context.Permissions.Remove(permission);
				await _context.SaveChangesAsync();
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
        }

		// POST: api/permissions/bulk-soft-delete
		[HttpPost("bulk-soft-delete")]
		public async Task<IActionResult> BulkSoftDelete([FromBody] IEnumerable<int> ids)
		{
			try
			{
				var permissions = await _context.Permissions.Where(p => ids.Contains(p.Id)).ToListAsync();
				foreach (var permission in permissions)
				{
					permission.DeletedAt = DateTime.UtcNow;
				}
				await _context.SaveChangesAsync();
				return Ok(new { softDeleted = permissions.Count });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// POST: api/permissions/bulk-restore
		[HttpPost("bulk-restore")]
		public async Task<IActionResult> BulkRestore([FromBody] IEnumerable<int> ids)
		{
			try
			{
				var permissions = await _context.Permissions.IgnoreQueryFilters().Where(p => ids.Contains(p.Id)).ToListAsync();
				foreach (var permission in permissions)
				{
					permission.DeletedAt = null;
				}
				await _context.SaveChangesAsync();
				return Ok(new { restored = permissions.Count });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		// POST: api/permissions/bulk-permanent-delete
		[HttpPost("bulk-permanent-delete")]
		public async Task<IActionResult> BulkPermanentDelete([FromBody] IEnumerable<int> ids)
		{
			try
			{
				var permissions = await _context.Permissions.IgnoreQueryFilters().Where(p => ids.Contains(p.Id)).ToListAsync();
				_context.Permissions.RemoveRange(permissions);
				await _context.SaveChangesAsync();
				return Ok(new { permanentlyDeleted = permissions.Count });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		private bool PermissionExists(int id)
		{
			return _context.Permissions.Any(e => e.Id == id && e.DeletedAt == null);
		}
	}
} 