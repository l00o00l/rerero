#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$DeviceId = "",
    [string]$PackageName = "com.rerero.dreamlaundromat",
    [string]$LevelIndexes = "0,4,9,14,29",
    [string]$OutputDir = "",
    [string]$ReportPath = "",
    [int]$LaunchWaitSeconds = 10,
    [int]$TimeoutSeconds = 300,
    [string]$AvdName = "PocketDodger_API36",
    [string]$Gpu = "swiftshader_indirect",
    [int]$BootTimeoutSeconds = 180,
    [int]$BuildTimeoutSeconds = 900,
    [switch]$Build,
    [switch]$NoAutoStart
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$LogsPath = Join-Path $ProjectPath "Logs"
New-Item -ItemType Directory -Force -Path $LogsPath | Out-Null
$script:BatchDeadline = (Get-Date).AddSeconds([Math]::Max(1, $TimeoutSeconds))

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $LogsPath "level-screenshots"
}

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path $LogsPath "level-screenshots-report.txt"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

function Convert-LevelIndexes {
    param([Parameter(Mandatory = $true)][string]$Value)

    $indexes = @()
    foreach ($part in ($Value -split "[,;\s]+")) {
        if ([string]::IsNullOrWhiteSpace($part)) {
            continue
        }

        $levelIndex = 0
        if (-not [int]::TryParse($part.Trim(), [ref]$levelIndex)) {
            throw "Invalid level index '$part'. Use a comma-separated list like 0,4,9,14,29."
        }

        $indexes += $levelIndex
    }

    if ($indexes.Count -eq 0) {
        throw "At least one level index is required."
    }

    return $indexes
}

function Get-AndroidSdkCandidates {
    $candidates = @()
    if ($env:LOCALAPPDATA) {
        $candidates += Join-Path $env:LOCALAPPDATA "Android\Sdk"
    }

    if ($env:ANDROID_HOME) {
        $candidates += $env:ANDROID_HOME
    }

    if ($env:ANDROID_SDK_ROOT) {
        $candidates += $env:ANDROID_SDK_ROOT
    }

    return @($candidates | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
}

function Assert-BatchTimeout {
    param([Parameter(Mandatory = $true)][string]$Step)

    if ((Get-Date) -gt $script:BatchDeadline) {
        throw "Android level screenshot batch timed out after $TimeoutSeconds seconds during $Step."
    }
}

function Resolve-AdbPath {
    foreach ($sdkPath in Get-AndroidSdkCandidates) {
        $candidate = Join-Path $sdkPath "platform-tools\adb.exe"
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "adb.exe was not found. Install Android SDK platform-tools or set ANDROID_HOME."
}

function Start-AndroidRunTarget {
    $runScript = Join-Path $PSScriptRoot "run-emulator.ps1"
    if (-not (Test-Path -LiteralPath $runScript)) {
        throw "Android run script was not found: $runScript"
    }

    $runArguments = @{
        AvdName = $AvdName
        Gpu = $Gpu
        BootTimeoutSeconds = $BootTimeoutSeconds
        BuildTimeoutSeconds = $BuildTimeoutSeconds
    }

    if ($Build) {
        $runArguments.Build = $true
    }

    Write-Host "Starting DreamLaundromat Android run target for level screenshots..."
    & $runScript @runArguments
}

function Get-ConnectedDeviceIds {
    $output = & $script:AdbPath devices
    $devices = @()
    foreach ($line in $output) {
        if ($line -match "^(\S+)\s+device$") {
            $devices += $Matches[1]
        }
    }

    return $devices
}

function Invoke-AdbTarget {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    if ($script:TargetDeviceId) {
        & $script:AdbPath -s $script:TargetDeviceId @Arguments
    } else {
        & $script:AdbPath @Arguments
    }
}

function Invoke-AdbTargetChecked {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    Assert-BatchTimeout -Step $Description
    $output = Invoke-AdbTarget -Arguments $Arguments
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "$Description failed with exit code $exitCode. Output: $($output -join ' ')"
    }

    return $output
}

function Assert-PngFile {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Screenshot was not written: $Path"
    }

    $file = Get-Item -LiteralPath $Path
    if ($file.Length -lt 50000) {
        throw "Screenshot is too small to be useful: $($file.Length) bytes"
    }

    $stream = [System.IO.File]::OpenRead($file.FullName)
    try {
        $signature = New-Object byte[] 8
        [void]$stream.Read($signature, 0, 8)
        $expected = [byte[]](137, 80, 78, 71, 13, 10, 26, 10)
        for ($i = 0; $i -lt $expected.Length; $i++) {
            if ($signature[$i] -ne $expected[$i]) {
                throw "Screenshot is not a valid PNG file: $Path"
            }
        }
    } finally {
        $stream.Dispose()
    }
}

