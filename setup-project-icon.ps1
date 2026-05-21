param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,

    [string]$IconPath,

    [switch]$ScaffoldPlaceholder,

    [string]$IconFileName,

    [string]$BaseDirectory = $PSScriptRoot
)

$ErrorActionPreference = "Stop"

function New-PlaceholderIcon {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$ProjectName
    )

    Add-Type -AssemblyName System.Drawing

    $bitmap = New-Object System.Drawing.Bitmap 256, 256
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::FromArgb(245, 238, 251))

    $borderPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(122, 45, 190)), 14
    $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(122, 45, 190))
    $font = New-Object System.Drawing.Font("Segoe UI", 88, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)

    $graphics.DrawRectangle($borderPen, 24, 24, 208, 208)

    $initials = Get-Initials -ProjectName $ProjectName
    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
    $graphics.DrawString($initials, $font, $textBrush, ([System.Drawing.RectangleF]::new(0, 0, 256, 256)), $stringFormat)

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)

    $stringFormat.Dispose()
    $font.Dispose()
    $textBrush.Dispose()
    $borderPen.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

function Get-Initials {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectName
    )

    $parts = $ProjectName -split '(?<=[a-z])(?=[A-Z])|[_\-\s]+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    if ($parts.Count -eq 0) {
        return "?"
    }

    $letters = $parts | ForEach-Object { $_.Substring(0, 1).ToUpperInvariant() }
    return (($letters -join '').Substring(0, [Math]::Min(2, ($letters -join '').Length)))
}

function Update-CsprojIconResource {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsprojPath,

        [Parameter(Mandatory = $true)]
        [string]$ResourceInclude
    )

    [xml]$projectXml = Get-Content -Path $CsprojPath

    foreach ($itemGroup in @($projectXml.Project.ItemGroup)) {
        foreach ($node in @($itemGroup.ChildNodes)) {
            $include = $node.Include
            if (($node.Name -eq "Content" -or $node.Name -eq "EmbeddedResource") -and $include -like "resources\*.png") {
                [void]$itemGroup.RemoveChild($node)
            }
        }
    }

    $embeddedGroup = $projectXml.Project.ItemGroup | Where-Object { $_.EmbeddedResource } | Select-Object -First 1
    if (-not $embeddedGroup) {
        $embeddedGroup = $projectXml.CreateElement("ItemGroup")
        [void]$projectXml.Project.AppendChild($embeddedGroup)
    }

    $embeddedResource = $projectXml.CreateElement("EmbeddedResource")
    $includeAttribute = $projectXml.CreateAttribute("Include")
    $includeAttribute.Value = $ResourceInclude
    [void]$embeddedResource.Attributes.Append($includeAttribute)
    [void]$embeddedGroup.AppendChild($embeddedResource)

    $projectXml.Save($CsprojPath)
}

function Update-GeneratorConfigIcon {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConfigPath,

        [Parameter(Mandatory = $true)]
        [string]$IconFileName,

        [Parameter(Mandatory = $true)]
        [string]$ResourceName
    )

    $config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json
    if (-not $config.PSObject.Properties['icon']) {
        $config | Add-Member -NotePropertyName icon -NotePropertyValue ([pscustomobject]@{})
    }

    $config.icon | Add-Member -NotePropertyName fileName -NotePropertyValue $IconFileName -Force
    $config.icon | Add-Member -NotePropertyName resourceName -NotePropertyValue $ResourceName -Force

    $config | ConvertTo-Json -Depth 20 | Set-Content -Path $ConfigPath
}

