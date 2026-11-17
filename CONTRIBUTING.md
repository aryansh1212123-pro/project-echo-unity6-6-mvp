# Contributing to Project Echo

Thanks for helping shape Project Echo. This is an early MVP, and thoughtful feature ideas are welcome.

## Suggest a feature

Open a [Feature suggestion](https://github.com/aryansh1212123-pro/project-echo-unity6-6-mvp/issues/new?template=feature_request.yml) issue. Describe the player experience you want, how it could deepen the bond with an Echo, and any trade-offs or risks. Please search existing issues first so related ideas can be discussed together.

Ideas that strengthen emotional attachment, personality emergence, memory, exploration, or Sanctuary life are especially relevant. Keep suggestions focused; the project intentionally avoids multiplayer, PvP, live-service systems, premium currencies, and battle passes.

## Contribute code

1. Fork the repository and create a branch for your change.
2. Open the project using Unity 6.6 (`6000.6.8f1`).
3. Keep simulation behavior deterministic and explainable. Prefer data-driven tuning and avoid adding dependencies without a clear need.
4. Test the change in the Unity Editor. If you have configured Unity license secrets, the Windows build workflow runs on pushes and pull requests to `main`.
5. Open a pull request. Explain what changed, why it helps the player experience, and how you tested it. Include screenshots or a short clip for visual changes when possible.

The CI workflow needs Unity license credentials configured as GitHub Actions secrets. Pull requests from forks do not receive repository secrets, so their Unity build step is skipped until an authorized maintainer runs it with credentials available.
