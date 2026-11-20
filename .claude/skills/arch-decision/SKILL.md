---
name: arch-decision
description: Change the architecture document (docs/architecture/VQ-ventuno-q-architecture-v<semver>.md) - add or reverse a D-row, fix a fact, update affected sections, bump the version (patch for a fact, minor for a decision), rename the file with git mv, add a changelog line. Use when a spike, a spec or the owner changes a decision.
---

# arch-decision

The doc holds facts and decisions only. There is no ADR folder.

## Steps

1. Find the current file: `docs/architecture/VQ-ventuno-q-architecture-v*.md` (one file only).
2. Make the change:
   - a decision: add or change the D-row; change every section that the decision touches (§3 platform, §5 software, §6 behaviour, §7 interfaces, §9–§12);
   - a fact: change it where it is stated; name its source (KB note, finding, vendor doc).
3. Bump the version: **patch** for a fact fix, **minor** for a new or reversed decision, **major** for a change of scope.
4. Update the header line (`v<new> · YYYY-MM-DD`).
5. Rename with `git mv` so history follows: `git mv ...-v0.7.2.md ...-v0.8.0.md`.
6. Add a changelog line at the end of the doc: `- YYYY-MM-DD v<new>: <what changed> (<reason or source>)`.
7. Fix links to the old file name (`AGENTS.md`, `docs/intent/`, specs).
8. Tag the PR with `ai103-objective-map`.

## Never

- Never keep two versions of the file in the tree.
- Never put measured numbers in the doc without a source (finding or experiment log).
