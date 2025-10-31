@echo off
REM Quick wrapper for generate_upload_package.ps1
REM Usage: publish.cmd ProjectName [Configuration] [OutputName]

if "%~1"=="" (
    echo Usage: publish.cmd ProjectName [Configuration] [OutputName]
    echo.
    echo Examples:
    echo   publish.cmd MathRound
    echo   publish.cmd MathRound Release
    echo   publish.cmd MathRound Release MathRound-v1.0.zip
    echo.
    echo Note: Runtime is always linux-x64 for OutSystems
    echo.
    echo For more options, see PACKAGING.md or use PowerShell:
    echo   powershell -File generate_upload_package.ps1 -ProjectName MathRound
    exit /b 1
)

set PROJECT_NAME=%~1
set CONFIGURATION=%~2
set OUTPUT_NAME=%~3

if "%CONFIGURATION%"=="" set CONFIGURATION=Release

if "%OUTPUT_NAME%"=="" (
    powershell -ExecutionPolicy Bypass -File "%~dp0generate_upload_package.ps1" -ProjectName "%PROJECT_NAME%" -Configuration "%CONFIGURATION%"
) else (
    powershell -ExecutionPolicy Bypass -File "%~dp0generate_upload_package.ps1" -ProjectName "%PROJECT_NAME%" -Configuration "%CONFIGURATION%" -OutputName "%OUTPUT_NAME%"
)

