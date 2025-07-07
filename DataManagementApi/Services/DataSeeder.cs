using DataManagementApi.Data;
using DataManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DataManagementApi.Services
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;

        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Seed Academic Years
            if (!await _context.AcademicYears.AnyAsync())
            {
                var academicYears = new[]
                {
                    new AcademicYear { Name = "2024-2025", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 8, 31) },
                    new AcademicYear { Name = "2025-2026", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 8, 31) }
                };

                await _context.AcademicYears.AddRangeAsync(academicYears);
                await _context.SaveChangesAsync();
            }

            // Seed Departments
            if (!await _context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Department { Name = "Khoa Công nghệ Thông tin", Code = "CNTT" },
                    new Department { Name = "Khoa Kinh tế", Code = "KT" },
                    new Department { Name = "Khoa Kỹ thuật", Code = "KT" }
                };

                await _context.Departments.AddRangeAsync(departments);
                await _context.SaveChangesAsync();
            }

            // Seed Semesters
            if (!await _context.Semesters.AnyAsync())
            {
                var firstAcademicYear = await _context.AcademicYears.FirstAsync();
                var semesters = new[]
                {
                    new Semester { Name = "Học kỳ 1", AcademicYearId = firstAcademicYear.Id },
                    new Semester { Name = "Học kỳ 2", AcademicYearId = firstAcademicYear.Id },
                    new Semester { Name = "Học kỳ hè", AcademicYearId = firstAcademicYear.Id }
                };

                await _context.Semesters.AddRangeAsync(semesters);
                await _context.SaveChangesAsync();
            }

            // Seed Students
            if (!await _context.Students.AnyAsync())
            {
                var students = new[]
                {
                    new Student 
                    { 
                        StudentCode = "SV001", 
                        FullName = "Nguyễn Văn A", 
                        DateOfBirth = new DateTime(2000, 1, 15),
                        Email = "nguyenvana@example.com",
                        PhoneNumber = "0123456789"
                    },
                    new Student 
                    { 
                        StudentCode = "SV002", 
                        FullName = "Trần Thị B", 
                        DateOfBirth = new DateTime(2000, 5, 20),
                        Email = "tranthib@example.com",
                        PhoneNumber = "0987654321"
                    },
                    new Student 
                    { 
                        StudentCode = "SV003", 
                        FullName = "Lê Hoàng C", 
                        DateOfBirth = new DateTime(2000, 8, 10),
                        Email = "lehoangc@example.com",
                        PhoneNumber = "0345678901"
                    }
                };

                await _context.Students.AddRangeAsync(students);
                await _context.SaveChangesAsync();
            }

            // Seed Partners
            if (!await _context.Partners.AnyAsync())
            {
                var partners = new[]
                {
                    new Partner
                    {
                        Name = "Công ty ABC",
                        Address = "123 Đường ABC, Quận 1, TP.HCM",
                        PhoneNumber = "0123456789",
                        Email = "contact@abc.com"
                    },
                    new Partner
                    {
                        Name = "Công ty XYZ",
                        Address = "456 Đường XYZ, Quận 2, TP.HCM",
                        PhoneNumber = "0987654321",
                        Email = "info@xyz.com"
                    }
                };

                await _context.Partners.AddRangeAsync(partners);
                await _context.SaveChangesAsync();
            }

            // Seed Roles
            if (!await _context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { Name = "Admin" },
                    new Role { Name = "Teacher" },
                    new Role { Name = "Student" }
                };

                await _context.Roles.AddRangeAsync(roles);
                await _context.SaveChangesAsync();
            }

            // Seed Permissions
            if (!await _context.Permissions.AnyAsync())
            {
                var permissions = new[]
                {
                    new Permission { Name = "CREATE_THESIS", Module = "Thesis" },
                    new Permission { Name = "READ_THESIS", Module = "Thesis" },
                    new Permission { Name = "UPDATE_THESIS", Module = "Thesis" },
                    new Permission { Name = "DELETE_THESIS", Module = "Thesis" },
                    new Permission { Name = "MANAGE_STUDENTS", Module = "Student" },
                    new Permission { Name = "MANAGE_PARTNERS", Module = "Partner" }
                };

                await _context.Permissions.AddRangeAsync(permissions);
                await _context.SaveChangesAsync();
            }

            // Seed Menus
            if (!await _context.Menus.AnyAsync())
            {
                var menus = new[]
                {
                    new Menu { Name = "Dashboard", Path = "/dashboard", Icon = "dashboard" },
                    new Menu { Name = "Theses", Path = "/theses", Icon = "book" },
                    new Menu { Name = "Students", Path = "/students", Icon = "users" },
                    new Menu { Name = "Partners", Path = "/partners", Icon = "building" },
                    new Menu { Name = "Reports", Path = "/reports", Icon = "chart" }
                };

                await _context.Menus.AddRangeAsync(menus);
                await _context.SaveChangesAsync();
            }
        }
    }
}
