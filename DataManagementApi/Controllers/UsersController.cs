using DataManagementApi.Data;
using DataManagementApi.Models;
using DataManagementApi.Models.Dto;
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
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                return await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                                               .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                return user;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi truy xuất dữ liệu từ cơ sở dữ liệu");
            }
        }
        
        // POST: api/Users
        // Lưu ý: Việc tạo user thường được kích hoạt "Just-in-Time" sau khi user đăng nhập lần đầu tiên qua Keycloak.
        // Endpoint này dùng để tạo thủ công bản ghi user trong DB local, giả định KeycloakUserId đã tồn tại.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null || string.IsNullOrWhiteSpace(createUserDto.KeycloakUserId))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await _context.Users.AnyAsync(u => u.KeycloakUserId == createUserDto.KeycloakUserId))
                {
                    return Conflict("Người dùng với KeycloakUserId này đã tồn tại.");
                }

                var user = new User
                {
                    KeycloakUserId = createUserDto.KeycloakUserId,
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    IsActive = createUserDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (createUserDto.RoleIds != null && createUserDto.RoleIds.Any())
                {
                    foreach (var roleId in createUserDto.RoleIds)
                    {
                        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
                        if (roleExists)
                        {
                            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            return BadRequest($"Vai trò với ID {roleId} không tồn tại.");
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                var createdUser = await _context.Users
                                            .Include(u => u.UserRoles)
                                            .ThenInclude(ur => ur.Role)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(u => u.Id == user.Id);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, "Đã xảy ra lỗi khi tạo người dùng.");
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            
            user.UpdatedAt = DateTime.UtcNow;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

            return NoContent();
        }


        // DELETE: api/Users/5
        // Endpoint này chỉ xóa user khỏi DB local, không xóa khỏi Keycloak.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi xóa người dùng");
            }
        }

        // POST: api/Users/{userId}/roles
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRoleToUser(int userId, [FromBody] UserRoleDto userRoleDto)
        {
            if (userRoleDto == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"Không tìm thấy người dùng với ID {userId}.");
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.Id == userRoleDto.RoleId);
            if (!roleExists)
            {
                return NotFound($"Không tìm thấy vai trò với ID {userRoleDto.RoleId}.");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = userRoleDto.RoleId
            };

            var alreadyExists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == userRoleDto.RoleId);

            if (alreadyExists)
            {
                return Conflict("Người dùng đã có vai trò này.");
            }

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gán vai trò thành công." });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
} 