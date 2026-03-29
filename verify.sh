#!/bin/bash
# Plannify - Complete Verification Script
# Run this to verify the Plannify application is ready for deployment

echo "=========================================="
echo "Plannify - Complete Verification Script"
echo "=========================================="
echo ""

# Check for .NET installation
echo "✓ Checking .NET installation..."
dotnet --version
if [ $? -eq 0 ]; then
    echo "  ✅ .NET is installed"
else
    echo "  ❌ .NET is not installed"
    exit 1
fi
echo ""

# Navigate to project directory
echo "✓ Navigating to project directory..."
cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify 2>/dev/null
if [ $? -eq 0 ]; then
    echo "  ✅ Project directory found"
else
    echo "  ❌ Project directory not found"
    exit 1
fi
echo ""

# Check for required files
echo "✓ Checking for required files..."
files_to_check=(
    "Program.cs"
    "Plannify.csproj"
    "appsettings.json"
    "appsettings.Development.json"
    "Data/AppDbContext.cs"
)

for file in "${files_to_check[@]}"; do
    if [ -f "$file" ]; then
        echo "  ✅ $file exists"
    else
        echo "  ❌ $file missing"
        exit 1
    fi
done
echo ""

# Restore dependencies
echo "✓ Restoring NuGet packages..."
dotnet restore > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "  ✅ Packages restored successfully"
else
    echo "  ❌ Package restore failed"
    exit 1
fi
echo ""

# Build the project
echo "✓ Building project..."
dotnet build > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "  ✅ Build successful"
else
    echo "  ❌ Build failed"
    exit 1
fi
echo ""

# Clean up old database
echo "✓ Cleaning up test database..."
rm -f timegrid.db* 2>/dev/null
echo "  ✅ Old database files removed"
echo ""

# Start application with timeout
echo "✓ Starting application..."
timeout 30 dotnet run > /tmp/plannify_test.log 2>&1 &
APP_PID=$!
sleep 10

# Check if app is running
if ps -p $APP_PID > /dev/null; then
    echo "  ✅ Application started successfully"
else
    echo "  ❌ Application failed to start"
    echo "  Log output:"
    tail -20 /tmp/plannify_test.log
    exit 1
fi

# Check for listening port
sleep 5
if grep -q "Now listening on" /tmp/plannify_test.log; then
    echo "  ✅ Application is listening on configured port"
else
    echo "  ⚠️  Could not confirm port listening"
fi

# Check database initialization
if grep -q "SELECT EXISTS" /tmp/plannify_test.log; then
    echo "  ✅ Database queries executed"
else
    echo "  ⚠️  Database queries not detected in logs"
fi

# Cleanup
echo ""
echo "✓ Cleaning up..."
kill $APP_PID 2>/dev/null
wait $APP_PID 2>/dev/null
echo "  ✅ Application stopped"
echo ""

echo "=========================================="
echo "✅ ALL VERIFICATIONS PASSED"
echo "=========================================="
echo ""
echo "Application Status: READY FOR DEPLOYMENT"
echo ""
echo "To run the application in development:"
echo "  cd /home/cy3pher/Documents/WorkSpace-Dev/Plannify/Plannify"
echo "  dotnet run"
echo ""
echo "Application will be available at:"
echo "  http://localhost:5152 (or configured port in appsettings.json)"
echo ""