function Push-LevelOverride {
    param([Parameter(Mandatory = $true)][int]$LevelIndex)

    $tempFile = New-TemporaryFile
    try {
        Set-Content -LiteralPath $tempFile.FullName -Value "$LevelIndex" -NoNewline -Encoding ASCII
        $deviceDir = "/sdcard/Android/data/$PackageName/files"
        $devicePath = "$deviceDir/release-screenshot-level.txt"
        Invoke-AdbTargetChecked -Arguments @("shell", "mkdir", "-p", $deviceDir) -Description "override directory create" | Out-Null
        Invoke-AdbTargetChecked -Arguments @("push", $tempFile.FullName, $devicePath) -Description "level override push" | Out-Null
    } finally {
        Remove-Item -LiteralPath $tempFile.FullName -Force -ErrorAction SilentlyContinue
    }
}

function Capture-LevelScreenshot {
    param([Parameter(Mandatory = $true)][int]$LevelIndex)

    $displayNumber = $LevelIndex + 1
    $outputPath = Join-Path $OutputDir ("level-{0:00}.png" -f $displayNumber)
    $deviceShotPath = "/sdcard/dreamlaundromat-level-$LevelIndex.png"

    Invoke-AdbTargetChecked -Arguments @("logcat", "-c") -Description "logcat clear" | Out-Null
    Invoke-AdbTarget -Arguments @("shell", "am", "force-stop", $PackageName) | Out-Null
    Push-LevelOverride -LevelIndex $LevelIndex
    Invoke-AdbTargetChecked -Arguments @("shell", "monkey", "-p", $PackageName, "1") -Description "app launch level $LevelIndex" | Out-Null
    Assert-BatchTimeout -Step "app launch wait"
    Start-Sleep -Seconds ([Math]::Max(1, $LaunchWaitSeconds))

    $appPid = ((Invoke-AdbTargetChecked -Arguments @("shell", "pidof", $PackageName) -Description "pid check") | Select-Object -First 1).Trim()
    if ([string]::IsNullOrWhiteSpace($appPid)) {
        throw "Package '$PackageName' is not running for level $LevelIndex."
    }

    $focusOutput = Invoke-AdbTargetChecked -Arguments @("shell", "dumpsys", "window") -Description "focus check"
    $focusLine = @($focusOutput | Where-Object { $_ -match "mCurrentFocus|mFocusedApp|topResumedActivity" } | Select-Object -First 1)
    if ($focusLine.Count -eq 0 -or ($focusLine[0] -notmatch [regex]::Escape($PackageName))) {
        throw "Package '$PackageName' is not focused. Focus: $($focusLine -join ' ')"
    }

    Invoke-AdbTargetChecked -Arguments @("shell", "screencap", "-p", $deviceShotPath) -Description "screenshot capture level $LevelIndex" | Out-Null
    Invoke-AdbTargetChecked -Arguments @("pull", $deviceShotPath, $outputPath) -Description "screenshot pull level $LevelIndex" | Out-Null
    Invoke-AdbTarget -Arguments @("shell", "rm", $deviceShotPath) | Out-Null
    Assert-PngFile -Path $outputPath

    $logcat = Invoke-AdbTargetChecked -Arguments @("logcat", "-d", "-t", "300") -Description "logcat read"
    $crashes = @($logcat | Where-Object { ($_ -match "FATAL EXCEPTION|Fatal signal|native crash|ANR in $([regex]::Escape($PackageName))") -and ($_ -notmatch "crashrecovery") })
    if ($crashes.Count -gt 0) {
        throw "Crash-like logcat lines were found after level $LevelIndex launch: $($crashes[0])"
    }

    return @{
        LevelIndex = $LevelIndex
        Screenshot = $outputPath
        Bytes = (Get-Item -LiteralPath $outputPath).Length
        Pid = $appPid
        Focused = $focusLine[0]
    }
}

$script:AdbPath = Resolve-AdbPath
$script:TargetDeviceId = $DeviceId

if ($Build) {
    Start-AndroidRunTarget
}

if ([string]::IsNullOrWhiteSpace($script:TargetDeviceId)) {
    $devices = @(Get-ConnectedDeviceIds)
    if ($devices.Count -eq 0) {
        if ($NoAutoStart) {
            throw "No Android device is connected. Run DreamLaundromat\run.cmd first or connect a device."
        }

        Start-AndroidRunTarget
        $devices = @(Get-ConnectedDeviceIds)
    }

    if ($devices.Count -eq 0) {
        throw "No Android device became available after starting the DreamLaundromat run target."
    }

    $script:TargetDeviceId = $devices[0]
}

$parsedLevelIndexes = @(Convert-LevelIndexes -Value $LevelIndexes)
$results = @()
foreach ($levelIndex in $parsedLevelIndexes) {
    if ($levelIndex -lt 0) {
        throw "Level index must be non-negative: $levelIndex"
    }

    Write-Host "Capturing release level screenshot: index=$levelIndex"
    $results += Capture-LevelScreenshot -LevelIndex $levelIndex
}

$report = @(
    "DreamLaundromat Android Level Screenshot Batch",
    "DeviceId=$script:TargetDeviceId",
    "PackageName=$PackageName",
    "OutputDir=$OutputDir",
    "LevelCount=$($results.Count)"
)

foreach ($result in $results) {
    $report += "LevelIndex=$($result.LevelIndex) Screenshot=$($result.Screenshot) Bytes=$($result.Bytes)"
}

Set-Content -LiteralPath $ReportPath -Value $report -Encoding UTF8
Write-Host "Android level screenshot batch passed: $ReportPath"
