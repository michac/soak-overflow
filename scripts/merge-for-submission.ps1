#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Merges all C# source files from the bot project into a single file for CodinGame submission.

.DESCRIPTION
    This script reads all .cs files from the bot/ directory, performs smart merging:
    - Deduplicates using statements
    - Preserves namespace declarations
    - Combines all type definitions (classes, records, enums, interfaces)

    Output is written to output/submission.cs

.EXAMPLE
    pwsh.exe scripts/merge-for-submission.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Get script root directory
$scriptRoot = Split-Path -Parent $PSScriptRoot
$botDir = Join-Path $scriptRoot "bot"
$outputDir = Join-Path $scriptRoot "output"
$outputFile = Join-Path $outputDir "submission.cs"

Write-Host "Merging C# files from: $botDir" -ForegroundColor Cyan
Write-Host "Output will be written to: $outputFile" -ForegroundColor Cyan

# Create output directory if it doesn't exist
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
    Write-Host "Created output directory: $outputDir" -ForegroundColor Green
}

# Get all C# files from bot directory
$csFiles = Get-ChildItem -Path $botDir -Filter "*.cs" -File | Sort-Object Name

if ($csFiles.Count -eq 0) {
    Write-Error "No .cs files found in $botDir"
    exit 1
}

Write-Host "Found $($csFiles.Count) C# files:" -ForegroundColor Yellow
$csFiles | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }

# Collections for parsed content
$allUsings = [System.Collections.Generic.HashSet[string]]::new()
$namespaceContent = [System.Text.StringBuilder]::new()

# Process each file
foreach ($file in $csFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Gray

    $content = Get-Content -Path $file.FullName -Raw
    $lines = Get-Content -Path $file.FullName

    $inUsings = $true
    $classContent = [System.Collections.Generic.List[string]]::new()

    foreach ($line in $lines) {
        $trimmed = $line.Trim()

        # Collect using statements
        if ($trimmed -match '^using\s+.*?;' -and $inUsings) {
            [void]$allUsings.Add($trimmed)
            continue
        }

        # Skip namespace declarations entirely (file-scoped)
        if ($trimmed -match '^namespace\s+.*;$') {
            continue
        }

        # Skip XML doc comments at file level
        if ($trimmed -match '^///') {
            continue
        }

        # Skip empty lines at the start
        if ([string]::IsNullOrWhiteSpace($trimmed) -and $inUsings) {
            continue
        }

        # We're past usings now
        if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
            $inUsings = $false
        }

        # Add all content after usings and namespaces
        if (-not $inUsings) {
            $classContent.Add($line)
        }
    }

    # Remove trailing empty lines
    while ($classContent.Count -gt 0 -and [string]::IsNullOrWhiteSpace($classContent[-1])) {
        $classContent.RemoveAt($classContent.Count - 1)
    }

    # Add class content to namespace content
    foreach ($line in $classContent) {
        [void]$namespaceContent.AppendLine($line)
    }

    # Add separator between files
    [void]$namespaceContent.AppendLine()
}

# Build final output
$output = [System.Text.StringBuilder]::new()

# Add using statements (sorted)
$sortedUsings = $allUsings | Sort-Object
foreach ($using in $sortedUsings) {
    [void]$output.AppendLine($using)
}

[void]$output.AppendLine()

# Add namespace wrapper
[void]$output.AppendLine("namespace SoakOverflow;")
[void]$output.AppendLine()

# Add all class content
[void]$output.Append($namespaceContent.ToString())

# Write to output file
$finalContent = $output.ToString()
Set-Content -Path $outputFile -Value $finalContent -Encoding UTF8

Write-Host "`nMerge completed successfully!" -ForegroundColor Green
Write-Host "Output file: $outputFile" -ForegroundColor Green
Write-Host "Total lines: $($finalContent -split "`n" | Measure-Object).Count" -ForegroundColor Cyan
