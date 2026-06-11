# Merge Guardrails

This repository is configured so Codex can prepare and review changes, but must
not merge them.

## Policy

- Codex may create branches, commits, PRs, and reviews when requested.
- Codex must not run `gh pr merge`, `git merge`, direct pushes to `main` or
  `master`, or GitHub API calls that can bypass the normal PR UI.
- A human must merge PRs in GitHub after review requirements pass.
- If Codex is asked to merge, it should refuse briefly and point to this file.

## Local Guards

- `.codex/rules/no-merge.rules` blocks known merge and protected-branch update
  commands in trusted Codex sessions.
- `.githooks/pre-push` blocks direct local pushes to `main` and `master`.
- `.githooks/pre-merge-commit` and `.githooks/pre-commit` block local merge
  commits.
- Local git config should point to the hook directory:

```powershell
git config core.hooksPath thkim/.githooks
git config pull.ff only
git config merge.ff only
```

## GitHub Guards

Use a repository ruleset or branch protection rule for the default branch:

- target: `main` or the default branch
- require a pull request before merging
- require at least one approving review
- require approval from someone other than the last pusher
- dismiss stale approvals on new commits
- require conversation resolution
- block force pushes and branch deletion
- do not add Codex/current automation accounts to bypass lists

For a single-person repository, requiring one approval from someone other than
the last pusher intentionally blocks self-merges until another reviewer with
write access approves.
