# VENTUNO Q inspection cell — architecture overview

*Foundry In Private · v0.7.2 · 2026-11-20*
*Input for specifications and implementation. Facts and decisions only.*

## 1. Purpose and scope

A two-board edge inspection cell built on two Arduino VENTUNO Q boards, governed from Microsoft Foundry in Azure. It serves as hands-on practice for **AI-103** (primary) and, later, as an **AI-500** capstone.

- **Board A**, the perception node: camera, defect detection on the NPU, read-only monitor.
- **Board B**, the reasoning and actuation node: agent, operator console, voice, servo.
- **Azure**, the control plane: models, Content Understanding, Speech, Foundry IQ, AI Search, Blob, tracing and evaluation.
- **MS-02**, outside the cell: plays a contractor's Windows field-service laptop.

Work runs in two tracks:
- **Track 1:** AI-103 cloud-only labs, weeks 1–6.
- **Track 2:** the hardware build, from week 3, a few hours a week.

## 2. Decisions

| ID | Decision |
|---|---|
| D1 | AI-103 first; AI-500 after |
| D2 | Boards split by role: A = perception, B = reasoning + actuation. One NPU workload owner per board |
| D3 | Agent reasoning, tool calling and image captioning run in Azure. The NPU runs detection (board A) and a tool-free local SLM for explanation text (board B) |
| D4 | Python 3.12 on both boards. C# (.NET 10) on the Windows field-service laptop. Operator screens are web pages in Chromium kiosk mode |
| D5 | Board B owns the operator console (screen B). Screen A is a read-only perception monitor |
| D6 | Offline mode is a deterministic state machine on board B. The local SLM only generates explanation text |
| D7 | Findings go to SQLite on board B and are pushed directly to the AI Search index behind Foundry IQ (near real time). Images go to Blob. A local `findings.recent` tool answers questions about the last hours |
| D8 | Board A is LAN-only: it stores images on its NVMe and serves them over the LAN. Only board B calls Azure data services (board A sends telemetry only) |
| D9 | Escalation buttons name the physical outcome: **Reject part** (gate fires) · **Pass part** (no movement) · **Defer** (no movement, part set aside) |
| D10 | Each board has an NVMe SSD for models, Docker data root, caches and datasets. Board B has one powered USB 3.0 hub |
| D11 | UNO Media Carriers are removed by default. Refit on board B only for the 3.5 mm microphone baseline test |
| D12 | The MS-02 is not part of the cell. It appears only as the contractor laptop (H3) and is used once to configure the PanaCast 20 with Jabra Direct |

## 3. Platform facts

### 3.1 VENTUNO Q (ABX00181)

| Item | Fact |
|---|---|
| SoC | Qualcomm Dragonwing IQ8 (QCS8275): 8× Kryo, Adreno 623, Hexagon NPU up to 40 dense TOPS, Spectra 692 ISP |
| Memory / storage | 16 GB LPDDR5, 64 GB eMMC (only variant), M.2 NVMe Gen 4 slot |
| MCU | STM32H5F5 (Cortex-M33, Zephyr), RPC bridge between Linux (Python) and the sketch |
| I/O | 2× USB 3.0 Type-A, 1× USB-C (host/device, DP Alt), HDMI, 2.5 GbE, Wi-Fi 6, BT 5.3, Qwiic, 40-pin HAT header, UNO headers (3.3 V MCU logic), CAN-FD, 3× MIPI CSI |
| Power | USB-C PD 9–20 V, or 7–24 V barrel jack / screw terminal. Arduino 65 W USB-C supply ships with a barrel adapter |
| OS | Ubuntu 24.04 (aarch64), Docker preinstalled |
| Thermal | The NPU is the main heat source under sustained load |

### 3.2 Runtimes on the board

