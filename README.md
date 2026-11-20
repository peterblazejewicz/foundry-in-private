# foundry-in-private

A two-board Arduino VENTUNO Q edge inspection cell, governed from Microsoft Foundry in Azure. Hands-on practice for AI-103.

- Scope and the success demo: [`docs/intent/inspection-cell.md`](docs/intent/inspection-cell.md)
- Architecture: [`docs/architecture/`](docs/architecture/)
- Rules for people and agents: [`AGENTS.md`](AGENTS.md)

## Quick start (dev machine)

```sh
uv sync --all-packages
uv run pre-commit install
uv run pytest
dotnet test --solution screens/Screens.slnx
dotnet run --project screens/src/Screen.B   # desktop window; on a board: --drm
```

Requirements: uv, .NET SDK 10 (see `global.json`), git.

## Pushing to `main` (owner only)

Agents never push `main` (AGENTS.md hard rule 7). The pre-commit hook `no-commit-to-branch` refuses a commit on `main`. The owner pushes `main` by hand:

```sh
git switch -c main    # first time, from the feature branch tip
git push -u origin main
```
