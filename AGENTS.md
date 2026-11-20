# AGENTS.md — foundry-in-private

A two-board VENTUNO Q inspection cell with an Azure control plane. It is also hands-on practice for AI-103.

Read first:
- `docs/intent/inspection-cell.md`: scope, the success demo, out of scope.
- `docs/architecture/VQ-ventuno-q-architecture-v*.md`: decisions (D-table), interfaces (§7), behaviour (§6).

## How to write

- Write prose in ASD-STE100 style: short sentences, active voice, one instruction per sentence.
- Keep it short. Give the result first. Do not repeat what the owner said. No filler.
- Put a 2–6 word label next to an identifier at its first mention, for example `C1 (Foundry project with Bicep/azd)`.
- Design in the CUPID spirit: composable, Unix philosophy (one job each), predictable, idiomatic, domain-based names.

## Hard rules

Stop and ask the owner before you break one of these.

1. **Boards.** Use the `board-access` skill. Read `~/README.txt` on the board before any change. Log every change there with a revert line. Ask before a reboot, a package install, or a stop of another appliance.
2. **Actuation.** The servo moves only through `mcu.servo` with an approved request (§6.3). Tests use a fake MCU. Do not weaken the §6.3 approval rules or the MCU limits.
3. **Azure.** Ask before `azd up`, `azd provision`, `azd deploy`, `azd down`, or any action that creates, changes or deletes a resource. Put no keys, tokens or connection strings in the repo. Use managed identity and Key Vault.
4. **Contracts first.** A change to a §7 object starts in `contracts/`. Then regenerate the Python and C# models (`contract-change` skill). Do not edit generated models by hand.
5. **Quality bar.** Do not add `# type: ignore`, `# noqa`, `#pragma warning disable`, `[SuppressMessage]`, skipped tests, or lower thresholds without a written reason in the same diff.
6. **Knowledge base.** The rules of `D:\develop\CLAUDE.md` and `kb/SCHEMA.md` apply. Get board, NPU and device facts from the KB (`kb-lookup`). Do not copy them here. Write measurements to `docs/experiments/`, then promote them (`kb-promote`).
7. **Git.**
   - Work on a feature branch (`feat/*`, `fix/*`, `chore/*`, `docs/*`, `lab/*`). Never commit to `main`.
   - Never push `main`. Never force-push. Never merge.
   - Ask the owner before each push of a feature branch. After the push, you may open a PR (`gh pr create`).
   - The owner merges.

## Layout

| Path | Contents | Language |
|---|---|---|
| `azure.yaml`, `infra/` | Control plane (Bicep, azd). Created in C1 (Foundry project with Bicep/azd) | Bicep |
| `contracts/` | §7 objects as JSON Schema (and `.proto` if the spec picks gRPC) | — |
| `edge/shared` | Contract models, telemetry, config for both boards | Python |
| `edge/board_a` | Capture, detector (ORT-QNN), image store, `vision.*` MCP | Python |
| `edge/board_b` | Cell controller, agent, offline state machine, findings, `mcu.*` MCP | Python |
| `screens/` | Avalonia DRM thin clients `Screen.A`, `Screen.B`, shared `Screen.Core` | C# |
| `firmware/mcu/` | STM32H5 sketch: servo limits, e-stop | Arduino/Zephyr |
| `tools/replay/` | H1 (two-node cell) replay harness, C6 (image generation) synthetic defects | Python |
| `labs/` | Side labs for exam coverage only | any |
| `models/manifest.json` | Model files: name, source, sha256. The files are not in git | — |
| `docs/` | Architecture, intent, specs, `experiments/` | Markdown |

## Commands

| Task | Command |
|---|---|
| Python setup | `uv sync --all-packages` |
| Python checks | `uv run ruff check . && uv run ruff format --check . && uv run pyright && uv run pytest` |
| .NET build and test | `dotnet build screens/Screens.slnx && dotnet test --solution screens/Screens.slnx` |
| .NET format | `dotnet format screens/Screens.slnx` |
| Screen publish (board) | `dotnet publish screens/src/Screen.B/Screen.B.csproj -c Release -r linux-arm64 --self-contained false` |
| Hooks | `uv run pre-commit install` (once), `uv run pre-commit run --all-files` |

## Large files

Models, datasets and images are not in git and not in LFS. A manifest gives the name, source and sha256 of each file. Blob is the canonical copy of the datasets.

## Skills and subagents

| Name | Kind | Job |
|---|---|---|
| `board-access` | skill | SSH to a board under the `~/README.txt` protocol |
| `edge-deploy` | skill | rsync + `uv sync --frozen` + screen publish; systemd units; no Docker |
| `npu-bringup` | skill | NPU bring-up steps; facts come from the KB |
| `azd-cycle` | skill | Region and quota check, cost note, `azd up` / `azd down` |
| `contract-change` | skill | Edit `contracts/`, regenerate, run contract tests |
| `replay-run` | skill | H1 (two-node cell) replay on a pinned dataset manifest |
| `experiment-log` | skill | `docs/experiments/` entry; spike mode for S-items |
| `demo-check` | skill | Verify the success demo against the live cell |
| `arch-decision` | skill | Change the architecture doc: D-table, semver, changelog |
| `ai103-objective-map` | skill | Tag labs and PRs with AI-103 domains; keep the objectives snapshot |
| `safety-reviewer` | subagent | Read-only review of a diff against §6.3 and the MCU limits |
| `infra-reviewer` | subagent | Read-only review of Bicep and azd: RBAC, identity, cost, teardown |
| `board-operator` | subagent | Bring-up work on a board through SSH only |

## Azure and Microsoft code

Check Azure SDK and API use against the current docs (`microsoft-docs`, `microsoft-code-reference`) before you write the code.
