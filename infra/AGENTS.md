# infra/ — Azure control plane (Bicep, azd)

- Created in C1 (Foundry project with Bicep/azd). `azure.yaml` goes at the repo root.
- Use the `azd-cycle` skill for `azd up` and `azd down`. Ask the owner first, every time.
- Run the `infra-reviewer` subagent on each Bicep change before the PR.
- Use managed identity and RBAC. No keys in outputs, parameters or app settings. Secrets go in Key Vault.
- One resource group per environment. `azd down --purge` must remove everything. Nothing billable outside the group.
- Tag every resource: `azd-env-name`, `project=foundry-in-private`, `lab=<C-id>`.
- Check the region before any code (C0, region check): models, Content Understanding, Speech, AI Search semantic ranking.
