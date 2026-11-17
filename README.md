# Project Echo — Unity 6.6 MVP vertical slice

Project Echo is a single-player life-simulation prototype about creating an artificial lifeform, caring for it, and seeing its personality and memories emerge. This MVP focuses on the emotional core: Sanctuary care, deterministic expedition choices, a persistent memory journal, and visible mutation progression.

**Have an idea?** Feature suggestions are welcome. [Open a feature suggestion](https://github.com/aryansh1212123-pro/project-echo-unity6-6-mvp/issues/new?template=feature_request.yml) or read [CONTRIBUTING.md](CONTRIBUTING.md).

## What is included

- **Glass Jungle and Sanctuary:** generated Unity scene with a procedural Echo, crystal canopy, ruin fragments, shifting fog, and a distant predator silhouette. No external art packages are required.
- **One Starter Echo:** generated name and profile, five personality traits, five needs, one player relationship, visible behavior state, and an explanation of its current decision.
- **Deterministic utility behavior:** unmet needs, traits, trust, and remembered predator/rescue experiences influence the Echo's decisions. Behavior selection is not random.
- **Expeditions:** three situations per journey, with two choices each. Outcomes are computed from the Echo's traits, needs, player trust, and relevant memories. Journeys can create rescue, ruin, discovery, predator, and homecoming memories.
- **Memory archive:** personal, environmental, and social memories record tags, emotional value, location, importance, time, participants, and strength. Remembered predator danger influences later decisions.
- **Sanctuary care:** feed, soothe, play, or rest. Each action changes needs or relationships and creates a social memory.
- **Gene Forge:** ten mutations with material costs. Several change the Echo's appearance; Regeneration, Deep Memory, Neural Creativity, Echo Sense, Defensive Shell, and Neural Link also affect simulation behavior or values.
- **Persistence:** versioned JSON save with backup fallback under `Application.persistentDataPath`.
- **GitHub Actions:** `.github/workflows/unity-build.yml` builds a Windows player using Unity 6.6 and uploads it as a downloadable workflow artifact when license secrets are configured.

## Open and run in Unity 6.6

1. Install **Unity 6.6 (`6000.6.8f1`)** in Unity Hub. This repository pins the project to that editor revision.
2. For a local Windows player build, install **Windows Build Support** in Unity Hub's install modules. The Editor workflow can also be used to run the game without building a standalone player.
3. In Unity Hub, choose **Add project from disk** and select the cloned repository folder. Let Unity import the project and compile its scripts.
4. From the Unity menu, choose **Project Echo → Create or Reset Playable Scene**. This generates `Assets/Scenes/ProjectEcho.unity` and `Assets/Resources/EchoTuning.asset`.
5. Press **Play**. Use the top navigation for Sanctuary, Expedition, Memory Archive, and Gene Forge. Expedition choices are made with the mouse.
6. To create a local Windows demo, choose **Project Echo → Build Windows Demo**. The output is `Build/ProjectEcho.exe` and its accompanying data folder.

The project uses built-in Unity primitives and IMGUI and has no third-party Unity package dependencies. If you open it using another editor version and Unity prompts to upgrade the project, make a copy first; the recommended version is the pinned Unity 6.6 editor.

## GitHub Actions build

The workflow runs on pushes to `main`, pull requests targeting `main`, and manual runs from the **Actions** tab. It uses GameCI's Unity Builder to target `StandaloneWindows64`. The editor method generates the scene and tuning asset in batch mode before building; the resulting player is uploaded as `ProjectEcho-Windows-Unity-6.6` for 14 days.

Unity requires an activated license for CI builds. Configure GitHub Actions secrets under **Settings → Secrets and variables → Actions**:

- For a Unity Personal license, add `UNITY_LICENSE` with the contents of your activated `.ulf` file, plus `UNITY_EMAIL` and `UNITY_PASSWORD` for the Unity account.
- For a Unity Professional license, add `UNITY_EMAIL`, `UNITY_PASSWORD`, and `UNITY_SERIAL`.

The workflow checks for credentials without printing them. If they are not configured, it skips the Unity build and emits a warning, so public forks and pull requests do not fail merely because secrets are unavailable. Do not commit license files, account credentials, or serial keys to the repository.

## Suggest features or contribute

Please share ideas through the [Feature suggestion issue form](https://github.com/aryansh1212123-pro/project-echo-unity6-6-mvp/issues/new?template=feature_request.yml). Explain what experience you want players to have and how it could deepen the bond with an Echo. Ideas about personality, memory, relationships, exploration, and Sanctuary life are especially welcome.

For code changes, read [CONTRIBUTING.md](CONTRIBUTING.md). This project intentionally avoids multiplayer, PvP, live-service systems, premium currencies, and battle passes.

## Project structure

```text
.github/
  ISSUE_TEMPLATE/feature_request.yml    Structured community feature-idea form
  workflows/unity-build.yml             Unity 6.6 Windows build workflow
Assets/
  Editor/ProjectEchoSetup.cs            Scene generation and local/CI build entry points
  Resources/EchoTuning.asset             Generated when the scene setup command runs
  Scenes/ProjectEcho.unity               Generated when the scene setup command runs
  Scripts/
    Data/EchoData.cs                     Serializable simulation/save models
    Data/EchoTuning.cs                   ScriptableObject balance and encounter content
    Systems/EchoServices.cs              Memory, needs, traits, relationships, mutations
    Systems/EchoBrain.cs                 Deterministic utility planner and action state
    Systems/EchoSaveManager.cs           Versioned JSON persistence
    World/EchoExpeditionDirector.cs      Expedition events and consequences
    World/EchoSceneBootstrap.cs          Runtime composition, world, and Echo actor
    UI/EchoHUD.cs                        Sanctuary, journal, expedition, mutation UI
Packages/manifest.json
ProjectSettings/ProjectVersion.txt
CONTRIBUTING.md
README.md
```

## Current status

This is a **source-level MVP vertical slice**, not a finished commercial Early Access release. The project has not been compiled or play-tested in this environment. Run it in the pinned Unity editor and resolve any local editor or platform-module prompts.

Before a Steam launch, it still needs a Unity build and QA pass on target PCs, rigged and animated creature/environment art, authored audio, accessibility and settings, controller support, tutorial/onboarding, performance profiling, crash/backup testing, Steamworks integration and store assets, legal/privacy review, and release configuration. This repository does not publish to Steam or claim a public launch is live. It deliberately has no combat, online features, monetization, or creature-loss mechanic.

## Save location

Unity writes runtime data to `Application.persistentDataPath/project_echo_save.json`, with a `.bak` fallback. Back up this file before changing save schemas during development.
