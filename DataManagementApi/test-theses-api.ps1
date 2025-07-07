# PowerShell script for testing DataManagementApi Theses endpoints

Write-Host "=== Testing Theses API Endpoints ===" -ForegroundColor Yellow

# Base URL
$baseUrl = "http://localhost:5100/api"

# Test 1: Get all theses
Write-Host "`nTest 1: GET /api/Theses" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method GET -ContentType "application/json"
    Write-Host "Success: Retrieved theses list" -ForegroundColor Green
    Write-Host "Count: $($response.Count)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

# Test 2: Create a new thesis with valid data
Write-Host "`nTest 2: POST /api/Theses (Valid Data)" -ForegroundColor Cyan
$validThesis = @{
    title = "Test Thesis - PowerShell API Test"
    studentId = 1
    academicYearId = 1
    semesterId = 1
    submissionDate = "2025-08-01"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method POST -Body ($validThesis | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Success: Created thesis" -ForegroundColor Green
    Write-Host "ID: $($response.id), Title: $($response.title)" -ForegroundColor White
    $createdThesisId = $response.id
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}

# Test 3: Create a new thesis with invalid student ID
Write-Host "`nTest 3: POST /api/Theses (Invalid Student ID)" -ForegroundColor Cyan
$invalidStudentThesis = @{
    title = "Test Thesis - Invalid Student"
    studentId = 999999
    academicYearId = 1
    semesterId = 1
    submissionDate = "2025-08-01"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method POST -Body ($invalidStudentThesis | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor White
    }
}

# Test 4: Create a new thesis with invalid academic year ID
Write-Host "`nTest 4: POST /api/Theses (Invalid Academic Year ID)" -ForegroundColor Cyan
$invalidAcademicYearThesis = @{
    title = "Test Thesis - Invalid Academic Year"
    studentId = 1
    academicYearId = 999999
    semesterId = 1
    submissionDate = "2025-08-01"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method POST -Body ($invalidAcademicYearThesis | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor White
    }
}

# Test 5: Create a new thesis with invalid semester ID
Write-Host "`nTest 5: POST /api/Theses (Invalid Semester ID)" -ForegroundColor Cyan
$invalidSemesterThesis = @{
    title = "Test Thesis - Invalid Semester"
    studentId = 1
    academicYearId = 1
    semesterId = 999999
    submissionDate = "2025-08-01"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method POST -Body ($invalidSemesterThesis | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor White
    }
}

# Test 6: Create a new thesis with missing title
Write-Host "`nTest 6: POST /api/Theses (Missing Title)" -ForegroundColor Cyan
$missingTitleThesis = @{
    title = ""
    studentId = 1
    academicYearId = 1
    semesterId = 1
    submissionDate = "2025-08-01"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses" -Method POST -Body ($missingTitleThesis | ConvertTo-Json) -ContentType "application/json"
    Write-Host "Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Green
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor White
    }
}

# Test 7: Get a specific thesis (using created thesis ID if available)
if ($createdThesisId) {
    Write-Host "`nTest 7: GET /api/Theses/$createdThesisId" -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/Theses/$createdThesisId" -Method GET -ContentType "application/json"
        Write-Host "Success: Retrieved thesis" -ForegroundColor Green
        Write-Host "ID: $($response.id), Title: $($response.title)" -ForegroundColor White
        Write-Host "Student: $($response.student.fullName)" -ForegroundColor White
        Write-Host "Academic Year: $($response.academicYear.name)" -ForegroundColor White
        Write-Host "Semester: $($response.semester.name)" -ForegroundColor White
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 8: Get a non-existent thesis
Write-Host "`nTest 8: GET /api/Theses/999999" -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/Theses/999999" -Method GET -ContentType "application/json"
    Write-Host "Unexpected success" -ForegroundColor Red
} catch {
    Write-Host "Expected error: $($_.Exception.Message)" -ForegroundColor Green
}

# Test 9: Delete the created thesis (cleanup)
if ($createdThesisId) {
    Write-Host "`nTest 9: DELETE /api/Theses/$createdThesisId (Cleanup)" -ForegroundColor Cyan
    try {
        Invoke-RestMethod -Uri "$baseUrl/Theses/$createdThesisId" -Method DELETE -ContentType "application/json"
        Write-Host "Success: Deleted thesis" -ForegroundColor Green
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nAPI tests completed!" -ForegroundColor Green
