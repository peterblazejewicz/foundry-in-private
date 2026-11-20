---
name: experiment-log
description: Write a step-by-step experiment or spike log to docs/experiments/ in the form kb-promote reads, then hand measured numbers to kb-promote. Spike mode runs an S-item (S12 camera, S13 kiosk, S15 NPU sharing, S16 NVMe, S17 USB-C host, S18 power meter, ...) against its pass criterion and fallback from the architecture doc.
---

# experiment-log

## File

`docs/experiments/YYYY-MM-DD-<topic>.md`:

```markdown
# <topic>

- date: YYYY-MM-DD
- board: <board A | board B (ventuno)> / OS, kernel, firmware
- item: <S-id or H-id with label>
- commit: <sha>

## Question
## Setup
## Steps          (exact commands, in order)
## Results        (table: metric | value | unit | notes)
## Conclusion     (one or two sentences)
## Decision       (spike mode only: PASS or FALLBACK-TAKEN, and what changes)
## KB             (what was promoted, or "nothing new")
```

## Normal mode

1. Write the file while you work, not after.
2. Put raw numbers in Results. Name the conditions for each number.
3. If a result is reusable (a device fact, a measured number), run `kb-promote`. Link the KB finding in the KB section.

## Spike mode

1. Read the spike row in the architecture doc §10: pass criterion and fallback.
2. Run the test. Measure the pass criterion directly.
3. Decision: PASS, or FALLBACK-TAKEN with the fallback named.
4. If the decision changes a D-row or a fact in the doc, run `arch-decision`.
