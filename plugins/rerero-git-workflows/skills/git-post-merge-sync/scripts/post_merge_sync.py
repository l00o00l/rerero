#!/usr/bin/env python3
"""Sync a local checkout after a GitHub PR has already been merged."""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path


def run(args: list[str], *, cwd: Path, check: bool = True) -> subprocess.CompletedProcess[str]:
    print("+ " + " ".join(args))
    result = subprocess.run(args, cwd=cwd, text=True, capture_output=True)
    if result.stdout:
        print(result.stdout, end="")
    if result.stderr:
        print(result.stderr, end="", file=sys.stderr)
    if check and result.returncode != 0:
        raise SystemExit(result.returncode)
    return result


def parse_json_command(args: list[str], *, cwd: Path) -> dict:
    result = run(args, cwd=cwd, check=True)
    try:
        return json.loads(result.stdout)
    except json.JSONDecodeError as exc:
        raise SystemExit(f"Failed to parse JSON from {' '.join(args)}: {exc}") from exc


def require_clean_tree(cwd: Path) -> None:
    status = run(["git", "status", "--porcelain"], cwd=cwd, check=True).stdout.strip()
    if status:
        raise SystemExit("Working tree is not clean. Commit, stash, or discard changes before syncing.")


def resolve_base(pr: str | None, cwd: Path) -> tuple[str, dict | None]:
    if pr:
        pr_data = parse_json_command(
            [
                "gh",
                "pr",
                "view",
                pr,
                "--json",
                "number,state,mergedAt,baseRefName,headRefName,url",
            ],
            cwd=cwd,
        )
        if pr_data.get("state") != "MERGED" or not pr_data.get("mergedAt"):
            raise SystemExit(f"PR is not merged: {pr_data.get('url', pr)}")
        return pr_data["baseRefName"], pr_data

    repo = parse_json_command(["gh", "repo", "view", "--json", "defaultBranchRef"], cwd=cwd)
    return repo["defaultBranchRef"]["name"], None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pr", help="Merged PR number, URL, or branch accepted by gh pr view.")
    parser.add_argument(
        "--repo",
        default=".",
        help="Repository working directory. Defaults to the current directory.",
    )
    args = parser.parse_args()

    cwd = Path(args.repo).resolve()
    run(["git", "rev-parse", "--show-toplevel"], cwd=cwd, check=True)
    require_clean_tree(cwd)

    base, pr_data = resolve_base(args.pr, cwd)
    if pr_data:
        print(f"Verified merged PR: {pr_data['url']}")

    run(["git", "switch", base], cwd=cwd, check=True)
    run(["git", "pull", "--ff-only", "origin", base], cwd=cwd, check=True)
    run(["git", "status", "--short", "--branch"], cwd=cwd, check=True)
    run(["git", "log", "--oneline", "--decorate", "-3"], cwd=cwd, check=True)
    print("No merge was performed. No protected-branch push was performed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
