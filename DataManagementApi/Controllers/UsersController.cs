using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    KeycloakUserId = u.KeycloakUserId,
                    Name = u.Name,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    UserRoles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                }).ToList();

                return userDtos;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    KeycloakUserId = user.KeycloakUserId,
                    Name = user.Name,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                };

                return userDto;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }
        
        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(CreateUserRequest request)
        {
            try
            {
                // Check model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new { 
                            Field = x.Key, 
                            Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>()
                        })
                        .ToList();
                    
                    return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
                }

                // Check if email already exists
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
                
                if (existingUserByEmail != null)
                {
                    return Conflict(new { message = "Email đã tồn tại trong hệ thống" });
                }

                // Check if Keycloak User ID already exists
                var existingUserByKeycloakId = await _context.Users
                    .FirstOrDefaultAsync(u => u.KeycloakUserId == request.KeycloakUserId);
                
                if (existingUserByKeycloakId != null)
                {
                    return Conflict(new { message = "Keycloak User ID đã tồn tại trong hệ thống" });
                }

                // Validate role IDs exist
                if (request.RoleIds.Any())
                {
                    var existingRoles = await _context.Roles
                        .Where(r => request.RoleIds.Contains(r.Id))
                        .CountAsync();
                    
                    if (existingRoles != request.RoleIds.Count)
                    {
                        return BadRequest(new { message = "Một hoặc nhiều vai trò được chỉ định không tồn tại" });
                    }
                }

                var user = new User
                {
                    KeycloakUserId = request.KeycloakUserId,
                    Name = request.Name,
                    Email = request.Email,
                    AvatarUrl = request.AvatarUrl,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add roles if provided
                if (request.RoleIds.Any())
                {
                    var userRoles = request.RoleIds.Select(roleId => new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    }).ToList();

                    _context.UserRoles.AddRange(userRoles);
                    await _context.SaveChangesAsync();
                }

                // Fetch the created user with roles
                var createdUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (createdUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi tạo mới người dùng");
                }

                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    KeycloakUserId = createdUser.KeycloakUserId,
                    Name = createdUser.Name,
                    Email = createdUser.Email,
                    AvatarUrl = createdUser.AvatarUrl,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt,
                    UpdatedAt = createdUser.UpdatedAt,
                    UserRoles = createdUser.UserRoles.Select(ur => ur.Role.Name).ToList()
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Lỗi khi tạo mới người dùng", details = ex.Message });
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserRequest request)
        {
            try
            {
                // Check model validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new { 
                            Field = x.Key, 
                            Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>()
                        })
                        .ToList();
                    
                    return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
                }

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                // Validate email if being updated
                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
                {
                    var existingUserWithEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != id);
                    
                    if (existingUserWithEmail != null)
                    {
                        return Conflict(new { message = "Email đã được sử dụng bởi người dùng khác" });
                    }
                }

                // Validate role IDs exist if being updated
                if (request.RoleIds != null && request.RoleIds.Any())
                {
                    var existingRoles = await _context.Roles
                        .Where(r => request.RoleIds.Contains(r.Id))
                        .CountAsync();
                    
                    if (existingRoles != request.RoleIds.Count)
                    {
                        return BadRequest(new { message = "Một hoặc nhiều vai trò được chỉ định không tồn tại" });
                    }
                }

                // Update user properties
                if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
                if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
                if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
                user.UpdatedAt = DateTime.UtcNow;

                // Update roles if provided
                if (request.RoleIds != null)
                {
                    // Remove existing roles
                    _context.UserRoles.RemoveRange(user.UserRoles);
                    
                    // Add new roles
                    if (request.RoleIds.Any())
                    {
                        var newUserRoles = request.RoleIds.Select(roleId => new UserRole
                        {
                            UserId = id,
                            RoleId = roleId
                        }).ToList();

                        _context.UserRoles.AddRange(newUserRoles);
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Lỗi cập nhật dữ liệu", details = ex.Message });
            }
        }


        // DELETE: api/Users/5
        // Endpoint này chỉ xóa user khỏi DB local, không xóa khỏi Keycloak.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == id);
                
                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                // Remove associated user roles first
                if (user.UserRoles.Any())
                {
                    _context.UserRoles.RemoveRange(user.UserRoles);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Lỗi khi xóa người dùng", details = ex.Message });
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
} 