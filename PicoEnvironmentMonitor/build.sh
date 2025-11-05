#!/bin/bash
echo "🏗️  Building Professional IoT Monitor..."
echo "=========================================="

# Clean
dotnet clean

# Restore packages
dotnet restore

# Build in Release mode
dotnet build -c Release --verbosity minimal

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build successful!"
    echo "🎯 Target: .NET 8.0"
    echo "📦 Architecture: Professional IoT System"
    echo ""
    echo "🚀 To run: ./run-pro.sh or dotnet run"
else
    echo "❌ Build failed!"
    exit 1
fi