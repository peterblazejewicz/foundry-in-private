@AGENTS.md

## Orchestration

- Start a task with `/lights-on:factory <goal>` (plugin `lights-on`). Catch up with `/lights-on:status`.
- The session does the task work. A fresh reviewer agent reviews the branch at the end of a `feature` run. The owner reviews the branch and pushes.

## Claude Code only

- Output style: `terse` (`.claude/output-styles/terse.md`), selected in `.claude/settings.json`.
- Skills are in `.claude/skills/`. Subagents are in `.claude/agents/`.
- Permissions in `.claude/settings.json` enforce hard rules 3 and 7: `git push`, `azd`, `ssh` and `rsync` ask; a push to `main` and a force-push are denied.
- Azure skills come from the project marketplace `.claude-plugin/marketplace.json`: plugin `cell-azure` = 5 skills of `microsoft/azure-skills` at a pinned commit, the Azure MCP server pinned to `@azure/mcp@4.0.0-beta.7`, and the Cost Management MCP. No hooks. Telemetry off (`AZURE_MCP_COLLECT_TELEMETRY=false` in `env` and on the server). Their MCP tools ask first.
- To move the pin: change `sha` (and the MCP version) in `marketplace.json` in one PR, then `claude plugin update cell-azure@foundry-in-private`.
- Nested `AGENTS.md` files in `edge/`, `screens/` and `infra/` load when you read a file there.
