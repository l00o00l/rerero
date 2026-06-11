#!/usr/bin/env python3
"""Collect GitHub PR or local branch context for review."""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path


def run(args: list[str], cwd: Path) -> tuple[int, str, str]:
    try:
        proc = subprocess.run(
            args,
            cwd=cwd,
            text=True,
            encoding="utf-8",
            errors="replace",
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            check=False,
        )
    except FileNotFoundError:
        return 127, "", f"Command not found: {args[0]}"
    return proc.returncode, proc.stdout.strip(), proc.stderr.strip()


def first_success(commands: list[list[str]], cwd: Path) -> str:
    for command in commands:
        code, out, _ = run(command, cwd)
        if code == 0 and out:
            return out
    return ""


def default_branch(cwd: Path) -> str:
    branch = first_success(
        [
            ["gh", "repo", "view", "--json", "defaultBranchRef", "-q", ".defaultBranchRef.name"],
            ["git", "symbolic-ref", "--short", "refs/remotes/origin/HEAD"],
        ],
        cwd,
    )
    if branch.startswith("origin/"):
        return branch.removeprefix("origin/")
    return branch or "main"


def section(title: str, body: str) -> None:
    print(f"\n## {title}\n")
    print(body if body else "_No output._")


def print_pr_json(raw: str) -> None:
    try:
        pr = json.loads(raw)
    except json.JSONDecodeError:
        section("PR Metadata", raw)
        return

    commits = pr.get("commits") or []
    labels = pr.get("labels") or []
    lines = [
        f"- PR: #{pr.get('number')} {pr.get('title')}",
        f"- URL: {pr.get('url')}",
        f"- State: {pr.get('state')}",
        f"- Draft: {pr.get('isDraft')}",
        f"- Author: {(pr.get('author') or {}).get('login')}",
        f"- Base: {pr.get('baseRefName')}",
        f"- Head: {pr.get('headRefName')}",
        f"- Mergeable: {pr.get('mergeable')}",
        f"- Review decision: {pr.get('reviewDecision')}",
        f"- Changed files: {pr.get('changedFiles')}",
        f"- Additions/deletions: +{pr.get('additions')} / -{pr.get('deletions')}",
        "- Labels: " + ", ".join(label.get("name", "") for label in labels),
        "- Commits:",
    ]
    lines.extend(f"  - {commit.get('oid', '')[:12]} {commit.get('messageHeadline', '')}" for commit in commits)
    section("PR Metadata", "\n".join(lines))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("target", nargs="?", help="PR number, URL, or branch. Defaults to current branch PR.")
    parser.add_argument("--cwd", default=".", help="Repository path.")
    args = parser.parse_args()

    cwd = Path(args.cwd).resolve()
    code, root, err = run(["git", "rev-parse", "--show-toplevel"], cwd)
    if code != 0:
        print(f"Not a Git repository: {err}", file=sys.stderr)
        return code
    cwd = Path(root)

    print("# Review Context")
    print(f"\n- Repository: `{cwd}`")
    _, status, _ = run(["git", "status", "--short", "--branch"], cwd)
    section("Status", status)

    target = args.target
    view_cmd = [
        "gh",
        "pr",
        "view",
        *( [target] if target else [] ),
        "--json",
        "number,title,url,state,isDraft,author,baseRefName,headRefName,mergeable,reviewDecision,additions,deletions,changedFiles,commits,labels",
    ]
    code, out, pr_err = run(view_cmd, cwd)
    if code == 0 and out:
        print_pr_json(out)
        checks_cmd = ["gh", "pr", "checks", *( [target] if target else [] )]
        _, checks, checks_err = run(checks_cmd, cwd)
        section("Checks", checks or checks_err)
        names_cmd = ["gh", "pr", "diff", *( [target] if target else [] ), "--name-only"]
        _, names, names_err = run(names_cmd, cwd)
        section("Changed Files", names or names_err)
        print("\n## Next Commands\n")
        target_text = target or ""
        print(f"- Full patch: `gh pr diff {target_text} --patch`".replace("  ", " "))
        print(f"- View PR: `gh pr view {target_text} --web`".replace("  ", " "))
        return 0

    section("GitHub PR", pr_err or "No GitHub PR found; falling back to local branch diff.")
    base = default_branch(cwd)
    upstream = f"origin/{base}"
    code, _, _ = run(["git", "rev-parse", "--verify", upstream], cwd)
    compare = f"{upstream}...HEAD" if code == 0 else "HEAD"
    commit_range = f"{upstream}..HEAD" if code == 0 else "HEAD"
    print(f"\n- Local compare range: `{compare}`")

    for title, command in (
        ("Commits", ["git", "log", "--oneline", "--decorate", commit_range]),
        ("Diff Stat", ["git", "diff", "--stat", compare]),
        ("Changed Files", ["git", "diff", "--name-status", compare]),
    ):
        _, cmd_out, cmd_err = run(command, cwd)
        section(title, cmd_out or cmd_err)

    print("\n## Next Commands\n")
    print(f"- Full patch: `git diff {compare}`")
    print(f"- File patch: `git diff {compare} -- <path>`")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