function Update-HandwrittenInterfaceIcon {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$ResourceName
    )

    $sourceFiles = Get-ChildItem -Path $ProjectPath -Recurse -Filter *.cs -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj|Generated)\\' }

    $interfaceFiles = @()
    foreach ($file in $sourceFiles) {
        $content = Get-Content -Path $file.FullName -Raw
        if ($content.IndexOf('[OSInterface', [System.StringComparison]::Ordinal) -ge 0) {
            $interfaceFiles += [pscustomobject]@{
                Path = $file.FullName
                Content = $content
            }
        }
    }

    if ($interfaceFiles.Count -eq 0) {
        throw "No OSInterface attribute was found in $ProjectPath."
    }

    if ($interfaceFiles.Count -gt 1) {
        throw "More than one OSInterface source file was found in $ProjectPath. Please wire the icon manually."
    }

    $fileToPatch = $interfaceFiles[0]
    $content = $fileToPatch.Content
    $pattern = '\[OSInterface\((?<args>[\s\S]*?)\)\]'
    $match = [regex]::Match($content, $pattern)
    if (-not $match.Success) {
        throw "Unable to locate the OSInterface attribute in $($fileToPatch.Path)."
    }

    $attributeArguments = $match.Groups['args'].Value
    if ($attributeArguments -match 'IconResourceName\s*=') {
        $updatedArgs = [regex]::Replace($attributeArguments, 'IconResourceName\s*=\s*"[^"]*"', "IconResourceName = `"$ResourceName`"")
    }
    else {
        $updatedArgs = $attributeArguments.TrimEnd() + ", IconResourceName = `"$ResourceName`""
    }

    $updatedAttribute = "[OSInterface($updatedArgs)]"
    $updatedContent = [regex]::Replace($content, $pattern, [System.Text.RegularExpressions.MatchEvaluator]{ param($m) $updatedAttribute }, 1)
    Set-Content -Path $fileToPatch.Path -Value $updatedContent

    Write-Host "Updated OSInterface source: $($fileToPatch.Path)" -ForegroundColor Green
}

function Invoke-Main {
    if ([string]::IsNullOrWhiteSpace($IconPath) -and -not $ScaffoldPlaceholder) {
        throw "Specify either -IconPath or -ScaffoldPlaceholder."
    }

    if (-not [string]::IsNullOrWhiteSpace($IconPath) -and $ScaffoldPlaceholder) {
        throw "Use either -IconPath or -ScaffoldPlaceholder, not both."
    }

    $projectPath = Join-Path $BaseDirectory $ProjectName
    $csprojPath = Join-Path $projectPath "$ProjectName.csproj"
    $generatorConfigPath = Join-Path $projectPath "outsystems-generator.json"
    $resourcesPath = Join-Path $projectPath "resources"

    if (-not (Test-Path $projectPath)) {
        throw "Project folder not found: $projectPath"
    }

    if (-not (Test-Path $csprojPath)) {
        throw "Project file not found: $csprojPath"
    }

    New-Item -ItemType Directory -Path $resourcesPath -Force | Out-Null

    if ([string]::IsNullOrWhiteSpace($IconFileName)) {
        if (-not [string]::IsNullOrWhiteSpace($IconPath)) {
            $IconFileName = Split-Path -Leaf $IconPath
        }
        else {
            $IconFileName = "ProjectIcon.png"
        }
    }

    $targetIconPath = Join-Path $resourcesPath $IconFileName
    $resourceInclude = "resources\$IconFileName"
    $resourceName = "$ProjectName.resources.$IconFileName"

    if (-not [string]::IsNullOrWhiteSpace($IconPath)) {
        $resolvedIconPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($IconPath)
        if (-not (Test-Path $resolvedIconPath)) {
            throw "Icon file not found: $resolvedIconPath"
        }

        if ([System.IO.Path]::GetFullPath($targetIconPath) -ne [System.IO.Path]::GetFullPath($resolvedIconPath)) {
            Copy-Item -Path $resolvedIconPath -Destination $targetIconPath -Force
        }
    }
    else {
        New-PlaceholderIcon -Path $targetIconPath -ProjectName $ProjectName
    }

    Update-CsprojIconResource -CsprojPath $csprojPath -ResourceInclude $resourceInclude

    if (Test-Path $generatorConfigPath) {
        Update-GeneratorConfigIcon -ConfigPath $generatorConfigPath -IconFileName $IconFileName -ResourceName $resourceName
        Write-Host "Updated generator config: $generatorConfigPath" -ForegroundColor Green
        Write-Host "WARNING: This is a generator-backed project. Regenerate the OutSystems wrapper files before you publish so the generated OSInterface picks up the new IconResourceName." -ForegroundColor Yellow
    }
    else {
        Update-HandwrittenInterfaceIcon -ProjectPath $projectPath -ResourceName $resourceName
    }

    Write-Host "Icon wired successfully." -ForegroundColor Green
    Write-Host "Project: $ProjectName" -ForegroundColor Green
    Write-Host "Icon file: $targetIconPath" -ForegroundColor Green
    Write-Host "Embedded resource: $resourceName" -ForegroundColor Green
}

Invoke-Main
