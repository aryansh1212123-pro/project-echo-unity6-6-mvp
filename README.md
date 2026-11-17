# Project Echo — Unity 6 MVP vertical slice

A small, self-contained Unity source project built from the supplied Project Echo brief. It focuses on the emotional core: care in the Sanctuary, deterministic expedition choices, personality shaped by experience, a persistent memory journal, and visible mutation progression.

## What this build includes

- **Playable Glass Jungle scene:** generated from Unity primitives at runtime, with a procedural Echo, crystal canopy, ruin fragments, softly shifting fog, and a distant predator silhouette. No external art packages are required.
- **One named Starter Echo:** generated profile, five traits, five needs, one player relationship, visible behavior state and explanation.
- **Deterministic utility behavior:** the Echo weighs unmet needs, traits, trust, and remembered predator/rescue experiences to choose actions. It does not use random behavior selection.
- **Three-choice expedition loop:** three situations per trip, with two meaningful options each. Outcome scores are deterministic and based on the favored trait, current safety, player trust, and relevant prior memories. Journeys can create rescue, ruin, discovery, predator, and homecoming memories.
- **Memory archive:** personal, environmental, and social records carry tags, emotional value, location, importance, time, participants, and strength. Predator memories make the Echo more cautious in subsequent decisions.
- **Sanctuary care actions:** feed, soothe, play, or rest; each changes needs/relationship and records a social memory.
- **Gene Forge:** ten mutations, with material costs. Several alter the model immediately; Regeneration, Deep Memory, and Neural Creativity also affect simulation values.
- **Persistence:** versioned JSON save with backup attempt under `Application.persistentDataPath`. Profile, needs, traits, memories, relationship, mutations, resources, days, and expedition count persist.
- **Windows demo build command:** an Editor menu command can build a standalone player if the Windows Build Support module is installed.

## Open and run

1. Install **Unity 6.6 (`6000.6.8f1`)** in Unity Hub. This project is pinned to that editor revision. Add **Windows Build Support (IL2CPP or Mono)** if you want a Windows executable.
2. In Unity Hub, add this folder as a project. Allow Unity to import and compile the scripts.
3. In the editor, choose **Project Echo → Create or Reset Playable Scene**. The command creates a tuning asset, generates `Assets/Scenes/ProjectEcho.unity`, and adds it to Build Settings.
4. Press **Play**. Use the top navigation for Sanctuary, Expedition, Memory Archive, and Gene Forge. Expedition choices are made with the mouse.
5. To create a local Windows demo, choose **Project Echo → Build Windows Demo**. The output is `Build/ProjectEcho.exe` plus its data folder.

The project uses built-in Unity primitives and IMGUI; it has no third-party package dependencies. If you open it in a different editor version and Unity prompts to upgrade the project, make a copy first; the recommended editor is the pinned 6.6 version.

## Project structure

```text
Assets/
  Editor/ProjectEchoSetup.cs          Scene generation and Windows demo build menu
  Resources/EchoTuning.asset          Created by the Editor setup command
  Scenes/ProjectEcho.unity            Created by the Editor setup command
  Scripts/
    Data/EchoData.cs                  Serializable simulation/save models
    Data/EchoTuning.cs                ScriptableObject balance and encounter content
    Systems/EchoServices.cs           Memory, needs, traits, relationship, mutations
    Systems/EchoBrain.cs              Deterministic utility planner and action state
    Systems/EchoSaveManager.cs        Versioned JSON persistence
    World/EchoExpeditionDirector.cs   Expedition events, choices, consequences
    World/EchoSceneBootstrap.cs       Scene, procedural world, and Echo actor
    UI/EchoHUD.cs                     Sanctuary, journal, expedition, mutation UI
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
```

## Honest launch status

This is a **source-level MVP vertical slice**, not a finished commercial Early Access release. The sandbox used to generate it does not have Unity installed, so the project has not been compiled or play-tested here. Unity generates the scene from the included menu command; run it locally and resolve any editor-version or platform-module prompts.

Before an actual Steam launch, the project still needs a Unity build and QA pass on target PCs, proper rigged/animated creature and environment art, authored audio/music, accessibility and settings, input/controller support, tutorial/onboarding, performance profiling, crash/backup testing, Steamworks integration and store assets, legal/privacy review, release configuration, and a Steam partner account/build upload. This package does **not** publish anything to Steam or claim a public launch is live. It deliberately has no combat, online features, monetization, or creature-loss mechanic.

## Save location

Unity writes the runtime save at `Application.persistentDataPath/project_echo_save.json`, with `.bak` fallback. Use a new profile or back up that file before changing save schemas during development.
