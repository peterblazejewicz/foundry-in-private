# contracts/

The cell's interface objects (architecture §7) as JSON Schema. This folder is the only source.

| Object | Schema (to write) | Producer → consumer |
|---|---|---|
| `vision.inspect` result | `vision-inspect-result.schema.json` | board A → board B |
| Finding | `finding.schema.json` | board B → SQLite, AI Search |
| Approval card / decision | `approval.schema.json` | board B ⇄ screen B |
| Actuation command / ack | `actuation.schema.json` | board B ⇄ MCU |
| Screen IPC messages | to be chosen in the spec (JSON Schema or `.proto`) | Python ⇄ Avalonia |

- `examples/`: one valid JSON file per object. Contract tests validate them and round-trip them.
- Generated Python models: `edge/shared/src/cell_shared/contracts/`. Generated C# models: `screens/src/Screen.Core/Contracts/`.
- Generator commands: to be fixed in the spec. Use the `contract-change` skill for every change.
