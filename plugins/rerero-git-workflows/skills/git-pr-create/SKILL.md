---
name: git-pr-create
description: Draft and create GitHub pull requests from local Git branches using git and GitHub CLI. Use when Codex is asked to prepare a PR, create a PR, draft a PR title/body, summarize branch changes for review, check whether a branch is ready for PR, or update an existing PR description.
---

# Git PR Create

## Overview

Use this skill to turn local branch changes into a clear, reviewable GitHub PR.
Prefer accurate context gathering over generic PR text.

## Workflow

1. Gather context.
   - Run `scripts/collect_pr_context.py` when available.
   - Also read local repo guidance such as `AGENTS.md`, `docs/PR_REVIEW_CHECKLIST.md`,
     `.github/pull_request_template.md`, or `docs/CONVENTIONS.md` when present.
2. Identify the base branch.
   - Prefer the repository default branch from `gh repo view`.
   - Fall back to `origin/HEAD`, then `main`, then `master`.
3. Inspect the branch.
   - Use `git status --short --branch`.
   - Review commits with `git log --oneline <base>..HEAD`.
   - Review changed files with `git diff --name-status <base>...HEAD`.
   - Review meaningful diffs before writing the PR body.
4. Check for an existing PR for the current branch.
   - Use `gh pr view --json number,title,url,state,isDraft,baseRefName,headRefName`.
   - Update an existing PR instead of creating a duplicate.
5. Prepare the PR.
   - Make the title concise and imperative.
   - Use the repo's PR template if it exists.
   - Include verification actually performed. Do not invent tests.
   - Call out known gaps and risks directly.
6. Create or update the PR.
   - If the user asked only for a draft, output the proposed title/body.
   - If the user explicitly asked to create the PR, use `gh pr create`.
   - If readiness is uncertain, prefer `--draft`.
   - Never merge PRs. Do not run `gh pr merge` or GitHub merge API calls, even
     if the user asks from this skill workflow.
   - Do not push, force-push, rebase, amend, or create commits unless the user
     requested that work or it is clearly required and safe.

## PR Body Shape

Use this structure unless the repository has a PR template:

```markdown
## Summary

- ...

## Changes

- ...

## Verification

- [x] ...
- [ ] Not run: ...

## Risks / Notes

- ...
```

For Unity/mobile projects, include asset, scene, Android build, and device-test
notes when relevant.

## Commands

Common commands:

```powershell
git status --short --branch
git log --oneline <base>..HEAD
git diff --stat <base>...HEAD
git diff --name-status <base>...HEAD
gh pr view --json number,title,url,state,isDraft,baseRefName,headRefName
gh pr create --base <base> --head <branch> --title "<title>" --body-file <file>
```

Use `--draft` when the PR should be opened for early review.

## Output

When finished, report:

- PR URL or that only a draft was prepared.
- Base and head branches.
- Tests/checks actually run.
- Confirmation that no merge was performed.
- Any uncommitted files, missing pushes, or known gaps.