| Runtime | Use in this cell | Notes |
|---|---|---|
| ONNX Runtime + Qualcomm QNN plugin EP (`onnxruntime-qnn`, Python wheel, Linux aarch64) | YoloX detector on board A's NPU | NuGet package is Windows-only; Python is the supported path |
| GenieX (Qualcomm community runtime; OpenAI-compatible server, default `127.0.0.1:18181`) | Local SLM on board B's NPU, text only | Docker needs `--privileged` and a `/usr/lib` mount for NPU access. VLM time to first token is 32–194 s depending on image size, so no local VLM. Serving-stack tool calling is unreliable, so no local tool calling |
| `faster-whisper` (tiny/base, int8, CPU) + Piper | Offline voice on board B | Cloud Azure Speech is the primary voice path |
| Foundry Local (on-device SDK) | Not used on the boards | Linux ARM64 is CPU only; NPU acceleration exists only on Windows. Used on the MS-02 laptop |
| Foundry Local on Azure Local | Not used | Requires Azure Local + AKS Arc; ARM64 support is preview (predictive ONNX on CPU, Jetson Thor GPU). Reference reading only |

### 3.3 Azure services

| Service | Use |
|---|---|
| Foundry project + model router | Agent reasoning, tool calling, multimodal captioning of defect crops |
| Content Understanding | Comparison lane for defect images; extraction of service-manual PDFs (H3) |
| Azure AI Search + Foundry IQ knowledge base | Findings index (push API) and service-manual index; queried over MCP |
| Blob Storage | Inspection images |
| Azure Speech | STT/TTS for the voice operator |
| Content Safety | Prompt Shields and filters on agent input |
| Application Insights + Foundry external agents | OTel traces; scheduled trace evaluation |
| Key Vault | Secrets for board B and the laptop |

Platform constraints:
- **Foundry Tools containers are x64-only**, so no containers run on the boards.
- **Azure Embedded Speech is Limited Access** (Microsoft-managed customers only) and has no pl-PL speech-to-text model, so it is not used.

## 4. Hardware

### 4.1 Inventory

| Item | Role |
|---|---|
| 2× Arduino VENTUNO Q | Board A and board B |
| 2× UNO Media Carrier | Off by default |
| Jabra PanaCast 20 (4K USB, UVC) | Board A camera. Intelligent Zoom off, set once via Jabra Direct |
| Hama 1080p USB camera | Spare / fallback camera |
| Waveshare 1280×800 HDMI touch display | Screen A (read-only, no touch cable) |
| ThinkSmart 1280×800 DisplayLink display | Screen B (video + touch over one USB cable) |
| Jabra Speak2 40 / 55 / 75 (USB) | Board B audio; three units for the mic comparison; one spare for the laptop |
| MS-02 (Windows) | Field-service laptop persona |

### 4.2 To buy

| Item | Board A | Board B |
|---|---|---|
| M.2 2230 NVMe SSD, 512 GB, PCIe Gen 4 | 1 | 1 |
| Powered USB 3.0 hub (own power supply) | – | 1 |
| MG90S metal-gear micro servo | – | 1 |
| 5 V 2 A supply + DC jack screw adapter | – | 1 |
| Mini breadboard + jumper wires | – | 1 |
| USB-C power meter with PD 3.0 passthrough | – | 1 |
| 5 V USB phone charger (spare) | 1 | – |
| Later: Modulino Latch Relay (DC ≤ 30 V / 2 A) | – | 1 |
| Optional: Arducam B0488 IMX577 (4-lane MIPI), Modulino Distance, desk lamp | | |

### 4.3 Board A wiring

| Port / connector | Connected to |
|---|---|
| Barrel jack | 65 W USB-C supply via its barrel adapter |
| M.2 | NVMe SSD |
| USB-A #1 | Jabra PanaCast 20 |
| USB-A #2 | Free (keyboard during bring-up) |
| USB-C | Unused |
| HDMI | Waveshare screen A; the screen is powered from a 5 V USB charger |
| Ethernet | LAN switch |

### 4.4 Board B wiring

