# AGENTS.md

## Non-negotiable constraints

- Never add Touhou original, extracted, translated, or transformed game assets to this repository.
- Never modify or repack a user's game EXE, DAT, ANM, ECL, music, or existing third-party patch.
- All game access is opt-in, local-only, read-only at the source, and limited by an exact asset-set allowlist.
- Unknown executable, DAT, texture, adapter, or patch-stack hashes fail closed.
- Generated patches live outside the repository and must not be offered for upload or sharing.
- Preserve image dimensions, alpha bytes, fully transparent RGBA bytes, sprite/frame layout, and animation timing.
- Do not change silhouettes, hitboxes, trajectories, speed, count, lifetime, game logic, backgrounds, effects, or outlines.
- Preset thresholds are versioned experimental defaults, never medical or scientific constants.
- Do not claim perceptual equivalence without preregistered user evidence and stated equivalence bounds.
- Use synthetic fixtures in automated tests. Never use game screenshots or pixels as test fixtures.
- Keep Korean, Japanese, and English UI resources structurally aligned; document language-specific layout differences.
- Application source is C# and XAML. Generated thcrap `.js` files are JSON configuration, not JavaScript source.
