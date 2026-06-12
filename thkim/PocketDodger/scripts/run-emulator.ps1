#requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$Build,
    [string]$AvdName = "PocketDodger_API36",
    [string]$Gpu = "swiftshader_indirect",
    [string]$DeviceId = "",
    [int]$BootTimeoutSeconds = 180,
    [int]$BuildTimeoutSeconds = 900
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$RepoRoot = (Resolve-Path (Join-Path $ProjectPath "..")).Path
$ApkPath = Join-Path $ProjectPath "Builds\Android\PocketDodger-debug.apk"
$PackageName = "com.rerero.pocketdodger"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"

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

function Get-AndroidSdkPath {
    $candidates = @()

    if ($env:ANDROID_HOME) {
        $candidates += $env:ANDROID_HOME
    }

    if ($env:ANDROID_SDK_ROOT) {
        $candidates += $env:ANDROID_SDK_ROOT
    }

    if ($env:LOCALAPPDATA) {
        $candidates += Join-Path $env:LOCALAPPDATA "Android\Sdk"
    }

    return Resolve-FirstExistingPath -CandidatePaths $candidates -Description "Android SDK"
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

function Wait-ForDevice {
    param([int]$TimeoutSeconds)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        $devices = @(Get-ConnectedDeviceIds)

        if ($script:TargetDeviceId) {
            if ($devices -contains $script:TargetDeviceId) {
                return $script:TargetDeviceId
            }
        } elseif ($devices.Count -gt 0) {
            $script:TargetDeviceId = $devices[0]
            return $script:TargetDeviceId
        }

        Start-Sleep -Seconds 2
    }

    throw "No Android device became available within $TimeoutSeconds seconds."
}

function Invoke-AdbTarget {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    if ($script:TargetDeviceId) {
        & $script:AdbPath -s $script:TargetDeviceId @Arguments
    } else {
        & $script:AdbPath @Arguments
    }
}

function Invoke-AdbTargetChecked {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $output = Invoke-AdbTarget -Arguments $Arguments
    $exitCode = $LASTEXITCODE
    $output | Out-Host

    if ($exitCode -ne 0) {
        throw "$Description failed with exit code $exitCode."
    }
}

function Wait-ForAndroidBoot {
    param([int]$TimeoutSeconds)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        $bootCompleted = ""

        try {
            $bootCompleted = ((Invoke-AdbTarget -Arguments @("shell", "getprop", "sys.boot_completed")) | Select-Object -First 1).Trim()
        } catch {
            $bootCompleted = ""
        }

        if ($bootCompleted -eq "1") {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "Android device '$script:TargetDeviceId' did not finish booting within $TimeoutSeconds seconds."
}

function Invoke-UnityAndroidBuild {
    param([int]$TimeoutSeconds)

    if (-not (Test-Path $UnityPath)) {
        throw "Unity Editor was not found at '$UnityPath'."
    }

    $logDir = Join-Path $RepoRoot ".unity-smoke-logs"
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null

    $logPath = Join-Path $logDir "pocketdodger-run-build.log"
    $startedAt = (Get-Date).ToUniversalTime().AddSeconds(-5)

    Write-Host "Building PocketDodger debug APK..."
    & $UnityPath -batchmode -quit -projectPath $ProjectPath -buildTarget Android -executeMethod Thkim.PocketDodger.Editor.BuildPipeline.BuildAndroidDebug.BuildApk -logFile $logPath

    $lastExitCodeVariable = Get-Variable -Name LASTEXITCODE -Scope Global -ErrorAction SilentlyContinue
    if ($lastExitCodeVariable -and $null -ne $lastExitCodeVariable.Value -and $lastExitCodeVariable.Value -ne 0) {
        throw "Unity build command failed with exit code $($lastExitCodeVariable.Value). See $logPath"
    }

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        $hasSuccessLog = $false

        if (Test-Path $logPath) {
            $hasSuccessLog = [bool](Select-String -Path $logPath -Pattern "Android debug build succeeded|Build Finished, Result: Success" -Quiet)
        }

        if ($hasSuccessLog -and (Test-Path $ApkPath)) {
            $apk = Get-Item $ApkPath
            if ($apk.LastWriteTimeUtc -ge $startedAt) {
                Write-Host "Build complete: $ApkPath"
                return
            }
        }

        Start-Sleep -Seconds 2
    }

    throw "Timed out waiting for APK build completion. See $logPath"
}

$sdkPath = Get-AndroidSdkPath
$script:AdbPath = Resolve-FirstExistingPath -CandidatePaths @(
    (Join-Path $sdkPath "platform-tools\adb.exe")
) -Description "adb"
$emulatorPath = Resolve-FirstExistingPath -CandidatePaths @(
    (Join-Path $sdkPath "emulator\emulator.exe")
) -Description "Android Emulator"
$script:TargetDeviceId = $DeviceId

if ($Build -or -not (Test-Path $ApkPath)) {
    Invoke-UnityAndroidBuild -TimeoutSeconds $BuildTimeoutSeconds
}

if (-not (Test-Path $ApkPath)) {
    throw "APK was not found at '$ApkPath'. Re-run with -Build or build it from Unity first."
}

$connectedDevices = @(Get-ConnectedDeviceIds)
if (-not $script:TargetDeviceId -and $connectedDevices.Count -eq 0) {
    Write-Host "Starting emulator '$AvdName' with GPU backend '$Gpu'..."
    Start-Process -FilePath $emulatorPath -ArgumentList @(
        "-avd", $AvdName,
        "-gpu", $Gpu,
        "-netdelay", "none",
        "-netspeed", "full",
        "-no-snapshot-load"
    )
} elseif (-not $script:TargetDeviceId) {
    $script:TargetDeviceId = $connectedDevices[0]
    Write-Host "Using connected Android device '$script:TargetDeviceId'."
} else {
    Write-Host "Using requested Android device '$script:TargetDeviceId'."
}

Wait-ForDevice -TimeoutSeconds $BootTimeoutSeconds | Out-Null
Write-Host "Waiting for Android boot on '$script:TargetDeviceId'..."
Wait-ForAndroidBoot -TimeoutSeconds $BootTimeoutSeconds

Write-Host "Installing APK..."
Invoke-AdbTargetChecked -Arguments @("install", "-r", $ApkPath) -Description "APK install"

Write-Host "Launching $PackageName..."
Invoke-AdbTargetChecked -Arguments @("shell", "monkey", "-p", $PackageName, "1") -Description "App launch"

Write-Host "PocketDodger is running on '$script:TargetDeviceId'."
