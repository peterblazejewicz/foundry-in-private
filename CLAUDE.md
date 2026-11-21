@AGENTS.md

## Orchestration

- Start a task with `/lights-on:factory <goal>` (plugin `lights-on`). Catch up with `/lights-on:status`.
- The session does the task work. A fresh reviewer agent reviews the branch at the end of a `feature` run. The owner reviews the branch and pushes.

## Claude Code only

- Output style: `terse` (`.claude/output-styles/terse.md`), selected in `.claude/settings.json`.
- Skills are in `.claude/skills/`. Subagents are in `.claude/agents/`.
- Permissions in `.claude/settings.json` enforce hard rules 3 and 7: `git push`, `azd`, `ssh` and `rsync` ask; a push to `main` and a force-push are denied.
- Azure skills come from the project marketplace `.claude-plugin/marketplace.json`: plugin `cell-azure` = 5 skills of `microsoft/azure-skills` at a pinned commit, and the Azure MCP server pinned to `@azure/mcp@4.0.0-beta.7`. No hooks. Telemetry off (`AZURE_MCP_COLLECT_TELEMETRY=false` in `env` and on the server). Its MCP tools ask first.
- The Cost Management MCP (`azure-cost`) is a local-scope server per user. Its client ID and tenant ID are not in git.
  - Setup: create an Entra app (single tenant, public client, redirect `http://localhost:8765/callback`, delegated `MCP.Access` on `22bfbae3-f4e7-485f-be43-8cee15065084` (ARM MCP Server app), admin consent). Then run this command from the repo root in Git Bash: `claude mcp add-json azure-cost '{"type":"http","url":"https://mcp.management.azure.com","headers":{"x-mcp-toolset":"CostManagement"},"oauth":{"clientId":"<CLIENT_ID>","callbackPort":8765,"authServerMetadataUrl":"https://login.microsoftonline.com/<TENANT_ID>/v2.0/.well-known/openid-configuration","scopes":"api://22bfbae3-f4e7-485f-be43-8cee15065084/MCP.Access"}}' --scope local`
    - Local scope binds to the project path. PowerShell can strip the inner JSON quotes.
    - Keep the server name `azure-cost`: the permission rules match that name.
    - The user needs the Cost Management Reader role (or higher) on the subscription.
    - Then sign in with `/mcp` → `azure-cost` → Authenticate.
  - Why the metadata URL: the server advertises the `/organizations` authority. It refuses personal accounts and signs a guest into the home tenant, not the resource tenant.
  - If sign-in reports "does not support dynamic client registration" after a config change: remove the stale `azure-cost` entries under `mcpOAuth` in `~/.claude/.credentials.json` (macOS: Keychain). Then restart Claude Code.
  - `settings.json` allows the 20 listed read tools of `azure-cost`. All other `azure-cost` tools ask first. After a server update, check its tool list against these rules.
- To move the pin: change `sha` (and the MCP version) in `marketplace.json` in one PR, then `claude plugin update cell-azure@foundry-in-private`.
- Nested `AGENTS.md` files in `edge/`, `screens/` and `infra/` load when you read a file there.