| Port / connector | Connected to |
|---|---|
| Barrel jack | 65 W USB-C supply → USB-C power meter → barrel adapter |
| M.2 | NVMe SSD |
| USB-A #1 | ThinkSmart screen B (DisplayLink, video + touch, one cable) |
| USB-A #2 | Powered USB hub → Jabra Speak2 (+ keyboard during bring-up) |
| USB-C | Unused until host mode under barrel power is verified |
| UNO header, PWM-capable pin (confirm in user manual) | MG90S signal |
| UNO header GND | Ground jumper to the 5 V servo supply GND |
| 5 V 2 A supply | MG90S V+ and GND |
| Ethernet | LAN switch |

### 4.5 Network

- Both boards on wired Ethernet to the lab LAN switch; outbound HTTPS to Azure.
- No inbound connections from Azure.
- Board A → Azure: telemetry only (Application Insights).
- Board B → Azure: models, Content Understanding, Speech, Content Safety, AI Search (push and query), Foundry IQ (MCP), Blob, telemetry.
- Board B → board A: MCP (`vision.*`) and HTTP image fetch on the LAN.
- Laptop → board B: read-only diagnostics MCP with a short-lived credential.

## 5. Software architecture

### 5.1 Board A — perception node (Python)

| Component | Responsibility |
|---|---|
| Capture | V4L2 capture from the PanaCast 20; preview stream (MJPEG/WebSocket) for screen A |
| Detector | YoloX on the NPU via ONNX Runtime + QNN plugin EP |
| Image store | Full frames and crops on NVMe, served over LAN HTTP; retained until board B confirms upload |
| MCP server | `vision.inspect`, `vision.capture_still` |
| Monitor UI | Screen A kiosk page: live view, boxes, detector latency |
| Telemetry | OTel → Application Insights |

### 5.2 Board B — reasoning and actuation node (Python)

| Component | Responsibility |
|---|---|
| Cell controller | Connectivity check; switches between the online agent and the offline state machine |
| Online agent (Microsoft Agent Framework) | Foundry model router; tools: `vision.inspect` (board A), `mcu.servo`, `findings.recent`, Foundry IQ (MCP); human approval through `RequestPort` |
| Offline state machine | Fixed flow (§6.2); GenieX for explanation text only |
| Operator console | Screen B kiosk page: Inspect, Reject part / Pass part / Defer, Talk, verdict history, reason, citations, lane, decided-by, model version |
| Voice | Speak2; Azure Speech online; `faster-whisper` int8 + Piper offline |
| Findings store | SQLite; upload queue to Blob (images) and AI Search (JSON); drains when online |
| MCU bridge | `mcu.servo` MCP tool → Arduino RPC bridge → STM32H5 sketch |
| MCU firmware | Servo range and duty limits, e-stop input; enforced regardless of commands |
| Diagnostics MCP | Read-only tools for the contractor laptop |
| Telemetry | OTel → Application Insights; registered as a Foundry external agent |

### 5.3 Field-service laptop (C#, Windows)

- Foundry Local (WinML) for offline reasoning.
- A service-manual knowledge base: PDFs extracted with Content Understanding and indexed in Foundry IQ, plus a small local index for offline use.
- A read-only diagnostics MCP client to board B, using a short-lived credential.
- Proposed fixes appear on screen B as pending actions that need the operator's tap.

## 6. Behaviour

### 6.1 Inspection, online

1. The operator taps **Inspect** on screen B.
2. Board B calls `vision.inspect` on board A. Board A captures, runs YoloX, saves the frame and crops to NVMe, and returns the detections with LAN image URLs.
3. Board B fetches the crops, shows them on screen B, and sends them to a Foundry multimodal model (with Content Understanding as a comparison lane). The agent decides `pass | escalate | reject`.
4. The decision takes effect:
   - `reject`: `mcu.servo`, with no tap needed. Recorded as `decided_by: agent`.
   - `escalate`: an approval card on screen B. **Reject part** fires the gate; **Pass part** means no movement; **Defer** or timeout means no movement and the part is set aside.
   - `pass`: log only.
