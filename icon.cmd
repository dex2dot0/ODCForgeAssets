@echo off
REM Wrapper for setup-project-icon.ps1
REM Usage:
REM   icon.cmd ProjectName --icon path\to\icon.png
REM   icon.cmd ProjectName --placeholder

if "%~1"=="" (
    echo Usage:
    echo   icon.cmd ProjectName --icon path\to\icon.png
    echo   icon.cmd ProjectName --placeholder
    exit /b 1
)

set PROJECT_NAME=%~1
set MODE=%~2
set VALUE=%~3

if /I "%MODE%"=="--icon" (
    powershell -ExecutionPolicy Bypass -Command "& { & '%~dp0setup-project-icon.ps1' -ProjectName '%PROJECT_NAME%' -IconPath '%VALUE%' }"
    exit /b %ERRORLEVEL%
)

if /I "%MODE%"=="--placeholder" (
    powershell -ExecutionPolicy Bypass -Command "& { & '%~dp0setup-project-icon.ps1' -ProjectName '%PROJECT_NAME%' -ScaffoldPlaceholder }"
    exit /b %ERRORLEVEL%
)

echo Invalid arguments.
echo Usage:
echo   icon.cmd ProjectName --icon path\to\icon.png
echo   icon.cmd ProjectName --placeholder
exit /b 1
