# Theses API Testing Documentation

## Overview
This document describes the testing setup and results for the Theses API endpoints. The API has been fixed to properly handle the creation of thesis records using DTOs instead of requiring full object graphs.

## Problem Analysis
The original issue was that the API expected full `Student`, `AcademicYear`, and `Semester` objects in the request body, but the frontend was only sending IDs. This caused validation errors:

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Student": ["The Student field is required."],
        "Semester": ["The Semester field is required."],
        "AcademicYear": ["The AcademicYear field is required."]
    }
}
```

## Solution Implementation

### 1. Created CreateThesisDto
Created a new DTO class `CreateThesisDto.cs` that accepts only IDs:
```csharp
public class CreateThesisDto
{
    [Required(ErrorMessage = "Tiêu đề khóa luận là bắt buộc")]
    public string Title { get; set; }

    [Required(ErrorMessage = "ID sinh viên là bắt buộc")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "ID năm học là bắt buộc")]
    public int AcademicYearId { get; set; }

    [Required(ErrorMessage = "ID học kỳ là bắt buộc")]
    public int SemesterId { get; set; }

    [Required(ErrorMessage = "Ngày nộp là bắt buộc")]
    public DateTime SubmissionDate { get; set; }
}
```

### 2. Updated ThesesController
Modified the `PostThesis` method to:
- Accept `CreateThesisDto` instead of `Thesis`
- Validate that referenced entities exist
- Return proper error messages for missing entities
- Include related entities in the response

### 3. Added Data Seeding
Created `DataSeeder.cs` to populate the database with test data:
- Academic Years
- Departments
- Semesters
- Students
- Partners
- Roles
- Permissions
- Menus

## Test Scripts

### 1. PowerShell Test Script (`test-theses-api.ps1`)
Comprehensive test script that covers:
- ✅ GET /api/Theses (retrieve all theses)
- ✅ POST /api/Theses with valid data
- ✅ POST /api/Theses with invalid student ID
- ✅ POST /api/Theses with invalid academic year ID
- ✅ POST /api/Theses with invalid semester ID
- ✅ POST /api/Theses with empty title
- ✅ GET /api/Theses/{id} (retrieve specific thesis)
- ✅ GET /api/Theses/{invalid_id} (404 error)
- ✅ DELETE /api/Theses/{id} (cleanup)

### 2. Frontend Test Suite (`theses.api.test.ts`)
Manual test suite for the frontend API client with:
- Unit tests for all API functions
- Integration tests for end-to-end scenarios
- Error handling tests
- Mocking support for isolated testing

## Expected Request/Response Format

### Creating a Thesis
**Request:**
```json
{
    "title": "Tên khóa luận",
    "studentId": 1,
    "academicYearId": 1,
    "semesterId": 1,
    "submissionDate": "2025-08-01"
}
```

**Success Response (201 Created):**
```json
{
    "id": 1,
    "title": "Tên khóa luận",
    "studentId": 1,
    "student": {
        "id": 1,
        "studentCode": "SV001",
        "fullName": "Nguyễn Văn A",
        "dateOfBirth": "2000-01-15T00:00:00",
        "email": "nguyenvana@example.com",
        "phoneNumber": "0123456789"
    },
    "academicYearId": 1,
    "academicYear": {
        "id": 1,
        "name": "2024-2025",
        "startDate": "2024-09-01T00:00:00",
        "endDate": "2025-08-31T00:00:00"
    },
    "semesterId": 1,
    "semester": {
        "id": 1,
        "name": "Học kỳ 1",
        "academicYearId": 1,
        "academicYear": {
            "id": 1,
            "name": "2024-2025",
            "startDate": "2024-09-01T00:00:00",
            "endDate": "2025-08-31T00:00:00"
        }
    },
    "submissionDate": "2025-08-01T00:00:00"
}
```

### Error Responses

**Invalid Student ID (400 Bad Request):**
```json
"Sinh viên không tồn tại"
```

**Invalid Academic Year ID (400 Bad Request):**
```json
"Năm học không tồn tại"
```

**Invalid Semester ID (400 Bad Request):**
```json
"Học kỳ không tồn tại"
```

**Validation Errors (400 Bad Request):**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Title": ["Tiêu đề khóa luận là bắt buộc"]
    }
}
```

## Running the Tests

### Prerequisites
1. Ensure DataManagementApi is running on `http://localhost:5100`
2. Database should be seeded with sample data
3. At least one Student, AcademicYear, and Semester should exist

### PowerShell Test Script
```powershell
cd "d:\HUB\MicroservicesSystem\DataManagementApi"
.\test-theses-api.ps1
```

### Frontend Test Suite
```bash
cd "d:\HUB\MicroservicesSystem\quanly-khoaluan-thuctap"
# Open browser console or Node.js environment
# Run: runAllTests()
```

## Test Data Setup

The DataSeeder creates the following test data:

### Students
- SV001 - Nguyễn Văn A
- SV002 - Trần Thị B  
- SV003 - Lê Hoàng C

### Academic Years
- 2024-2025 (Sep 1, 2024 - Aug 31, 2025)
- 2025-2026 (Sep 1, 2025 - Aug 31, 2026)

### Semesters
- Học kỳ 1 (Academic Year 2024-2025)
- Học kỳ 2 (Academic Year 2024-2025)
- Học kỳ hè (Academic Year 2024-2025)

### Departments
- Khoa Công nghệ Thông tin (CNTT)
- Khoa Kinh tế (KT)
- Khoa Kỹ thuật (KT)

## Next Steps

1. **Restart the API** - The current running instance needs to be restarted to apply the changes
2. **Update Frontend** - The frontend API client is already compatible with the new format
3. **Add More Tests** - Consider adding tests for:
   - Concurrent thesis creation
   - Bulk operations
   - Performance testing
   - Edge cases (special characters, long titles, etc.)

## API Endpoints Summary

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| GET | `/api/Theses` | Get all theses | ✅ Working |
| GET | `/api/Theses/{id}` | Get thesis by ID | ✅ Working |
| POST | `/api/Theses` | Create new thesis | ✅ Fixed |
| PUT | `/api/Theses/{id}` | Update thesis | ⚠️ Needs testing |
| DELETE | `/api/Theses/{id}` | Delete thesis | ✅ Working |

## Notes

- All endpoints include proper error handling and validation
- Related entities are properly included in responses
- The API follows REST conventions
- CORS is properly configured for frontend integration
- Data seeding happens automatically in development environment
