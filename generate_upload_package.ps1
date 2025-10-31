param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName,
    
    [string]$Configuration = "Release",
    
    [string]$OutputName,
    
    [bool]$SelfContained = $false,
    
    [string]$TargetFramework
)

# Hard-coded runtime for OutSystems (Linux)
$Runtime = "linux-x64"

# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Publishing Package for $ProjectName" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Validate project exists
$projectPath = Join-Path $PSScriptRoot $ProjectName
$csprojFile = Join-Path $projectPath "$ProjectName.csproj"

if (-not (Test-Path $csprojFile)) {
    Write-Error "Project file not found: $csprojFile"
    exit 1
}

Write-Host "Project: $csprojFile" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Green
Write-Host "Runtime: $Runtime" -ForegroundColor Green
Write-Host "Self-contained: $SelfContained" -ForegroundColor Green

# If no target framework specified, try to detect it from csproj
if (-not $TargetFramework) {
    $csprojContent = Get-Content $csprojFile -Raw
    if ($csprojContent -match '<TargetFramework>(.*?)</TargetFramework>') {
        $TargetFramework = $Matches[1]
        Write-Host "Detected target framework: $TargetFramework" -ForegroundColor Green
    }
}

# Build publish command
$publishArgs = @(
    "publish",
    $csprojFile,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", $SelfContained.ToString().ToLower()
)

Write-Host "`nRunning: dotnet $($publishArgs -join ' ')" -ForegroundColor Yellow
& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Determine publish path
$publishPath = Join-Path $projectPath "bin\$Configuration\$TargetFramework\$Runtime\publish\*"

if (-not (Test-Path $publishPath)) {
    Write-Error "Publish output not found at: $publishPath"
    exit 1
}

# Set output zip name
if (-not $OutputName) {
    $OutputName = "$ProjectName.zip"
}

# Ensure .zip extension
if (-not $OutputName.EndsWith(".zip")) {
    $OutputName += ".zip"
}

# Save to project bin folder
$outputDir = Join-Path $projectPath "bin\$Configuration"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}
$outputPath = Join-Path $outputDir $OutputName

# Remove existing zip if it exists
if (Test-Path $outputPath) {
    Write-Host "`nRemoving existing package: $outputPath" -ForegroundColor Yellow
    Remove-Item $outputPath -Force
}

# Create zip archive
Write-Host "`nCreating package: $outputPath" -ForegroundColor Yellow
Compress-Archive -Path $publishPath -DestinationPath $outputPath -Force

if (Test-Path $outputPath) {
    $fileSize = (Get-Item $outputPath).Length / 1KB
    Write-Host "`n====================================" -ForegroundColor Green
    Write-Host "SUCCESS!" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Green
    Write-Host "Package created: $outputPath" -ForegroundColor Green
    Write-Host "Size: $([math]::Round($fileSize, 2)) KB" -ForegroundColor Green
} else {
    Write-Error "Failed to create package"
    exit 1
}