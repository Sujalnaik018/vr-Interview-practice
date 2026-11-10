# Unity 3D Setup & Execution Guide — VR Interview Practice System

This guide walks you through setting up and running the 3D interview simulator in Unity, designed to be 100% beginner-friendly and dependency-free.

---

## Prerequisites

1. **Unity Hub**: Download from [unity.com/download](https://unity.com/download).
2. **Unity Editor Version**: Recommended **Unity 2022.3 LTS** (or Unity 2021.3 LTS / Unity 6).
   - In Unity Hub, go to **Installs** → **Install Editor** → Select **2022.3 LTS** (Standard 3D Core template).
3. **Microphone**: Any built-in laptop microphone or external USB headset.

---

## Method 1: Instant Setup (Recommended — 1-Click Generation)

We have included an automated Editor tool (`SetupInterviewScene.cs`) that creates the 3D office, materials, lighting, interviewer avatar, Canvas UI panels, and links all Inspector references in seconds.

### Step-by-Step Instructions:

1. **Create a New Unity Project**:
   - Open **Unity Hub**.
   - Click **New Project**.
   - Select **3D (Core)**.
   - Name the project `VRInterviewPractice` and choose your preferred location.
   - Click **Create Project** and wait for Unity to open.

2. **Copy the Assets Folder**:
   - Copy the contents of this project's `unity/Assets/` folder into your new Unity project's `Assets/` directory:
     - `Assets/Scripts/`
     - `Assets/Editor/`

3. **Generate the Scene**:
   - In Unity's top menu bar, you will see a new menu:
     `Tools` → `VR Interview Practice` → `Generate Complete Scene`.
   - Click **Generate Complete Scene**.
   - Click **Generate Scene** in the confirmation dialog.
   - *Result*: The 3D executive office room, virtual interviewer, seated camera, lighting, and all Canvas UI panels (Main Menu, Interview HUD, Results, History, Loading overlay) are instantly built and connected!

4. **Run the Simulator**:
   - Ensure the Python Flask backend is running (`python app.py` on `http://127.0.0.1:5000`).
   - Press the **Play (▶)** button at the top of the Unity Editor.
   - Enter your name, choose **Behavioral / HR** or **Technical Systems**, and click **START PRACTICE SESSION**!

---

## Method 2: Manual Inspector Setup (For Learning & Viva Demonstration)

If your college professor asks you how the scene is structured or you want to configure components manually:

### 1. Main Camera Rig
- Position: `(X: 0, Y: 1.25, Z: -0.6)`
- Rotation: `(X: 5, Y: 0, Z: 0)`
- Clear Flags: `Skybox`
- Add Component: `CameraLookController.cs` (enables seated right-click neck rotation).

### 2. Office Environment
- Create an empty GameObject named `Office_Environment`.
- Add Component: `OfficeRoomGenerator.cs`.
- Right-click the component header in the Inspector and click **Build Office Room**.
- Standard primitives (Cube, Cylinder, Sphere) form the room walls, ceiling, mahogany desk, chairs, laptop, whiteboard, and the 3D virtual interviewer avatar with `VirtualInterviewer.cs`.

### 3. Master Controller (`AppManager`)
- Create an empty GameObject named `AppManager`.
- Add Components:
  - `NetworkClient.cs` (`Backend Base Url`: `http://127.0.0.1:5000`)
  - `MicrophoneRecorder.cs` (`Sample Rate`: `16000`, `Max Recording Duration`: `60`)
  - `GameManager.cs` (Master state coordinator)
- In the `GameManager` Inspector, drag and drop the UI Panels and scripts into their corresponding serialized slots.

---

## Controls & Usage

| Action | Control |
| :--- | :--- |
| **Look Around Seated Desk** | Hold **Right Mouse Button** and move mouse |
| **Reset Camera Angle** | Release Right Mouse Button |
| **Typed Answer Fallback** | Click the answer box, type response, press Next Question |
| **Voice Answer Recording** | Click **Record Voice Answer**, speak clearly, click **Stop Recording** |
| **Microphone Meter** | The green meter bar reacts dynamically to your voice volume |
| **Finish Session** | Click **Finish Interview** on the final question to trigger AI evaluation |

---

## Troubleshooting Common Issues

1. **"Backend: Offline (Start Flask)"**:
   - Open a terminal/command prompt.
   - Navigate to `backend/` and run `python app.py`.
   - Ensure `http://127.0.0.1:5000/health` returns `status: healthy` in your web browser.

2. **Microphone Not Recording**:
   - Check Windows Settings → **Privacy & Security** → **Microphone** → Ensure "Let desktop apps access your microphone" is **On**.
   - In Unity, you can always use the typed answer fallback if a microphone is not connected.

3. **Audio Level Bar Doesn't Move**:
   - Speak with normal conversational volume.
   - Verify that your default audio input device is active in Windows Sound Settings.
