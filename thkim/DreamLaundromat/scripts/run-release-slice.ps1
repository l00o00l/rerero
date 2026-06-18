#requires -Version 5.1
[CmdletBinding()]
param(
    [Alias("UnityPath")]
    [string]$UnityEditorPath = "",
    [int]$MaxVisitedStates = 60000,
    [int]$SolverTimeoutMilliseconds = 4000,
    [string]$ReportPath = "",
    [int]$TimeoutSeconds = 600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$LogDir = Join-Path $ProjectPath "Logs"

function Resolve-FirstExistingPath {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$CandidatePaths,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    foreach ($candidate in $CandidatePaths) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (Test-Path $candidate) {
            return (Resolve-Path $candidate).Path
        }
    }

    throw "$Description was not found. Checked: $($CandidatePaths -join ', ')"
}

function Get-UnityEditorPath {
    $candidates = @()

    if ($UnityEditorPath) {
        $candidates += $UnityEditorPath
    }

    if ($env:UNITY_EDITOR_PATH) {
        $candidates += $env:UNITY_EDITOR_PATH
    }

    $candidates += "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"

    return Resolve-FirstExistingPath -CandidatePaths $candidates -Description "Unity Editor"
}

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $LogDir "release-slice-validation-report.txt"
}

if (-not [System.IO.Path]::IsPathRooted($ReportPath)) {
    $ReportPath = Join-Path (Get-Location) $ReportPath
}

$ReportPath = [System.IO.Path]::GetFullPath($ReportPath)
$LogPath = Join-Path $LogDir "release-slice-validation.log"
$UnityPath = Get-UnityEditorPath

foreach ($path in @($ReportPath, $LogPath)) {
    if (Test-Path $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

$unityArguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $ProjectPath,
    "-executeMethod", "Thkim.DreamLaundromat.Editor.ReleaseSlice.ReleaseSliceValidationReport.RunFromCommandLine",
    "-logFile", $LogPath,
    "-releaseSliceReportPath", $ReportPath,
    "-releaseSliceMaxVisitedStates", $MaxVisitedStates,
    "-releaseSliceSolverTimeoutMilliseconds", $SolverTimeoutMilliseconds,
    "-releaseSliceFailOnInvalid", "true"
)

Write-Host "Running Release Slice level validation..."
$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArguments -WindowStyle Hidden -PassThru
$completed = $process.WaitForExit($TimeoutSeconds * 1000)

if (-not $completed) {
    Stop-Process -Id $process.Id -Force
    throw "Release Slice validation timed out after $TimeoutSeconds seconds. See $LogPath"
}

if ($process.ExitCode -ne 0) {
    throw "Release Slice validation failed with exit code $($process.ExitCode). See $LogPath"
}

if (-not (Test-Path $ReportPath)) {
    throw "Release Slice validation did not produce output. See $LogPath"
}

Write-Host "Release Slice validation report written: $ReportPath"
Get-Content -LiteralPath $ReportPath -TotalCount 16
