---
name: contract-change
description: Change a cell interface object (§7 - vision.inspect result, finding, approval card/decision, actuation command/ack, IPC messages) - edit the schema in contracts/, regenerate the Python and C# models, run the contract tests. Use whenever a field, enum value or message changes.
---

# contract-change

Contracts first (hard rule 4). The schema is the only source.

## Steps

1. Edit the schema in `contracts/` (JSON Schema; `.proto` if the spec picks gRPC for IPC).
2. Bump the schema version in its `$id` or package: minor for an added optional field, major for a removed or renamed field or a changed enum.
3. Regenerate:
   - Python models → `edge/shared/src/cell_shared/contracts/` (generated, do not edit).
   - C# models → `screens/src/Screen.Core/Contracts/` (generated, do not edit).
   The generator commands are in `contracts/README.md`.
4. Run the contract tests: each `contracts/examples/*.json` must validate, and must round-trip through the Python and C# models.
5. Update §7 of the architecture doc with `arch-decision` when the change is visible to another node.
6. Check the safety rules still hold when the change touches §7.4 (approval) or §7.5 (actuation): run the `safety-reviewer` subagent.

## Never

- Never hand-edit a generated model.
- Never remove a correlation key (`inspection_id`, `trace_id`, `otel_agent_id`).