5. Board B writes the finding to SQLite and queues the images for Blob and the JSON for the search index; the queue drains immediately when online.

### 6.2 Inspection, offline (state machine on board B)

```
idle ──Inspect──▶ inspecting ──vision.inspect──▶ scored
scored ── no detection ≥ threshold ──▶ pass ──▶ log ──▶ idle
scored ── any detection ≥ threshold ──▶ awaiting_operator
            (GenieX writes a one-paragraph explanation; screen B shows crop + text)
awaiting_operator ── Reject part ──▶ actuating ──mcu.servo──▶ log ──▶ idle
awaiting_operator ── Pass part ──▶ log ──▶ idle                 (no movement)
awaiting_operator ── Defer / timeout ──▶ log ──▶ idle           (no movement, set aside)
```

Offline findings carry `lane: offline` and stay in board B's queue until the connection returns. Board A keeps the images until board B has uploaded them.

### 6.3 Human approval rules

- **A tap on screen B is required for:**
  - every escalated part;
  - every reject while offline;
  - any configuration change proposed by the contractor laptop;
  - actions above the high-risk tier in the guardrail lab.
- **Only a screen B tap resolves a card.** It must be bound to the pending request id; a timeout means no movement.
- **Nothing the model reads, hears or says can resolve a card.** A spoken "reject" or "pass" only highlights the matching button.
- **The MCU enforces servo limits and the e-stop** whatever it is asked to do.

## 7. Interfaces and data objects

### 7.1 MCP tools

| Tool | Host | Called by | Purpose |
|---|---|---|---|
| `vision.inspect` | Board A | Board B | Capture + detect; returns detections and LAN image URLs |
| `vision.capture_still` | Board A | Board B | Full-resolution still on demand |
| `mcu.servo` | Board B | Board B agent / state machine | Move the reject gate; returns the MCU acknowledgement |
| `findings.recent` | Board B | Board B agent | Findings from the last N hours (SQLite) |
| Foundry IQ knowledge base | Azure | Board B agent, laptop | Historical findings, inspection rules, service manuals |
| Diagnostics (read-only) | Board B | Laptop | Recent findings, detector confidence histogram, model versions, camera settings, temperatures |

### 7.2 `vision.inspect` result (board A → board B)

```jsonc
{
  "inspection_id": "insp-2026-11-20T21-14-07Z-A-0142",
  "part_id": "1142",
  "captured_at": "2026-11-20T21:14:07.412Z",
  "camera": { "model": "Jabra PanaCast 20", "resolution": [3840, 2160], "intelligent_zoom": false },
  "detector": { "model": "yolox-s-defects", "version": "1.2.0", "runtime": "ort-qnn-htp", "latency_ms": 38 },
  "detections": [
    { "label": "scratch", "confidence": 0.87, "bbox_xyxy": [1412, 820, 1690, 905] },
    { "label": "missing_stud", "confidence": 0.41, "bbox_xyxy": [2010, 640, 2140, 760] }
  ],
  "images": {
    "full": "http://board-a.lan:8080/images/insp-…-0142/full.jpg",
    "crops": ["http://board-a.lan:8080/images/insp-…-0142/crop-0.jpg",
              "http://board-a.lan:8080/images/insp-…-0142/crop-1.jpg"]
  }
}
```

### 7.3 Finding (board B → SQLite, AI Search, Foundry IQ)

