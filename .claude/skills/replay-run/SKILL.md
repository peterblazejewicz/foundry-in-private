---
name: replay-run
description: Run the H1 image-replay regression harness - feed a pinned image set (real captures and C6 synthetic defects) through the inspection path with a fake camera, compare verdicts and detector scores with the last accepted run. Use before a detector, threshold, prompt or agent change is merged.
---

# replay-run

## Inputs

- A dataset manifest entry in `tools/replay/datasets.json`: name, Blob path, file list with sha256, labels (expected verdict per image).
- The code under test (current branch).

## Steps

1. Fetch the dataset to the local cache and check every sha256. Stop on a mismatch.
2. Run the harness with the fake camera pointed at the dataset. Run the real detector on a board, or the fake detector with recorded scores on the dev machine. Say which.
3. Write the results: per image `inspection_id`, verdict, `decided_by`, lane, detector scores, latency.
4. Compare with the last accepted run of the same dataset:
   - verdict changes (count and list);
   - false pass and false reject counts against the labels;
   - p50/p95 latency.
5. Write the run to `docs/experiments/` (`experiment-log`). Name the dataset, its manifest sha and the commit.
6. A new false pass on a labelled defect blocks the change. Say so in the first line.

## Synthetic defects (C6, image generation and inpainting)

- Each synthetic image records its source image, the mask, the prompt and the model deployment in the dataset manifest.
- Keep real and synthetic results apart in the report.
