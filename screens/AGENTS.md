# screens/ — Avalonia DRM thin clients (C#, .NET 10)

- A screen client shows state and sends taps. It holds no cell logic. Python owns all logic.
- `Screen.Core` holds the shared start code, the IPC client and the view models. `Screen.A` and `Screen.B` are entry points only.
- On a board: `--drm`, a single-view lifetime, no Window. On the dev machine: a desktop window.
- Pick the DRM card by driver (`evdi` for the ThinkSmart on board B, `msm_dpu` for HDMI on board A), not by card number. evdi needs `--force-probe`.
- A tap on an approval card sends the decision with its `request_id` (§7.4). The client never makes a decision on its own. A spoken word only highlights a button (§6.3).
- Package versions are central (`Directory.Packages.props`). Lock files are on. Change an Avalonia version on purpose and test it on a board.
- Tests: xUnit v3 on Microsoft.Testing.Platform (`dotnet test --solution Screens.slnx`).
