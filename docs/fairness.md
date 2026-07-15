# Fairness envelope

Chromassist is intended to recover visual distinguishability, not to increase gameplay information.

## Allowed

- RGB changes to gameplay-relevant textures;
- bounded hue and chroma changes;
- limited lightness adjustment after a documented threshold is adopted;
- preservation of existing palette-role relationships where possible.

## Forbidden

- size, resolution, alpha mask, silhouette, hitbox, trajectory, speed, count, animation timing, or game logic changes;
- background or screen-effect removal;
- added outlines, letters, symbols, patterns, or markers;
- visibility increases without a documented cap.

The automated prototype currently enforces dimensions, alpha bytes, and fully transparent RGBA bytes. It records color difference but does not yet enforce empirically justified lightness or contrast thresholds. Therefore every non-original preset is labeled `experimental_unvalidated`.

The intended future comparison is:

1. measure the original palette under a normal-vision baseline;
2. measure the candidate under the user's selected CVD condition;
3. approach the baseline without materially exceeding it;
4. cap original-to-candidate lightness and background-contrast changes;
5. combine metrics with affected-user A/B testing and normal-vision blind review.

All initial thresholds must be configuration values described as experimental defaults, not scientific constants.