```jsonc
{
  "finding_id": "fnd-0142",
  "inspection_id": "insp-2026-11-20T21-14-07Z-A-0142",
  "part_id": "1142",
  "verdict": "reject",            // pass | reject | escalated_reject | escalated_pass | deferred
  "decided_by": "agent",          // agent | operator | state_machine
  "lane": "cloud",                // cloud | offline
  "defects": [
    { "type": "scratch", "severity": "major", "confidence": 0.87,
      "caption": "Diagonal scratch across the top face, about 4 mm long" }
  ],
  "summary": "Part 1142 rejected: a major scratch on the top face (detector 0.87, confirmed by the vision model).",
  "reasoning": {
    "model": "<deployment name>",
    "caption_source": "foundry-multimodal",   // foundry-multimodal | content-understanding | none
    "citations": ["kb://inspection-rules#scratch-major"]
  },
  "images": { "full": "blob://…/full.jpg", "crops": ["blob://…/crop-0.jpg"] },  // LAN URLs until uploaded
  "approval": null,               // see 7.4 when escalated or offline
  "actuation": { "command_id": "act-0142", "result": "done" },
  "trace_id": "4bf92f3577b34da6a3ce929d0e0e4736",
  "agent": { "otel_agent_id": "fip-vqb-inspector", "version": "0.3.0" },
  "upload_state": "uploaded",     // queued | uploaded | failed
  "created_at": "2026-11-20T21:14:09.880Z"
}
```

`summary` is the retrievable text in the search index; the other fields are filterable metadata.

### 7.4 Approval card and decision (board B ⇄ screen B)

```jsonc
// board B → screen B
{ "request_id": "apr-0142", "inspection_id": "insp-…-0142",
  "question": "Possible missing stud (0.41). What should happen to part 1142?",
  "options": ["reject_part", "pass_part", "defer"],
  "evidence": { "crop": "http://board-a.lan:8080/images/insp-…-0142/crop-1.jpg", "explanation": "…" },
  "expires_at": "2026-11-20T21:15:09Z",
  "on_timeout": "no_movement" }

// screen B → board B
{ "request_id": "apr-0142", "decision": "reject_part",   // reject_part | pass_part | defer | timeout
  "operator": "station-1", "decided_at": "2026-11-20T21:14:31Z" }
```

### 7.5 Actuation command and acknowledgement (board B ⇄ STM32)

```jsonc
// board B → MCU
{ "command_id": "act-0142", "action": "servo.move", "position": "reject",
  "authorized_by": "agent",        // agent | apr-<id>
  "deadline_ms": 500 }

// MCU → board B
{ "command_id": "act-0142", "result": "done",   // done | refused | timeout
  "applied_angle_deg": 75, "limit_clamped": false, "estop": false }
```

Correlation keys: `inspection_id`, `trace_id` and `otel_agent_id` appear across all objects.

## 8. Track 1 — AI-103 cloud labs (weeks 1–6)

| # | Lab | Domain |
|---|---|---|
| C0 | One Azure region that offers the chat/multimodal models, image (and video) generation, Content Understanding audio/video analyzers, Speech and AI Search semantic ranking | D1 |
| C1 | Foundry project with Bicep/azd: managed identity, RBAC, private endpoint + private DNS zone, Key Vault | D1 |
| C2 | Deployment types and quotas (TPM/RPM), rate limits, model router modes | D1 |
| C3 | GitHub Actions running evaluations as a PR gate | D1 |
| C4 | Content Safety: filters, Prompt Shields, groundedness | D1 |
| C5 | Foundry Agent Service prompt agent: tools, MCP tool, file search, conversation state | D2 |
| C6 | Image generation and editing (masks/inpainting); video generation where available | D3 |
| C7 | Content Understanding analyzers: document, image, audio, video | D3 / D5 |
| C8 | Azure AI Search: skillsets, integrated vectorization, keyword/vector/hybrid/semantic ranking, push API; Foundry IQ on top | D5 |
| C9 | Language and Translator (PII, entities, summarization, document translation); Speech STT/TTS/translation, custom speech | D4 |

Verify the objective wording against the current AI-103 study guide.

## 9. Track 2 — hardware work items

