---
name: ai103-objective-map
description: Keep a dated snapshot of the AI-103 study-guide objectives in docs/exam/ai103-objectives.md (from Microsoft Learn via the microsoft-docs tools), tag labs (C0-C9), work items (H1-H3) and PRs with the exam domains they cover, and list objectives with no lab yet. Refresh when asked or when the snapshot is older than 30 days. AI-103 only.
---

# ai103-objective-map

## Snapshot

File: `docs/exam/ai103-objectives.md`.

```markdown
# AI-103 objectives — snapshot

- source: <Microsoft Learn study guide URL>
- fetched: YYYY-MM-DD
- exam version / last updated on the page: <as shown>

| Domain | Weight | Objective | Covered by |
|---|---|---|---|
```

1. Find the study guide with `microsoft_docs_search` ("AI-103 study guide"). Fetch it with `microsoft_docs_fetch`.
2. Copy the domains, weights and objectives as the page gives them. Do not paraphrase.
3. Fill "Covered by" with lab and work-item IDs (C0–C9, H1–H3), or `—`.

## Refresh

- When the owner asks, or when `fetched` is older than 30 days: fetch again, show the diff (added, removed, changed objectives), update the file.
- Before C0 (region check) and before the exam is booked: refresh (architecture §12 risks).

## Tagging

- A PR description gets one line: `AI-103: <domain> — <objective>` per objective it covers.
- At the end of each lab, list the objectives it covered and any it did not reach.
- Report gaps: objectives with no lab, sorted by domain weight.
