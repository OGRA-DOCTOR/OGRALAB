@echo off
echo ============================================
echo OGRALAB - Build Script
echo ============================================

echo.
echo Cleaning previous builds...
dotnet clean

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building project...
dotnet build --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo Build completed successfully!
    echo ============================================
    echo.
    echo To run the application:
    echo   dotnet run --project src/OGRALAB
    echo.
    echo Default login credentials:
    echo   Username: admin
    echo   Password: Admin@123
    echo.
) else (
    echo.
    echo ============================================
    echo Build failed! Check the errors above.
    echo ============================================
)

pause
