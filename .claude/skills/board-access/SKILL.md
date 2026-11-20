---
name: board-access
description: Use before any SSH command, file copy or system change on a VENTUNO Q board (board B = `ventuno`, 10.0.0.9; board A = `board-a` when set up). Enforces the ~/README.txt protocol - read first, log each change with a revert line, ask before reboot, install or stopping another appliance.
---

# board-access

## Hosts

| Role | SSH alias | Notes |
|---|---|---|
| Board B (reasoning, actuation) | `ventuno` | 10.0.0.9. Shared with other appliances until they are stopped |
| Board A (perception) | `board-a` | Not set up yet. Add the alias when the board is on the LAN |

## Procedure

1. Read the log: `ssh <host> cat ~/README.txt`. Find which appliance owns the NPU, the Speak2 and each screen now.
2. Read the KB note for the board (`kb-lookup`: `arduino-ventuno-q`) when the change touches the NPU, DRM cards, audio or USB.
3. Say the change to the owner in one line. Ask first for:
   - a reboot or a power cycle;
   - `apt`, `pip`, `uv tool` or any other install;
   - a stop, disable or uninstall of another appliance (gemma-teacher, AskBox, storybook);
   - a change in `/etc`, systemd units, udev rules, kernel modules or groups.
4. Make the change.
5. Append to `~/README.txt` on the board, in this form:

   ```
   ## YYYY-MM-DD foundry-in-private: <what changed>
   - change: <exact command or file>
   - revert: <exact command that undoes it>
   ```

6. Check the result with a command, not by assumption. Put measured numbers in `docs/experiments/` (`experiment-log`).

## Never

- Never edit or delete earlier lines of `~/README.txt`.
- Never store a secret on the board outside the agreed path (Key Vault reference or a root-only env file named in the README).
