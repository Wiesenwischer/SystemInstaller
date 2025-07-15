#!/bin/bash

echo "🚀 Running YARP Routing Configuration Tests"
echo "==========================================="
echo ""

# Ensure we're in the right directory
cd "$(dirname "$0")"

echo "📍 Current directory: $(pwd)"
echo "🔍 Testing YARP routing configuration..."
echo ""

# Run the YARP routing test
node yarp-routing-test.js

echo ""
echo "✅ YARP routing tests completed!"
echo ""
echo "💡 If auth routes are being proxied incorrectly, the YARP configuration needs adjustment."
echo "💡 If frontend routes are not being proxied, check the YARP cluster configuration."
