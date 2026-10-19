# CS464 Assignment 02

**Name:** Mustafa Fawwaz | **Roll Number:** BSSE23036

This repository contains the implementation and parameter tuning for Assignment 02. The project focuses on creating a polished, physics-based 3D character controller and camera system that delivers tight, responsive platforming across five greybox level blockouts.

---

## System Architecture & Implementation

The movement and camera mechanics are split cleanly between Unity's execution loops to ensure frame-rate independence and robust physics behavior:

- **Input & Jump Queuing (`Update`):** Reads user WASD input, projects movement directions relative to the camera view, and registers jump requests via input buffering.
- **Physics Execution (`FixedUpdate`):** Applies horizontal movement forces to the `Rigidbody`, calculates kinematics for jumping, and handles ground checks using a dedicated layer mask.
- **Camera Tracking (`LateUpdate`):** Updates camera orbit position after all player movement has resolved, utilizing frame-rate independent exponential smoothing.

---

## Tuning Table

Below are the exact parameters configured in `PlayerController.cs` and `CameraFollow.cs` to achieve a snappy, responsive feel across all five levels:

| Parameter | Selected Value | Design Justification |
| :--- | :--- | :--- |
| **Movement Speed (`speed`)** | `5.0` | Provides swift, controllable traversal across level blockouts without causing precision overshooting. |
| **Jump Height (`jumpHeight`)** | `1.5m` | Scaled to clear single-block obstacles reliably while keeping jump arcs predictable. |
| **Coyote Time (`coyoteTime`)** | `0.1s` | Allows a $100\text{ms}$ grace period after running off edges, preventing unintended falls without feeling unfair. |
| **Jump Buffer (`jumpBuffer`)** | `0.175s` | Registers jump inputs made $175\text{ms}$ prior to landing, eliminating missed inputs during rapid platforming. |
| **Fall Multiplier (`fallMultiplier`)** | `2.2` | Increases downward gravity during descent to avoid "floaty" physics and give landing a heavy, responsive feel. |
| **Camera Sharpness (`sharpness`)** | `5.0` | Uses exponential smoothing ($1 - e^{-k \cdot dt}$) to create smooth follow dynamics without camera lag. |
| **Mouse Sensitivity** | `3.0` | Provides fluid, controllable 360-degree orbit rotation locked to the screen view. |

---

## 🎯 Level Traversal Overview

All five level blockouts utilize the unified `Player` and `Main Camera` prefabs with consistent physics settings:

| Level | Focus & Challenges | Traversal Dynamics |
| :--- | :--- | :--- |
| **Level 01** | Parkour Stunt Track | Tests straight-line momentum, 45-degree ramp climbing, and precision jumps across cylinder bridges. |
| **Level 02** | Vertical Ascent | Evaluates slope stability and upward jump trajectories leading to an illuminated treasure room. |
| **Level 03** | Elevated Arena | Features diverging paths testing choice execution between safe solid perimeters and floating platforms. |
| **Level 04** | Pinch & Release Tunnel | Tests tight platforming through enclosed spatial pinches into open courtyard releases. |
| **Level 05** | Track Platforming | Features long-approach ramps, cylindrical tunnel navigation, and precise drop-down platform landings. |

---

## Short Answer Questions (Part D)

### 1. Why do we separate input reading in `Update()` from physics forces in `FixedUpdate()`?
`Update()` runs once per rendered frame, making it the ideal location to capture user keyboard/mouse inputs instantaneously without missing button presses (such as jump buffering). `FixedUpdate()` runs on a deterministic physics timestep (typically 50 Hz). Applying forces or velocity modifications inside `Update()` results in inconsistent movement behavior dependent on frame rate spikes or drops. Splitting the logic ensures reliable physics calculations while maintaining crisp input responsiveness.

### 2. How does camera-relative movement improve 3D platforming readability?
In a 3D environment with a free-orbiting camera, fixed world-axis controls (where pressing **W** always moves absolute World North) create severe cognitive friction whenever the camera rotates. By projecting **WASD** movement inputs onto the flattened horizontal plane of the camera (`camForward` and `camRight`), pressing **W** always pushes the character directly away from the player's screen view, making navigation intuitive regardless of camera orientation.

### 3. What role do Coyote Time and Jump Buffering play in game feel?
In strict physical simulations, pressing a jump key 1 frame before landing or 1 frame after leaving a ledge results in a non-input, leading to player frustration. Coyote time gives players a brief window ($0.1\text{s}$) to execute a jump after walking off a platform, while jump buffering holds pre-landing inputs ($0.175\text{s}$) to execute immediately upon touching down. Together, these fairness windows bridge the gap between player intent and execution.

---

## Video Showcase

- **Video Folder Link:** `https://drive.google.com/drive/folders/1RtQXvaG_EdV7HNikOdZDjYsWHAcSOooQ?usp=drive_link`
- **Showcase Highlights:**
  - Mouse-orbit camera with **ESC** toggle cursor locking.
  - Camera-relative WASD movement in action.
  - Demonstration of Coyote Time and Jump Buffering windows.
  - Full traversal across Levels 01 through 05.