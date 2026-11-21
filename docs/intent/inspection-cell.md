# Intent — VENTUNO Q inspection cell

*Confirmed 2026-11-20 (interview). Input for specs. Source of truth for scope until a spec replaces it.*

## Outcome

One repo holds a working two-board VENTUNO Q inspection cell. The AI-103 labs build its Azure control plane:

- C0 (region check), C1 (Foundry project with Bicep/azd), C3 (eval gate in CI), C4 (Content Safety), C5 (prompt agent with tools and MCP), C7 (Content Understanding), C8 (AI Search + Foundry IQ).
- C6 (image generation and inpainting) makes synthetic defect images for the H1 (two-node cell) image-replay harness.
- C9 Speech is the start of H2 (voice operator).

## Edge stack

- Python processes own all logic on both boards.
- Avalonia DRM thin clients draw screen A (board A, HDMI, msm) and screen B (board B, DisplayLink, evdi). They talk to Python over IPC. The IPC mechanism (Unix socket or gRPC) is open for the spec.
- Board B's local SLM is llama.cpp Hexagon (not GenieX). It writes offline explanation text only. It calls no tools.
- This changes D3 (cloud vs NPU split), D4 (languages per device) and §3.2 (runtimes) of architecture v0.7.2.

## User

The owner: builder, and operator at screen B.

## Why now

The AI-103 exam is the deadline. The cell is the goal. The exam keeps the cloud work on schedule.

## Success (the demo)

Live, no image replay:

1. Tap **Inspect** on screen B. Board A's NPU detects a defect. The servo rejects the part.
2. The finding is in Foundry IQ. The agent answers "what did we reject in the last hour?" with a citation of that finding.
3. The trace is in Application Insights across board A, board B and Foundry.
4. Disconnect Ethernet. An offline inspection (§6.2 state machine) ends in an approval card. The operator taps it.

## Constraints

- The exam date wins every trade-off. Azure spend is second: `azd up` / `azd down` teardown stays easy.
- Hardware gets a few hours a week.
- `ventuno` (10.0.0.9) becomes board B. Its appliances stop as its `~/README.txt` tells. Board A is set up from zero during the work.

## Out of scope for "done"

- Voice (H2) past the C9 Speech lab. It is necessary later in Track 2.
- H3 (contractor laptop), H0 (placement table), S9 (GenieX conformance; nice-to-have when we benchmark the board).
- AI-500 items: P2 (guardrails), P4 (A2A + chaos drills), R1 (canary), X1 (extras).
- Side labs for exam coverage only: C6 video generation; C9 Language, Translator, custom speech.

## Repo foundation

- Monorepo by deployable unit: `azure.yaml` + `infra/`, `contracts/`, `edge/{board_a,board_b,shared}`, `screens/{Screen.A,Screen.B,Screen.Core}`, `firmware/mcu/`, `tools/replay/`, `labs/`, `docs/`.
- Python: uv workspace, 3.12, `uv.lock`, ruff, pyright (strict on `edge/shared`, basic in other packages), pytest.
- C#: one `.slnx`, `global.json` (SDK 10), `Directory.Build.props` (nullable, warnings as errors, `latest-recommended`), central package management with lock files, `linux-arm64` framework-dependent publish.
- Hygiene: `.editorconfig`, `.gitattributes` (LF; CRLF for `*.ps1`/`*.cmd`; binaries marked), `.gitignore`, pre-commit (ruff, pyright, dotnet format, gitleaks).
- Large files: not in git, no LFS. Manifests with sha256. Blob is the canonical copy.
- CI: GitHub Actions, lint + types + tests on each PR. C3 eval gate later.

## Agent tooling

- `AGENTS.md` is canonical. `CLAUDE.md` imports it. Nested `AGENTS.md` in `edge/`, `screens/`, `infra/`. STE prose, CUPID spirit, minimal verbosity (project output style).
- Hard rules: board protocol; servo only through an approved `mcu.servo`; ACK for billable Azure actions; contracts first; no silent quality-bar drops; KB rules inherited; feature branches only, `main` never pushed, each feature-branch push needs owner ACK, agents may open PRs, the owner merges.
- Skills: `board-access`, `edge-deploy`, `npu-bringup`, `azd-cycle`, `contract-change`, `replay-run`, `experiment-log` (with spike mode), `demo-check`, `arch-decision`, `ai103-objective-map`.
- Subagents: `safety-reviewer`, `infra-reviewer`, `board-operator`.
- Azure skills at project scope through an in-repo marketplace (`.claude-plugin/marketplace.json`). One plugin, `cell-azure`, points at `microsoft/azure-skills` at a pinned commit and lists 5 skills only: `microsoft-foundry`, `azure-validate`, `azure-quotas`, `foundry-iq`, `cost-estimation`. It brings the Azure MCP server only (pinned to `4.0.0-beta.7`, telemetry off) and no hooks. No copies of their skill files. The Cost Management MCP is a separate local-scope server per user. Its client ID and tenant ID are not in git. Known limits: foundry-iq graph workflows do not work (they need the plugin runtime); `azure-validate` names `azure-prepare` / `azure-deploy`, which are not installed. (Changed 2026-11-20: skillOverrides does not apply to plugin skills.)

## Open for the spec

- Avalonia ↔ Python IPC: Unix socket or gRPC.
