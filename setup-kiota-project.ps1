param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,

    [string]$InputName,

    [string]$ClientClassName,

    [string]$GeneratedClassName,

    [string]$GeneratedInterfaceName,

    [string]$TargetFramework = "net10.0",

    [string]$SolutionPath,

    [switch]$SkipSolutionAdd
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($SolutionPath)) {
    $SolutionPath = Join-Path $PSScriptRoot "ForgeAssets.sln"
}

function Get-DefaultGeneratedClassName {
    param([string]$Name)
    return "$Name" + "ActionsGenerated"
}

function Get-DefaultGeneratedInterfaceName {
    param([string]$Name)
    return "I$Name" + "ActionsGenerated"
}

if ([string]::IsNullOrWhiteSpace($InputName)) {
    $InputName = $ProjectName
}

if ([string]::IsNullOrWhiteSpace($ClientClassName)) {
    $ClientClassName = "$ProjectName" + "Client"
}

if ([string]::IsNullOrWhiteSpace($GeneratedClassName)) {
    $GeneratedClassName = Get-DefaultGeneratedClassName -Name $ProjectName
}

if ([string]::IsNullOrWhiteSpace($GeneratedInterfaceName)) {
    $GeneratedInterfaceName = Get-DefaultGeneratedInterfaceName -Name $ProjectName
}

$projectDirectory = Join-Path $PSScriptRoot $ProjectName
$csprojPath = Join-Path $projectDirectory "$ProjectName.csproj"
$configPath = Join-Path $projectDirectory "outsystems-generator.json"

if (Test-Path $projectDirectory) {
    throw "Project directory already exists: $projectDirectory"
}

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Scaffolding Kiota Project" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Project: $ProjectName" -ForegroundColor Green
Write-Host "Input name: $InputName" -ForegroundColor Green
Write-Host "Client class: $ClientClassName" -ForegroundColor Green
Write-Host "Generated class: $GeneratedClassName" -ForegroundColor Green
Write-Host "Generated interface: $GeneratedInterfaceName" -ForegroundColor Green

& dotnet new classlib -n $ProjectName -f $TargetFramework
if ($LASTEXITCODE -ne 0) {
    throw "dotnet new classlib failed with exit code $LASTEXITCODE"
}

if (-not $SkipSolutionAdd -and (Test-Path $SolutionPath)) {
    & dotnet sln $SolutionPath add $csprojPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet sln add failed with exit code $LASTEXITCODE"
    }
}

if (Test-Path (Join-Path $projectDirectory "Class1.cs")) {
    Remove-Item (Join-Path $projectDirectory "Class1.cs") -Force
}

New-Item -ItemType Directory -Path (Join-Path $projectDirectory "Client") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $projectDirectory "Generated") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $projectDirectory "Specs") -Force | Out-Null

[xml]$projectXml = Get-Content -Path $csprojPath

$packageGroup = $projectXml.CreateElement("ItemGroup")
$kiotaReference = $projectXml.CreateElement("PackageReference")
$kiotaReference.SetAttribute("Include", "Microsoft.Kiota.Bundle")
$kiotaReference.SetAttribute("Version", "2.0.0")
[void]$packageGroup.AppendChild($kiotaReference)

$outsystemsReference = $projectXml.CreateElement("PackageReference")
$outsystemsReference.SetAttribute("Include", "OutSystems.ExternalLibraries.SDK")
$outsystemsReference.SetAttribute("Version", "1.5.0")
[void]$packageGroup.AppendChild($outsystemsReference)
[void]$projectXml.Project.AppendChild($packageGroup)

$folderGroup = $projectXml.CreateElement("ItemGroup")
foreach ($folderName in @("Client\", "Generated\", "Specs\")) {
    $folder = $projectXml.CreateElement("Folder")
    $folder.SetAttribute("Include", $folderName)
    [void]$folderGroup.AppendChild($folder)
}
[void]$projectXml.Project.AppendChild($folderGroup)

$projectXml.Save($csprojPath)

$config = [ordered]@{
    namespace = "$ProjectName.Generated"
    className = $GeneratedClassName
    interfaceName = $GeneratedInterfaceName
    inputName = $InputName
}

$config | ConvertTo-Json -Depth 10 | Set-Content -Path $configPath

Write-Host ""
Write-Host "Created:" -ForegroundColor Cyan
Write-Host " - $csprojPath"
Write-Host " - $configPath"
Write-Host " - $projectDirectory\Client"
Write-Host " - $projectDirectory\Generated"
Write-Host " - $projectDirectory\Specs"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host " 1. Put your OpenAPI spec under '$ProjectName\Specs\' or choose a remote URL."
Write-Host " 2. Run 'dotnet tool restore' if needed."
Write-Host " 3. Run Kiota to generate the client into '$ProjectName\Client'."
Write-Host " 4. Run KiotaOutSystems.Generator with '$configPath'."
