# Not On My Watch — VR First Aid Training Simulation

A VR simulation game built in Unity where players practice real-world first aid procedures on patients in distress. The game tests response time, procedural accuracy, and prioritization across multiple simultaneous emergencies.

---

## Presentation

[View Slides](https://canva.link/mpnmt7v1juj64x9)

---

## Demo Videos

| Video | Link |
|---|---|
| CPR Practice Mode | [Watch](https://drive.google.com/file/d/1tQrhZFDDjYe5gc2ctWC0WrhVPrO7Ebf4/view?usp=sharing) |
| Wound Care Practice Mode | [Watch](https://drive.google.com/file/d/1j2KovTA70kEP6xeyuNMQ_4lq4UGKBTTA/view?usp=sharing) |
| Start Screen & Game Scene Walkthrough | [Watch](https://drive.google.com/file/d/1UGUfjW9Mq4gBZ-2EdYEO-jRuId7OowIc/view?usp=sharing) |

---

## Overview

The player is placed in a VR environment and must respond to up to 4 patients that go into distress sequentially over a 20-minute session. Patients require either CPR or wound care, and must be treated in the correct procedural order. Points are deducted for late responses, wrong step order, and incomplete treatment.

**Product name:** not-on-my-watch-vc-v11  
**Engine:** Unity (XR Interaction Toolkit)  
**Target platform:** Meta Quest (VR)

---

## Scenes

| Scene | Purpose |
|---|---|
| `StartScene` | Main menu and entry point |
| `Room1` | Tutorial — guided CPR training with step-by-step instruction cards |
| `Room2` | Tutorial — guided wound care training with step-by-step instruction cards |
| `GameScene` | Full gameplay — all 4 patients, 20-minute session |

---

## Tutorial Rooms

Before entering the main game, players go through two tutorial scenes that teach the exact procedures they will need in `GameScene`. No score is tracked and there is no time pressure — the goal is to build muscle memory for each treatment.

### Room1 — CPR Tutorial

Room1 walks the player through the CPR procedure one step at a time using instruction cards placed in the environment:

1. **Call 911** — Pick up the in-game phone and dial `9-1-1` on the physical keypad.
2. **Check breathing** — Lean your head close to the patient's face and hold the position for ~8 seconds. A circular progress indicator confirms the check is complete.
3. **Chest compressions** — Place both hands on the patient's chest and compress 30 times at the correct depth (6 cm) and rate (100–120 BPM). On-screen sliders give real-time feedback on depth and rhythm.

Completing Room1 familiarises players with the phone interaction, the breathing-check zone, and the compression tracker before those mechanics are tested under time pressure in `GameScene`.

### Room2 — Wound Care Tutorial

Room2 walks the player through the wound care procedure one step at a time:

1. **Put on gloves** — Walk to the first aid station and pick up the gloves.
2. **Clean the wound** — Grab the antiseptic spray, approach the patient, and spray the wound zone 3 times.
3. **Apply bandage** — Grab the bandage roll and wrap it around the patient's arm in a circular motion 4 times. A progress bar tracks how many wraps have been completed.

Completing Room2 familiarises players with the first aid station layout, the spray grab-and-aim interaction, and the bandage wrapping mechanic before those steps must be performed quickly in `GameScene`.

---

## Patients

There are 4 patients that appear in order during a `GameScene` session:

1. **CPR Adult** — requires CPR
2. **Wound Care Patient 1** — requires wound care
3. **CPR Kid** — requires CPR
4. **Wound Care Patient 2** — requires wound care

Each patient is scheduled at a randomised time window in sequential order. Patients go through these states:

`Waiting → Distressed → BeingHelped → Healed / PassedOut`

When a patient goes into distress, a pulsing red light and audio cue activate. A timer starts on the HUD showing how long until the patient passes out (default: 4 minutes).

---

## Treatment Procedures

### CPR (Adult & Child)

Must be completed in this exact order:

| Step | Action | How |
|---|---|---|
| 1 | Call 911 | Pick up the in-game phone, dial `9-1-1` on the keypad |
| 2 | Check breathing | Lean your head over the patient's face and hold for ~8 seconds |
| 3 | Perform chest compressions | Place both hands on the patient's chest and compress 30 times at the correct depth (6 cm) and rate (100–120 BPM) |

After a valid 911 call, an ambulance is dispatched and arrives in 90–180 seconds. When the ambulance arrives, if all 3 steps are complete the patient is healed; otherwise **-25 points** are deducted.

### Wound Care

Must be completed in this exact order:

| Step | Action | How |
|---|---|---|
| 1 | Call 911 | Pick up the in-game phone, dial `9-1-1` on the keypad |
| 2 | Put on gloves | Go to the first aid station, pick up the gloves |
| 3 | Clean the wound | Grab the antiseptic spray, bring it to the patient, spray the wound zone 3 times |
| 4 | Apply bandage | Grab the bandage roll and wrap it around the patient's arm in a circular motion 4 times |

Wound care patients walk around the environment before going into distress. When distressed they fall and stop moving. When healed, they start moving again.

---

## Scoring

Starting score: **100 points**

| Condition | Penalty |
|---|---|
| Not reaching a patient within 60 seconds | −1 pt every 10 seconds |
| Performing a step out of order | −1 pt every 5 seconds until correct step is done |
| Ambulance arrives / timer expires with incomplete steps | −25 pts |
| Score reaches 0 | Game ends immediately |

A **hint panel** appears on the HUD whenever a step is done out of order, telling the player what they should do first.

---

## HUD

The HUD follows the player's camera in world space. It displays:

- **Score panel** — current score (updates every frame)
- **Patient timer** — shows "Reach them!" with a 60-second response countdown, then switches to "Treat them!" with the treatment countdown once the player arrives
- **Ambulance ETA** — appears after a valid 911 call is made
- **Hint panel** — appears when a step is done out of order or the player is too slow to respond; auto-dismisses for timed hints

---

## Key Scripts

| Script | Purpose |
|---|---|
| `GameFlowManager` | Game timer, patient scheduling, score, end game |
| `GamePatient` | Per-patient state machine, step tracking, penalty timers |
| `GameHUD` | World-space HUD — score, timers, hint panel |
| `AmbulanceSystem` | Tracks 911 calls, runs ambulance countdown, resolves CPR patients |
| `PerformanceSummaryManager` | End-of-session results panel (outcomes + response times) |
| `PatientStateController` | Patient animations (fall, blood, heal) |
| `WoundCareWalker` | NPC waypoint movement for wound care patients |
| `CPRCompressionTracker` | Measures hand Y-position to track compression depth and rate |
| `CPRCompressionUI` / `CPRFullUI` | Visual feedback sliders for compression depth and BPM |
| `BreathingCheckZone` | Detects player head proximity for the breathing check step |
| `CPRZone` | Detects both hands positioned correctly on the patient's chest |
| `SprayInteraction` | Handles antiseptic spray activation and wound cleaning |
| `SprayZone` | Trigger zone on each patient that registers spray hits |
| `BandageCircularWrapper` | Tracks circular bandage wrapping motion around a limb |
| `BandageRoll` | Manages which wrapper the held bandage roll is targeting |
| `BandageUI` | Progress feedback during bandage wrapping |
| `WearableItem` | Glove pickup — fires `onWorn` event when worn |
| `FirstAidStation` | Spawns and manages first aid tools; optional respawn timer |
| `PhoneCallManager` | Handles 911 keypad input, triggers ambulance via `onValidCallMade` event |
| `PhoneKeypad` | Physical keypad input in VR |
| `WorldSpaceUIFollow` | Keeps UI panels facing the player camera |
| `CountdownCircleUI` | Circular progress indicator (used for breathing check) |
| `RandomIdleNPC` | Ambient NPC wandering behaviour |
| `ConversationController` / `ChatManager` | AI dialogue integration (DialogueEditor) |

---

## Equipment & Interactions

| Item | Interaction |
|---|---|
| Phone | Pick up and use keypad to dial 911 |
| Gloves | Walk into the glove trigger zone at the first aid station |
| Antiseptic spray | Grab (XR grab), aim at patient wound zone, squeeze trigger 3 times |
| Bandage roll | Grab and physically rotate around the patient's arm 4 times |

Equipment is managed by `FirstAidStation`, which can optionally respawn items after a configurable delay.

---

## Step Order Enforcement

Each patient tracks which step is expected next (`nextExpectedStep`, starting at 1). When a step interaction fires:

- **Correct step** — advances the step counter, clears any active hint
- **Wrong step (skipped ahead)** — shows a hint on the HUD, starts a −1 pt/5 sec penalty until the correct step is completed
- **CPR step 1 (call 911)** — requires player proximity to the patient before registering

Completing all 3 steps sets `AllStepsComplete = true`, which determines the outcome when the ambulance arrives or the help timer expires.

---

## End of Session

When all patients are resolved (healed or passed out) or the 20-minute game timer ends, the `PerformanceSummaryManager` shows a summary panel with:

- Final score
- Each patient's outcome (Helped / Passed out) and response time
- One-line feedback on overall performance

---

## Installation

### Requirements

- [Unity Hub](https://unity.com/download)
- Unity 6 (or the version listed in `ProjectSettings/ProjectVersion.txt`)
- Git

### Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/lhchi04/not-on-my-watch.git
   ```

2. Open **Unity Hub**, click **Add** (or **Add project from disk**), and select the cloned folder.

3. Unity Hub will detect the correct editor version. Install it if prompted, then open the project.

4. Once the project loads, open the `StartScene` scene from the Project window to start from the main menu, or open any individual scene directly.

---

## Playing on Laptop (No Headset)

The project includes the **XR Device Simulator**, which lets you play in the editor without a VR headset.

To enable it in a scene:

1. In the **Hierarchy**, find the `XR Device Simulator` GameObject (or `XR Origin` → `XR Device Simulator`).
2. **Check the checkbox** next to it to activate it.
3. Press **Play**.

Do this for each scene you want to test: `StartScene`, `Room1`, `Room2`, and `GameScene`.

**Default simulator controls (keyboard + mouse):**

| Action | Control |
|---|---|
| Move | `W A S D` |
| Look | Hold `Right Mouse Button` + move mouse |
| Left hand | `T` (toggle) |
| Right hand | `Y` (toggle) |
| Grip / grab | `G` |
| Trigger | `Mouse Button 1` |
