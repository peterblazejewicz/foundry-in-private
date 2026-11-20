---
name: azd-cycle
description: Bring the Azure control plane up or down with azd - region and quota check, cost note, `azd up` / `azd provision` / `azd down --purge`, each only after the owner's ACK. Use for any provision, deploy, teardown or "is it still billing?" task.
---

# azd-cycle

Every step that creates, changes or deletes a resource needs the owner's ACK (hard rule 3).

## Up

1. **Region (C0, region check).** Check that the region has the models, Content Understanding, Speech and AI Search semantic ranking the cell needs. Use the quota skill (`azure-quotas`) or `az` read-only commands.
2. **Quota (C2, deployment types and quotas).** Check TPM/RPM for the model deployments.
3. **Cost note.** Estimate the cost per day of the resources in the plan (`cost-estimation` or the Azure pricing pages). Write it in one line: "about N EUR/day while up".
4. **Preflight.** `azd provision --preview`. Run the `infra-reviewer` subagent on the Bicep diff.
5. **ACK.** Show the owner: environment, region, the resource list, the cost note. Wait for "yes".
6. `azd up` (or `azd provision`). Record the environment name and the time.

## Down

1. List what is billing: `az resource list -g <rg> -o table`.
2. ACK from the owner.
3. `azd down --purge --force`. `--purge` removes soft-deleted Key Vault and Cognitive Services accounts.
4. Check: `az group exists -n <rg>` returns `false`.

## Rules

- One resource group per azd environment. Nothing outside it.
- No keys in `.azure/` outputs that go into git (`.azure/` is ignored).
- Say when an environment has been up for more than one day.
