# Test script for current API format (full objects required)

Write-Host "Testing current API with full object format..." -ForegroundColor Yellow

# Get available data
Write-Host "Getting available data..." -ForegroundColor Cyan

try {
    $students = Invoke-RestMethod -Uri "http://localhost:5100/api/Students" -Method GET
    $academicYears = Invoke-RestMethod -Uri "http://localhost:5100/api/AcademicYears" -Method GET  
    $semesters = Invoke-RestMethod -Uri "http://localhost:5100/api/Semesters" -Method GET

    Write-Host "Available Students:" -ForegroundColor Green
    $students | ForEach-Object { Write-Host "  ID: $($_.id), Code: $($_.studentCode), Name: $($_.fullName)" }

    Write-Host "Available Academic Years:" -ForegroundColor Green  
    $academicYears | ForEach-Object { Write-Host "  ID: $($_.id), Name: $($_.name)" }

    Write-Host "Available Semesters:" -ForegroundColor Green
    $semesters | ForEach-Object { Write-Host "  ID: $($_.id), Name: $($_.name), Academic Year ID: $($_.academicYearId)" }

    # Select first available entities
    $student = $students | Select-Object -First 1
    $academicYear = $academicYears | Select-Object -First 1  
    $semester = $semesters | Select-Object -First 1

    Write-Host "`nUsing:" -ForegroundColor Yellow
    Write-Host "Student: $($student.fullName) (ID: $($student.id))"
    Write-Host "Academic Year: $($academicYear.name) (ID: $($academicYear.id))"
    Write-Host "Semester: $($semester.name) (ID: $($semester.id))"

    # Test 1: Try with just IDs (current user request format)
    Write-Host "`nTest 1: Creating thesis with IDs only (user's format)..." -ForegroundColor Cyan
    $thesisDataIds = @{
        title = "Test Thesis - IDs Only"
        studentId = $student.id
        academicYearId = $academicYear.id
        semesterId = $semester.id
        submissionDate = "2025-08-01"
    }

    try {
        $response1 = Invoke-RestMethod -Uri "http://localhost:5100/api/Theses" -Method POST -Body ($thesisDataIds | ConvertTo-Json) -ContentType "application/json"
        Write-Host "SUCCESS: Thesis created with IDs only!" -ForegroundColor Green
        Write-Host "Response: $($response1 | ConvertTo-Json)" -ForegroundColor White
    } catch {
        Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Error Response: $responseBody" -ForegroundColor Red
        }
    }

    # Test 2: Try with full objects (workaround)
    Write-Host "`nTest 2: Creating thesis with full objects (workaround)..." -ForegroundColor Cyan
    $thesisDataFull = @{
        title = "Test Thesis - Full Objects"
        studentId = $student.id
        student = $student
        academicYearId = $academicYear.id
        academicYear = $academicYear
        semesterId = $semester.id
        semester = $semester
        submissionDate = "2025-08-01"
    }

    try {
        $response2 = Invoke-RestMethod -Uri "http://localhost:5100/api/Theses" -Method POST -Body ($thesisDataFull | ConvertTo-Json -Depth 10) -ContentType "application/json"
        Write-Host "SUCCESS: Thesis created with full objects!" -ForegroundColor Green
        Write-Host "Response: $($response2 | ConvertTo-Json)" -ForegroundColor White
        
        # Store the created thesis ID for cleanup
        $createdThesisId = $response2.id
        
        # Test getting the created thesis
        Write-Host "`nTest 3: Getting created thesis..." -ForegroundColor Cyan
        $getResponse = Invoke-RestMethod -Uri "http://localhost:5100/api/Theses/$createdThesisId" -Method GET
        Write-Host "SUCCESS: Retrieved thesis!" -ForegroundColor Green
        Write-Host "Title: $($getResponse.title)" -ForegroundColor White
        
        # Cleanup - delete the created thesis
        Write-Host "`nCleaning up - deleting created thesis..." -ForegroundColor Cyan
        Invoke-RestMethod -Uri "http://localhost:5100/api/Theses/$createdThesisId" -Method DELETE
        Write-Host "SUCCESS: Thesis deleted!" -ForegroundColor Green
        
    } catch {
        Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Error Response: $responseBody" -ForegroundColor Red
        }
    }

} catch {
    Write-Host "Error getting initial data: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed!" -ForegroundColor Green