| ID | Work item | AI-103 coverage |
|---|---|---|
| H1 | Two-node inspection cell (§5, §6). Test parts: LEGO bricks, screws, printed cards. Image-folder replay as the regression harness | D1, D2, D3, D5 |
| H2 | Voice operator on board B: Azure Speech online, `faster-whisper` + Piper offline; WER (PL/EN script), voice-to-voice latency and VAD false triggers per Speak2 model and per lane | D1, D4 |
| H3 | Field-service laptop (C#): Foundry Local, service-manual KB, read-only diagnostics MCP, fixes approved on screen B | D1, D2, D5 |
| H0 | Placement table on board B: GenieX NPU vs CPU llama.cpp vs cloud router, tool-free prompts, watts from the inline meter | D1 |

### 9.1 AI-500 capstone (after AI-103)

| ID | Work item |
|---|---|
| P2 | Guardrails: four middleware checkpoints (input, tool call, tool response, action); ablation; SDK red-team scan against board B's agent endpoint and a cloud twin; ASR per layer. Attack surfaces: printed text in front of the camera, spoken injection, model output imitating a control |
| P4 | A2A between the two nodes, durable workflow, chaos drills (power loss on board A mid-inspection) |
| R1 | Canary of agent versions on board B, eval-gated promote/rollback |
| X1 | LangGraph port; Transformers fine-tune of a guard classifier; Key Vault rotation; Foundry hosted-agent twin; Entra Agent ID; agent memory design and evaluation; Microsoft Purview classification |

## 10. Spikes

| ID | Spike | Pass | Fallback |
|---|---|---|---|
| S9 | GenieX conformance (F1 T1–T5) at 500 / 1,000 / 2,000-token prompts | Behaviour documented | – (no tools used) |
| S11 | Speak2 call button as HID push-to-talk | Press → recording within 50 ms | Talk button on screen B |
| S12 | PanaCast 20 settings survive a power cycle; V4L2 capture ≥ 15 fps at 1280×720 | Stable framing; frames reach the detector | Hama camera |
| S13 | Kiosk on both screens; ThinkSmart video + touch over one cable, stable on bus power under load | Both screens usable; touch correct on B; no flicker | Powered USB Y-cable for the ThinkSmart |
| S15 | NPU sharing (two ORT-QNN sessions; ORT-QNN + GenieX) | Documented with latency | One NPU owner per board |
| S16 | NVMe mounted; Docker data root, model caches, board A image store moved | Nothing large on eMMC | – |
| S17 | USB-C host mode under barrel power | USB device enumerates | Leave USB-C unused |
| S18 | Power meter inline with PD passthrough | Board negotiates 20 V and boots | Wall-plug meter |
| S8 | Optional: QNN plugin from C# on linux-arm64 | HTP device listed | Python |

## 11. Timeline

| Weeks | Track 1 | Track 2 |
|---|---|---|
| 1–2 | C0–C4 | Order parts |
| 3–4 | C5–C7 | Bring-up: NVMe, power, hub, S12, S13, S16, S17, S18 |
| 5–6 | C8–C9, practice assessment, sit AI-103 | H1 on image replay |
| 7–10 | AI-500 study | H1 live camera + servo; H2; S9, S11 |
| 11–14 | AI-500 study | H3, H0 |
| 15+ | AI-500 study | P2, P4, R1, X1 |

## 12. Risks

| Risk | Mitigation |
|---|---|
| Local tool calling unreliable | Offline path is a state machine; no local tools |
| Uploads fail while offline | Board B queue; images kept on board A until uploaded; `findings.recent` |
| Wrong escalation button | Buttons name the physical outcome; the gate moves only on **Reject part** |
| PanaCast settings lost after firmware updates | Re-apply via Jabra Direct on the laptop; Hama fallback |
| USB / Wi-Fi flakiness after boot | Barrel power, powered hub on B, wired Ethernet |
| USB audio dropouts next to DisplayLink | DisplayLink and Speak2 on separate USB-A ports |
| NPU thermals | Temperature telemetry; pause NPU work when hot |
| CPU contention on board B offline | int8 tiny/base Whisper; static screen during capture |
| Regional gaps in Azure services | Region check C0 before infrastructure code |
| Exam objectives change | Re-read study guides before C0 and before booking |
