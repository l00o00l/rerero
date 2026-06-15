# Implementation Planning Guide

This guide defines the standard format for implementation plans in this Unity
game workspace. Use it when starting a new game prototype or a major gameplay
feature.

Implementation plans translate concept and design decisions into buildable,
reviewable work. They should not duplicate the full concept document. Link to
the relevant concept and planning documents, then focus on execution order,
verification, PR boundaries, and known risks.

Every implementation plan must include verification and test planning. Do not
start implementation from a plan that only lists build tasks without explaining
how correctness, playability, build health, and manual inspection gaps will be
checked.

## Location

Prefer a game-local plan:

```text
<GameName>/docs/PLAN.md
```

Examples:

```text
PocketDodger/docs/PLAN.md
DreamLaundromat/docs/PLAN.md
```

Use repo-level planning docs only for shared workflow, tooling, or repository
policy.

## Language

Follow the documentation language policy in `AGENTS.md`.

- Plan body text should be Korean by default.
- Section headings, table headers, game working names, technical terms, code
  identifiers, paths, commands, and API names may stay in English when clearer.
- Meta/process docs like this guide are written in English.

## Required Sections

Use this structure unless the change is too small to justify a full plan.

### Summary

State what is being built and why this implementation pass exists.

### Planning References

Link to the concept, pre-production, design, or previous implementation docs
that constrain the work.

### Prototype Goal

State the core hypothesis the prototype must prove. This should be testable,
not a broad product ambition.

### Scope

List what this implementation plan includes.

### Non-Goals

List what is explicitly excluded. This is important for keeping prototype PRs
small.

### Key Decisions

Record implementation decisions that should not keep being re-litigated during
the work, such as input model, undo policy, data format, scene strategy, or
platform target assumptions.

### Target Platforms

State the primary target platform for this implementation pass and any secondary
platforms that must remain viable. Include screen/orientation assumptions, input
model, build target, run script, and manual platform checks.

Follow `docs/UNITY_PROJECT_STRUCTURE.md` when deciding whether to add
platform-specific folders or scripts.

### Architecture

Describe the major systems, namespaces, Unity folders, and runtime data flow.
Prefer simple structures that match existing project conventions.

### Data Model

Define the gameplay data shape: level data, runtime state, ScriptableObject or
JSON direction, persistence, validation, and testable pure models.

### Scene And UI Plan

Describe the scene hierarchy, UI regions, input flow, safe area concerns, and
which objects are scene-owned versus prefab/data-owned.

### Milestones

Group work into a small number of larger milestones such as project shell,
core model, playable UI, platform smoke, tests, and polish.

### Task Breakdown

Break milestones into small task IDs such as `DL-001`.

Each task should include:

- Outputs
- Concrete work
- Verification
- Done criteria

### PR Plan

Define how work should be split across PRs. Separate these areas when
practical:

- Unity project setup
- Core rules/model code
- Level/data authoring
- Scene and UI work
- Platform input
- Platform build and smoke testing
- Tests
- Polish

### Verification And Test Plan

List the checks that should be run during implementation and PR review:

- `git status --short --branch`
- Edit Mode tests
- Play Mode tests
- Unity batchmode import/build checks
- Platform build, such as Android or Windows
- Emulator, device, or desktop smoke test
- Manual visual/game-feel checks

Do not claim checks that were not actually run.

For each meaningful system or milestone, state the expected test level:

- Pure model or rules code: focused Edit Mode tests.
- Scene/UI behavior: Play Mode tests when feasible, plus screenshot or manual
  visual checks when layout matters.
- Build or platform work: Unity batchmode import/build and target-platform build
  checks.
- Runnable platform work: emulator, real-device, or desktop smoke test, with any
  manual device or environment limitations called out.

### CLI And Manual Boundary

Separate what Codex can do from the CLI from what requires human inspection or
external UI interaction, such as Unity Game view judgment, Android device
authorization, game feel, or GitHub merge actions.

### Risks

Call out likely risks: Unity YAML churn, missing `.meta` files, serialized
reference fragility, platform UI readability, hot-path allocations, platform
build breakage, level production bottlenecks, and manual test gaps.

### Deferred Or Out Of Scope

Keep ordinary game backlog and feature ideas in the game-local plan, PR notes,
or issue tracker. Use `docs/TODO.md` only for deferred work that affects shared
workflow, repository policy, infrastructure, or future productivity across
tasks.

### First Implementation Step

State the next concrete step after the plan is accepted.

## Drafting Workflow

When creating or substantially updating an implementation plan:

1. Draft the full plan using the required sections.
2. Self-review the completed plan against the review checklist in this guide.
3. Revise the plan immediately when the self-review finds contradictions,
   missing decisions, unclear scope, weak verification, unsafe PR boundaries, or
   mismatches with the referenced concept/planning docs.
4. Confirm that the verification and test plan covers the riskiest implementation
   assumptions before asking for approval or starting code.
5. Identify decisions that require the user before implementation can safely
   start.
6. Present those user decisions clearly, with the recommended option and the
   impact of each choice when the tradeoff matters.

Do not treat the first draft as finished. The implementation plan is ready only
after the self-review pass is complete and unresolved user decisions are called
out.

## Puzzle Game Additions

For puzzle games, the implementation plan must define these before scene/UI work
dominates the project:

- Pure rules model
- Level data shape
- Move/action model
- Undo policy
- Clear and failure conditions
- Level validation path
- Focused Edit Mode tests

Avoid binding core puzzle rules tightly to `MonoBehaviour` or scene objects
before the rules model is testable.

## Task Example

```md
### DL-001 - Core Attribute Model

- Outputs:
  - `DreamAttributes`
  - `DreamFragment`
  - Edit Mode tests
- Work:
  - Represent `stain` and `moisture` as explicit attributes.
  - Support order matching against partially specified attributes.
- Verification:
  - Edit Mode tests for exact and partial order matching.
- Done criteria:
  - Rules can compare a dream fragment with an order without scene objects.
```

## Review Checklist

Before accepting an implementation plan, check:

- The plan links to the relevant concept/planning docs.
- Scope and non-goals are explicit.
- Key decisions are either made in the plan or listed as user decisions.
- Target platform assumptions are explicit.
- Work is split into reviewable PRs.
- The first PR is small enough to review safely.
- Verification and test planning covers the riskiest assumptions.
- Automated, smoke, build, and manual checks are separated clearly.
- Manual checks are not disguised as automated checks.
- Game-local backlog is separated from shared TODO items.
- The plan has been self-reviewed and revised after the initial draft.
