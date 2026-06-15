# Android Mobile Development Guide

This guide applies to Android-targeted Unity games in this workspace. iOS builds
require macOS and Xcode and are not covered by the Windows local toolchain.

## Baseline Sources

- Unity Android build process: https://docs.unity3d.com/6000.4/Documentation/Manual/android-BuildProcess.html
- Unity Android Player settings: https://docs.unity3d.com/6000.4/Documentation/Manual/class-PlayerSettingsAndroid.html
- Android App Bundle: https://developer.android.com/guide/app-bundle
- Google Play target API requirements: https://developer.android.com/google/play/requirements/target-sdk
- Android 16 KB page sizes: https://developer.android.com/guide/practices/page-sizes
- Android game development with Unity: https://developer.android.com/games/engines/unity/start-in-unity

## Local Toolchain

Expected local tools:

```powershell
unity --version
adb version
java -version
gh --version
```

Unity Editor:

```text
C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe
```

Android SDK, NDK, and OpenJDK should come from the Unity Android Build Support
module unless there is a documented reason to override them.

## Android Player Settings

Release-oriented defaults:

- Package name: use a stable reverse-DNS id, for example `com.<studio>.<game>`.
- Build format for Google Play: Android App Bundle (`.aab`).
- Scripting backend: IL2CPP for release builds.
- Target architecture: ARM64 for release builds.
- Target API level: meet the current Google Play requirement.
- Minimum API level: choose based on target device support, not by default.
- App category: Game.
- Internet permission: Auto unless networking is required.
- Write external storage permission: avoid unless there is a real requirement.
- Development Build: off for release builds.
- Script debugging: off for release builds.

Review these settings in every release PR.

## Debug Builds

Use APK for quick local device iteration. Keep debug builds separate from
release artifacts.

Typical flow:

```powershell
adb devices
adb install -r <path-to-debug.apk>
adb logcat
```

Do not use Unity Remote as the primary device validation path. Test real builds
on real devices whenever possible.

## Local Run Scripts

Every runnable game should provide a one-command local run script inside that
game's directory. For Android emulator smoke runs, use this naming pattern:

```text
<GameName>/run.cmd
<GameName>/scripts/run-emulator.ps1
```

The script should:

- find the local Android SDK or fail with a clear message
- find Unity from `-UnityEditorPath`, `UNITY_EDITOR_PATH`, or the documented
  default path when rebuilding
- start the expected AVD when no Android device is connected
- use documented emulator options required by this machine
- install the debug APK
- launch the package
- optionally rebuild the APK with a `-Build` switch

PocketDodger example:

```powershell
.\PocketDodger\run
.\PocketDodger\run -Build
```

On this machine, PocketDodger uses `-gpu swiftshader_indirect` for emulator
smoke tests because the default emulator GPU path rendered a black screen during
local verification.

## Local Test Scripts

Every tested game should provide a one-command local test script inside that
game's directory:

```text
<GameName>/test.cmd
<GameName>/scripts/run-tests.ps1
```

The script should:

- find Unity from `-UnityEditorPath`, `UNITY_EDITOR_PATH`, or the documented
  default path
- run Edit Mode and Play Mode tests by default
- write logs and XML results under the ignored game `Logs/` directory
- fail if Unity exits nonzero
- fail if the expected XML result is missing
- fail if the XML result is not `Passed`

When using Unity Test Runner from CLI, do not pass `-quit` with `-runTests`.
The Test Runner exits Unity after writing results.

PocketDodger example:

```powershell
.\PocketDodger\test
.\PocketDodger\test -Mode EditMode
.\PocketDodger\test -Mode PlayMode
```

## Shared Assets

Reusable assets that should be shared across games belong in the workspace-level
Unity package:

```text
C:\WorkSpace\rerero\shared-unity\com.rerero.shared-assets
```

Game projects should reference it as a local package instead of copying assets:

```json
"com.rerero.shared-assets": "file:../../../shared-unity/com.rerero.shared-assets"
```

Keep third-party source notes and license files under the package `LICENSES/`
folder before using external assets in a game.

## Release Builds

Release builds must produce an AAB for Google Play.

Checklist:

- Version code increased.
- Version name is correct.
- IL2CPP enabled.
- ARM64 enabled.
- Target API level satisfies Google Play.
- Keystore is provided through secure local/CI configuration, not Git.
- Development Build and Script Debugging disabled.
- App Bundle generated and archived outside source control.
- Native SDKs and plugins checked for Android 16 KB page-size compatibility.
- A smoke install/run test is done on at least one physical Android device when
  a runnable APK is available.

## Signing And Secrets

Never commit:

- `.keystore`
- `.jks`
- signing passwords
- Play Console credentials
- service account JSON
- API keys or SDK secrets

Use local ignored files or CI secret storage.

## Performance Budget

Mobile work should be reviewed for:

- frame time and visible stutter
- garbage collection allocations
- CPU main-thread spikes
- GPU overdraw and shader cost
- texture memory and compression
- audio memory
- build size and first install size
- thermal behavior on real devices

Do not optimize blindly. Profile first, then make the smallest change that
addresses the measured bottleneck.

## Asset Guidelines

- Prefer ASTC texture compression for modern Android targets when the device
  target supports it; document fallback strategy when broad compatibility is
  required.
- Keep texture max size as low as visual quality allows.
- Avoid uncompressed large textures or audio in mobile builds.
- Prefer streaming/compressed audio for long music and memory-resident clips
  only for short sound effects.
- Treat large asset additions as PR review points even when code is unchanged.

## Native Plugins And SDKs

Every Android native plugin or third-party SDK PR should state:

- why the SDK is needed
- Android permissions added
- activities, services, receivers, or providers added to the manifest
- native `.so` architectures included
- whether the SDK supports 16 KB page-size devices
- privacy/data collection implications

## CLI Build Direction

Project-specific build scripts should eventually live under `Tools/` or
`Assets/_Project/Editor/Build/`. Until those exist, use Unity batchmode for
smoke checks:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath <project-path> -buildTarget Android -logFile <log-path>
```

Add a dedicated build method once the Unity project exists.
