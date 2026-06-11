---
name: git-pr-review
description: Review GitHub pull requests and local PR-sized changes using git and GitHub CLI from multiple engineering perspectives. Use when Codex is asked to review a PR, inspect a branch before merge, analyze PR risks, review GitHub checks, or provide findings across correctness, tests, security, performance, Unity/mobile, and release readiness.
---

# Git PR Review

## Overview

Use this skill for code-review style analysis. Findings come first, ordered by
severity, with file/line references whenever possible.

## Workflow

1. Identify the review target.
   - Use a PR number, URL, branch name, or current branch.
   - If no PR exists, review the local diff against the repository default branch.
2. Gather context.
   - Run `scripts/collect_review_context.py [target]` when available.
   - Use `gh pr view`, `gh pr diff`, and `gh pr checks` for GitHub PRs.
   - Read repo guidance when present: `AGENTS.md`, `docs/PR_REVIEW_CHECKLIST.md`,
     `docs/CONVENTIONS.md`, `docs/MOBILE_ANDROID.md`.
3. Inspect the diff directly.
   - Do not rely only on PR summaries.
   - Open changed files and relevant surrounding code.
   - Compare behavior before and after the change.
4. Select review perspectives.
   - Always cover correctness/regression and test coverage.
   - Add security/privacy, performance, architecture, release, Unity, and mobile
     perspectives when the files or repo context make them relevant.
   - For complex reviews, read `references/review-perspectives.md`.
5. Produce findings.
   - Start with findings. Do not lead with a summary.
   - Include severity, file/line, impact, and suggested direction.
   - Avoid nit-only comments unless the user requested style review.
   - If no issues are found, say so clearly and list residual risks or test gaps.
6. Submit only when requested.
   - Do not run `gh pr review --approve`, `--request-changes`, or `--comment`
     unless the user explicitly asks to publish the review.
   - Never merge PRs. Do not run `gh pr merge` or GitHub merge API calls from a
     review workflow.

## Commands

Useful commands:

```powershell
git status --short --branch
gh pr view <target> --json number,title,url,state,isDraft,author,baseRefName,headRefName,mergeable,reviewDecision,additions,deletions,changedFiles,commits,labels
gh pr checks <target>
gh pr diff <target> --name-only
gh pr diff <target> --patch
```

For local branch review:

```powershell
git diff --name-status origin/main...HEAD
git diff --stat origin/main...HEAD
git diff origin/main...HEAD -- <path>
```

## Finding Format

Use this shape:

```markdown
Findings

- High: `path/file.cs:42` Short issue title.
  Explain the concrete failure mode and why it matters. Suggest the smallest
  viable fix or verification path.

Open Questions

- ...

Residual Risk / Test Gaps

- ...
```

Use `Critical`, `High`, `Medium`, or `Low`. Prefer fewer, stronger findings.

## Unity And Mobile Review

When reviewing a Unity/mobile repo, explicitly check:

- missing or unexpected `.meta` files
- scene, prefab, material, animation, and asset YAML churn
- Unity package lock changes
- serialized fields and scene references
- runtime allocations in hot paths
- mobile texture/audio/build size impact
- Android permissions, SDKs, AAB, IL2CPP, ARM64, target API, signing, and 16 KB
  page-size risk for release changes

Use the repository checklist when available.
