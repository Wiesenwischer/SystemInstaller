#!/bin/bash

echo "Testing logout flow..."

# Test 1: Check if app is accessible
echo "1. Testing app access..."
curl -s -o /dev/null -w "%{http_code} %{url_effective}" http://localhost:5000/
echo ""

# Test 2: Check logout endpoint
echo "2. Testing logout endpoint..."
curl -s -L -o /dev/null -w "%{http_code} %{url_effective}" http://localhost:5000/auth/logout
echo ""

# Test 3: Check if login endpoint works
echo "3. Testing login endpoint..."
curl -s -L -o /dev/null -w "%{http_code} %{url_effective}" http://localhost:5000/auth/login
echo ""

echo "Test completed."
