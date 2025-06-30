using DataManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Thesis> Theses { get; set; }
        public DbSet<Internship> Internships { get; set; }
        
        // --- Models cho User, Role, Permission ---
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        // -----------------------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ cha-con cho Department
            modelBuilder.Entity<Department>()
                .HasOne(d => d.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Cấu hình cho User, Role, Permission, Menu ---

            // User: Thiết lập unique index cho KeycloakUserId và Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.KeycloakUserId)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Menu: Cấu hình quan hệ cha-con
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.ChildMenus)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRole: Khóa chính zusammengesetzt
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // RolePermission: Khóa chính zusammengesetzt
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // RoleMenu: Khóa chính zusammengesetzt
            modelBuilder.Entity<RoleMenu>()
                .HasKey(rm => new { rm.RoleId, rm.MenuId });
            modelBuilder.Entity<RoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId);
            modelBuilder.Entity<RoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);

            // --- Cấu hình cho Internship và Thesis để tránh lỗi Multiple Cascade Paths ---
            modelBuilder.Entity<Internship>()
                .HasOne(i => i.AcademicYear)
                .WithMany() // Không có collection tương ứng trong AcademicYear
                .HasForeignKey(i => i.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Internship>()
                .HasOne(i => i.Semester)
                .WithMany() // Không có collection tương ứng trong Semester
                .HasForeignKey(i => i.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Thesis>()
                .HasOne(t => t.AcademicYear)
                .WithMany() // Không có collection tương ứng trong AcademicYear
                .HasForeignKey(t => t.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Thesis>()
                .HasOne(t => t.Semester)
                .WithMany() // Không có collection tương ứng trong Semester
                .HasForeignKey(t => t.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 