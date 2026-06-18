param(
    [Alias("UnityPath")]
    [string] $UnityEditorPath = "",
    [int] $TimeoutSeconds = 900,
    [string] $ReportPath = ""
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$unity = if (-not [string]::IsNullOrWhiteSpace($UnityEditorPath)) {
    $UnityEditorPath
} elseif (-not [string]::IsNullOrWhiteSpace($env:UNITY_EDITOR_PATH)) {
    $env:UNITY_EDITOR_PATH
} else {
    "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
}
if (-not (Test-Path -LiteralPath $unity)) {
    throw "Unity Editor was not found at $unity"
}

$logs = Join-Path $projectPath "Logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $logs "release-balance-report.txt"
}

$logPath = Join-Path $logs "release-balance-report.log"
$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectPath,
    "-logFile", $logPath,
    "-executeMethod", "Thkim.DreamLaundromat.Editor.ReleaseSlice.ReleaseBalanceReport.RunFromCommandLine",
    "-releaseBalanceReportPath", $ReportPath
)

Write-Host "Writing DreamLaundromat QA balance report..."
$process = Start-Process -FilePath $unity -ArgumentList $arguments -PassThru -WindowStyle Hidden
$completed = $process.WaitForExit($TimeoutSeconds * 1000)
if (-not $completed) {
    Stop-Process -Id $process.Id -Force
    throw "Unity balance report timed out after $TimeoutSeconds seconds. Log: $logPath"
}

if ($process.ExitCode -ne 0) {
    throw "Unity balance report failed with exit code $($process.ExitCode). Log: $logPath"
}

if (-not (Test-Path -LiteralPath $ReportPath)) {
    throw "Balance report was not written: $ReportPath"
}

Get-Content -LiteralPath $ReportPath -TotalCount 16
