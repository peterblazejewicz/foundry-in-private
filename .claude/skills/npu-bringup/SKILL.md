---
name: npu-bringup
description: Step-by-step bring-up of a Hexagon NPU workload on a VENTUNO Q (ORT + QNN EP detector on board A, llama.cpp Hexagon on board B) - device nodes, libraries, unit grants, first-computation check, restart rule. Procedure only; facts and numbers come from the KB.
---

# npu-bringup

This skill holds steps only. Facts and numbers are in the KB. Load them first:
`kb-lookup` → `wiki/devices/arduino-ventuno-q.md` (Facts, Gotchas, Measured).

## Steps

1. **Owner.** Confirm one NPU owner per board (D2). On board B, stop other NPU appliances first (`board-access`).
2. **Nodes and groups.** Check the fastrpc device nodes and the service user's groups against the KB note.
3. **Libraries.** Check the fastrpc library name the runtime opens and the `ADSP_LIBRARY_PATH` form against the KB gotchas. Make a symlink in a directory of your own if needed; never in system paths without asking.
4. **Unit grants.** Put the device grants from the KB in the unit (`DeviceAllow=`, `SupplementaryGroups=`).
5. **Start check.** A start counts as done only after the first computation (one inference or one prompt). Script it as `ExecStartPost=` or a health endpoint.
6. **Restart rule.** On a start failure, restart. Count the failures; log them.
7. **Sharing.** Do not run QNN and llama.cpp Hexagon on the NPU at the same moment. If both are needed, take turns (S15, NPU sharing).
8. **Thermals.** Read the NPU temperature in telemetry. Pause NPU work when it is hot (§12 risks).
9. **Record.** Write the run to `docs/experiments/` (`experiment-log`). New facts or numbers go to the KB with `kb-promote`.

## Board A (ORT + QNN EP)

- Install `onnxruntime-qnn` (Python wheel, linux-aarch64) in the board_a package.
- List the execution providers. QNN must be there with the HTP backend before any detector work.
- Record model name, version, runtime and latency for each run (§7.2 `detector` object).

## Board B (llama.cpp Hexagon)

- Use an Arduino `llamacpp/<date>` Hexagon build listed in the KB as good. Record the build tag.
- Size the context from `/props` `n_ctx`. Do not trust a default.
- The SLM writes explanation text only. It gets no tools (D6).
