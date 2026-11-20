# edge/ — board code (Python 3.12)

- Each board is one package: `board_a` (perception), `board_b` (reasoning and actuation). Shared code goes in `shared` (`cell_shared`) only when both boards use it.
- `cell_shared` is pyright strict. Keep it free of board-only dependencies.
- Before work on the NPU, the camera, the screens or USB audio, load these KB notes with `kb-lookup`: `arduino-ventuno-q`, `lenovo-thinksmart-controller`, `jabra-speak2-75`. Do not copy their facts here.
- NPU rules that act on the KB facts:
  - One NPU owner per board (D2).
  - Count an NPU process as started only after its first computation. Restart it on failure.
  - A sandboxed unit needs the NPU device grants. The `npu-bringup` skill lists the steps.
- Hold CPU-heavy work (voice, ONNX on CPU) to cores 0–3 with a cgroup cpuset, not `taskset`.
- Times are UTC and timezone-aware. Correlation keys `inspection_id`, `trace_id`, `otel_agent_id` go on every log, span and object (§7).
- Board B talks to Azure. Board A sends telemetry only (D8).
- Tests run on the dev machine with fakes: fake camera (image folder), fake NPU detector, fake MCU. A test that needs a board is marked `@pytest.mark.board` and does not run in CI.
