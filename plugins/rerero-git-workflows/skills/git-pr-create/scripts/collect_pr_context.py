#!/usr/bin/env python3
"""Collect local Git/GitHub context for drafting a PR."""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path


def run(args: list[str], cwd: Path, check: bool = False) -> tuple[int, str, str]:
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
    if check and proc.returncode != 0:
        raise RuntimeError(f"{' '.join(args)} failed: {proc.stderr.strip()}")
    return proc.returncode, proc.stdout.strip(), proc.stderr.strip()


def first_success(commands: list[list[str]], cwd: Path) -> str:
    for command in commands:
        code, out, _ = run(command, cwd)
        if code == 0 and out.strip():
            return out.strip()
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
        branch = branch.removeprefix("origin/")
    if branch:
        return branch

    for candidate in ("main", "master"):
        code, _, _ = run(["git", "rev-parse", "--verify", f"origin/{candidate}"], cwd)
        if code == 0:
            return candidate
    return "main"


def print_section(title: str, body: str) -> None:
    print(f"\n## {title}\n")
    print(body if body else "_No output._")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--base", help="Base branch name. Defaults to repository default branch.")
    parser.add_argument("--cwd", default=".", help="Repository path.")
    args = parser.parse_args()

    cwd = Path(args.cwd).resolve()
    code, root, err = run(["git", "rev-parse", "--show-toplevel"], cwd)
    if code != 0:
        print(f"Not a Git repository: {err}", file=sys.stderr)
        return code
    cwd = Path(root)

    base = args.base or default_branch(cwd)
    branch = first_success([["git", "branch", "--show-current"]], cwd)
    upstream = f"origin/{base}"
    code, _, _ = run(["git", "rev-parse", "--verify", upstream], cwd)
    compare = f"{upstream}...HEAD" if code == 0 else "HEAD"
    commit_range = f"{upstream}..HEAD" if code == 0 else "HEAD"

    print(f"# PR Context\n")
    print(f"- Repository: `{cwd}`")
    print(f"- Current branch: `{branch or '(detached)'}`")
    print(f"- Base branch: `{base}`")
    print(f"- Compare range: `{compare}`")

    for title, command in (
        ("Status", ["git", "status", "--short", "--branch"]),
        ("Commits", ["git", "log", "--oneline", "--decorate", commit_range]),
        ("Diff Stat", ["git", "diff", "--stat", compare]),
        ("Changed Files", ["git", "diff", "--name-status", compare]),
    ):
        _, out, err = run(command, cwd)
        print_section(title, out or err)

    code, out, err = run(
        [
            "gh",
            "pr",
            "view",
            "--json",
            "number,title,url,state,isDraft,baseRefName,headRefName",
        ],
        cwd,
    )
    if code == 0 and out:
        try:
            pr = json.loads(out)
            print_section(
                "Existing PR",
                "\n".join(
                    [
                        f"- #{pr.get('number')}: {pr.get('title')}",
                        f"- URL: {pr.get('url')}",
                        f"- State: {pr.get('state')}",
                        f"- Draft: {pr.get('isDraft')}",
                        f"- Base: {pr.get('baseRefName')}",
                        f"- Head: {pr.get('headRefName')}",
                    ]
                ),
            )
        except json.JSONDecodeError:
            print_section("Existing PR", out)
    else:
        print_section("Existing PR", err or "No PR detected for the current branch.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
