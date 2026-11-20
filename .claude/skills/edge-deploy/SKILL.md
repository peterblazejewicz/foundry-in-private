---
name: edge-deploy
description: Deploy board code to a VENTUNO Q - rsync the uv workspace, `uv sync --frozen` on the board, publish an Avalonia screen for linux-arm64, install or restart systemd units. No Docker. Use for any "deploy to board A/B" or "update the screen on the board" task.
---

# edge-deploy

Every deploy is a board change. Follow `board-access` first. Ask the owner before the first install of a unit.

## Python (both boards)

1. Local checks pass: `uv run ruff check . && uv run pyright && uv run pytest`.
2. Copy the workspace, not the venv:
   `rsync -az --delete --exclude .venv --exclude '**/__pycache__' --exclude .git ./ <host>:/opt/cell/`
3. On the board: `cd /opt/cell && uv sync --frozen --package <cell-board-a|cell-board-b>`.
4. Restart the unit: `sudo systemctl restart cell-<role>.service`. Check `systemctl status` and the first log lines.

## Screens (Avalonia DRM)

1. `dotnet publish screens/src/Screen.<A|B>/Screen.<A|B>.csproj -c Release -r linux-arm64 --self-contained false -o out/screen-<a|b>`
2. `rsync -az --delete out/screen-<a|b>/ <host>:/opt/cell/screens/screen-<a|b>/`
3. Restart `cell-screen-<a|b>.service`.

## systemd units

- Units live in `edge/<board>/deploy/systemd/`. Install with `sudo install -m 644` into `/etc/systemd/system/`, then `daemon-reload`.
- Run as a dedicated user, not root. Add hardening (`ProtectSystem=strict`, `NoNewPrivileges=yes`, `PrivateTmp=yes`).
- An NPU unit needs the device grants and groups. Get the exact list from `npu-bringup` and the KB.
- A screen unit picks its DRM card by driver at start (`evdi` on B, `msm_dpu` on A).
- `Restart=on-failure` with `RestartSec=`; an NPU service counts as started only after its first computation.

## After the deploy

- Append the deploy to `~/README.txt` with the revert line (the previous release directory, or `systemctl disable --now <unit>`).
- Run `demo-check` when the deploy touches the inspection path.
