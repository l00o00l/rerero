#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet("All", "EditMode", "PlayMode")]
    [string]$Mode = "All",
    [Alias("UnityPath")]
    [string]$UnityEditorPath = "",
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

function Invoke-UnityTests {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("EditMode", "PlayMode")]
        [string]$TestPlatform,

        [Parameter(Mandatory = $true)]
        [int]$TimeoutSeconds
    )

    New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

    $platformName = $TestPlatform.ToLowerInvariant()
    $resultPath = Join-Path $LogDir "pocketdodger-$platformName-results.xml"
    $logPath = Join-Path $LogDir "pocketdodger-$platformName-tests.log"
    $unityPath = Get-UnityEditorPath

    $unityArguments = @(
        "-batchmode",
        "-projectPath", $ProjectPath,
        "-runTests",
        "-testPlatform", $TestPlatform,
        "-testResults", $resultPath,
        "-logFile", $logPath
    )

    $exitCode = $null
    $maxAttempts = 3

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        foreach ($path in @($resultPath, $logPath)) {
            if (Test-Path $path) {
                Remove-Item -LiteralPath $path -Force
            }
        }

        Write-Host "Running PocketDodger $TestPlatform tests... attempt $attempt/$maxAttempts"
        $process = Start-Process -FilePath $unityPath -ArgumentList $unityArguments -WindowStyle Hidden -PassThru
        $completed = $process.WaitForExit($TimeoutSeconds * 1000)

        if (-not $completed) {
            Stop-Process -Id $process.Id -Force
            throw "Unity $TestPlatform tests timed out after $TimeoutSeconds seconds. See $logPath"
        }

        $exitCode = $process.ExitCode

        if (Test-Path $resultPath) {
            break
        }

        if ($attempt -lt $maxAttempts) {
            Write-Host "Unity $TestPlatform tests did not produce result XML. Retrying after Unity releases the project lock..."
            Start-Sleep -Seconds 10
        }
    }

    if (-not (Test-Path $resultPath)) {
        throw "Unity $TestPlatform tests did not produce a result XML. ExitCode=$exitCode. See $logPath"
    }

    [xml]$results = Get-Content -LiteralPath $resultPath
    $testRun = $results.SelectSingleNode("/test-run")

    if ($null -eq $testRun) {
        throw "Unity $TestPlatform result XML is missing the test-run root. See $resultPath"
    }

    $result = $testRun.GetAttribute("result")
    $total = $testRun.GetAttribute("total")
    $passed = $testRun.GetAttribute("passed")
    $failed = $testRun.GetAttribute("failed")
    $skipped = $testRun.GetAttribute("skipped")

    if ($result -ne "Passed") {
        throw "Unity $TestPlatform tests did not pass. Result=$result Total=$total Passed=$passed Failed=$failed Skipped=$skipped. See $resultPath"
    }

    if ($exitCode -ne 0) {
        throw "Unity $TestPlatform tests passed in XML but Unity exited with code $exitCode. See $logPath"
    }

    Write-Host "PocketDodger $TestPlatform tests passed. Total=$total Passed=$passed Failed=$failed Skipped=$skipped"
}

$platforms = if ($Mode -eq "All") {
    @("EditMode", "PlayMode")
} else {
    @($Mode)
}

for ($i = 0; $i -lt $platforms.Count; $i++) {
    Invoke-UnityTests -TestPlatform $platforms[$i] -TimeoutSeconds $TimeoutSeconds

    if ($i -lt ($platforms.Count - 1)) {
        Start-Sleep -Seconds 3
    }
}
