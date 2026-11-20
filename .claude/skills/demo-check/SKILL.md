---
name: demo-check
description: Verify the success demo against the live cell for one inspection_id - SQLite finding on board B, AI Search document, Blob images, Application Insights trace across both boards and Foundry, agent answer citing the finding; offline run checks lane, decided_by and queue drain. Use after a live demo run or a deploy that touches the inspection path.
---

# demo-check

Read-only. It changes nothing in the cell or in Azure.

## Input

An `inspection_id` (or "the last one"). Add `--offline` for a run made with Ethernet disconnected.

## Checks

| # | Check | Where |
|---|---|---|
| 1 | The finding exists; `verdict`, `decided_by`, `lane`, `trace_id` are set | SQLite on board B (`board-access`, read-only query) |
| 2 | `upload_state = uploaded` | SQLite |
| 3 | The finding document is in the findings index, same `finding_id` | AI Search (query by `inspection_id`) |
| 4 | Full frame and crops are in Blob; the finding's image URLs are `blob://` | Blob |
| 5 | One trace with spans from board A, board B and the Foundry agent | Application Insights (KQL on `trace_id`) |
| 6 | The agent answers "what did we reject in the last hour?" and cites the finding | Board B agent, or the Foundry project |
| 7 | Reject: the actuation ack is `done`, `estop=false` | SQLite / finding `actuation` |

Offline run, in addition:

| # | Check |
|---|---|
| 8 | `lane = offline`, `decided_by = operator`, an `approval` object with the `request_id` |
| 9 | After reconnect: the queue drained, checks 2–5 pass |
| 10 | Board A deleted the images only after board B confirmed the upload |

## Output

One table: check, PASS or FAIL, evidence (query result, one line). The first line says DEMO PASS or DEMO FAIL with the failed check numbers.
