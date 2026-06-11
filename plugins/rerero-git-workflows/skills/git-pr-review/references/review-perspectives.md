# Review Perspectives

Use these perspectives selectively. Do not force every category into every
review; pick the ones the diff can realistically affect.

## Correctness And Regression

- Changed control flow, state transitions, nullability, error handling, retry
  behavior, and boundary conditions.
- User-visible behavior differences not described in the PR.
- Data migration, serialization, persistence, or compatibility risks.

## Tests And Verification

- Missing coverage for the changed behavior.
- Tests that assert implementation details rather than behavior.
- Flaky or environment-dependent verification.
- CI failures or skipped checks that block merge confidence.

## Architecture And Maintainability

- New coupling across ownership boundaries.
- Duplicated logic that will diverge.
- Public API or serialized field changes without migration path.
- Overly broad abstractions added before the need is clear.

## Security And Privacy

- Secrets, credentials, tokens, signing files, or service accounts.
- New network calls, permissions, SDKs, telemetry, identifiers, location, or
  sensitive user data.
- Debug-only access paths leaking into release behavior.

## Performance

- Hot-path allocations, repeated lookups, blocking I/O, unnecessary async churn,
  N+1 queries, or expensive rendering paths.
- Memory, startup time, frame time, battery, thermal, and build-size impact.

## Unity

- `.meta` consistency.
- Scene/prefab YAML churn and merge risks.
- Editor-only code in runtime assemblies.
- Package version and lock-file changes.
- Asset import settings and large binary assets.

## Android Mobile Release

- AAB vs APK for release.
- IL2CPP and ARM64.
- Target API level.
- Signing and secret handling.
- Manifest permissions and third-party SDK declarations.
- Native plugin support for 16 KB page-size devices.
