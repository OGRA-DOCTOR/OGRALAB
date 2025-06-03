@echo off
echo ============================================
echo OGRALAB - Starting Application
echo ============================================

echo.
echo Starting OGRALAB Medical Laboratory Management System...
echo.
echo Default login credentials:
echo   Username: admin
echo   Password: Admin@123
echo.

cd /d "%~dp0"
dotnet run --project src/OGRALAB

pause
