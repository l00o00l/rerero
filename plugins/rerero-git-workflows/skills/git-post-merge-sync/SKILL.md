---
name: git-post-merge-sync
description: Sync a local Git checkout after a GitHub PR has already been merged. Use when Codex is asked to move back to the base branch after a merge, pull the latest default/base branch, verify local branch state after a PR merge, or cleanly continue after the user merged a PR in GitHub. This skill must not merge PRs or push protected branches.
---

# Git Post-Merge Sync

## Overview

Use this skill after a human has merged a PR in GitHub and the local checkout
needs to return to the base branch and fast-forward to the merged commit.

This skill is for sync only. Never run `gh pr merge`, `git merge`, direct pushes
to protected branches, or GitHub merge API calls.

## Workflow

1. Confirm the PR is already merged.
   - Prefer `gh pr view <target> --json state,mergedAt,baseRefName,headRefName,url`.
   - Stop if the PR is not `MERGED` or `mergedAt` is empty.
2. Confirm the working tree is clean.
   - Run `git status --short --branch`.
   - Stop if there are uncommitted changes unless the user explicitly asks how
     to preserve them.
3. Identify the base branch.
   - Prefer the PR `baseRefName`.
   - Fall back to `gh repo view --json defaultBranchRef`.
4. Switch to the base branch.
   - Run `git switch <base>`.
5. Pull with fast-forward only.
   - Run `git pull --ff-only origin <base>`.
   - Do not run `git merge`.
6. Verify final state.
   - Run `git status --short --branch`.
   - Run `git log --oneline --decorate -3`.
7. Treat cleanup as optional.
   - Do not delete local or remote feature branches automatically.
   - Offer cleanup separately only when it is useful.

## Script

Prefer the bundled script for the standard flow:

```powershell
python <this-skill-dir>\scripts\post_merge_sync.py --pr <number-or-url>
```

The script verifies the PR is merged, checks for a clean tree, switches to the
PR base branch, and pulls with `--ff-only`. It does not delete branches and does
not merge anything.

Resolve `<this-skill-dir>` to this skill folder inside the installed plugin or
repo checkout. On Windows, if `python` is not on `PATH`, find the installed
Python executable and invoke the same script path explicitly.

```powershell
& <python-exe> <this-skill-dir>\scripts\post_merge_sync.py --pr <number-or-url>
```

## Output

When finished, report:

- PR checked and whether it was merged.
- Base branch synced.
- Final branch and tracking status.
- Commands/checks actually run.
- Confirmation that no merge and no protected-branch push was performed.
