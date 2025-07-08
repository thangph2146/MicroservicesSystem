#!/bin/bash

# Test script for DataManagementApi Theses endpoints

echo "=== Testing Theses API Endpoints ==="

# Base URL
BASE_URL="http://localhost:5100/api"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting API tests...${NC}"

# Test 1: Get all theses
echo -e "\n${YELLOW}Test 1: GET /api/Theses${NC}"
curl -s -X GET "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n" \
  | head -20

# Test 2: Create a new thesis with valid data
echo -e "\n${YELLOW}Test 2: POST /api/Theses (Valid Data)${NC}"
curl -s -X POST "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Thesis - API Test",
    "studentId": 1,
    "academicYearId": 1,
    "semesterId": 1,
    "submissionDate": "2025-08-01"
  }' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 3: Create a new thesis with invalid student ID
echo -e "\n${YELLOW}Test 3: POST /api/Theses (Invalid Student ID)${NC}"
curl -s -X POST "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Thesis - Invalid Student",
    "studentId": 999999,
    "academicYearId": 1,
    "semesterId": 1,
    "submissionDate": "2025-08-01"
  }' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 4: Create a new thesis with invalid academic year ID
echo -e "\n${YELLOW}Test 4: POST /api/Theses (Invalid Academic Year ID)${NC}"
curl -s -X POST "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Thesis - Invalid Academic Year",
    "studentId": 1,
    "academicYearId": 999999,
    "semesterId": 1,
    "submissionDate": "2025-08-01"
  }' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 5: Create a new thesis with invalid semester ID
echo -e "\n${YELLOW}Test 5: POST /api/Theses (Invalid Semester ID)${NC}"
curl -s -X POST "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Thesis - Invalid Semester",
    "studentId": 1,
    "academicYearId": 1,
    "semesterId": 999999,
    "submissionDate": "2025-08-01"
  }' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 6: Create a new thesis with missing title
echo -e "\n${YELLOW}Test 6: POST /api/Theses (Missing Title)${NC}"
curl -s -X POST "$BASE_URL/Theses" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "",
    "studentId": 1,
    "academicYearId": 1,
    "semesterId": 1,
    "submissionDate": "2025-08-01"
  }' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 7: Get a specific thesis (assuming ID 1 exists)
echo -e "\n${YELLOW}Test 7: GET /api/Theses/1${NC}"
curl -s -X GET "$BASE_URL/Theses/1" \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n"

# Test 8: Get a non-existent thesis
echo -e "\n${YELLOW}Test 8: GET /api/Theses/999999${NC}"
curl -s -X GET "$BASE_URL/Theses/999999" \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n"

echo -e "\n${GREEN}API tests completed!${NC}"
