# Color science status

The prototype converts sRGB to linear RGB and OKLab/OKLCH, applies bounded hue/chroma changes, and converts back to sRGB. It records mean and maximum OKLab distance per generated texture. Lightness is held constant in OKLCH, but gamut clipping can still change realised lightness slightly.

The current preset parameters are engineering placeholders. They are not derived from a validated color-vision-deficiency optimisation, do not reproduce an individual user's perception, and must not be presented as medical advice or a proven equalisation method.

Machado et al. is a candidate for future Protan/Deutan simulation with continuous severity. Before embedding matrices, the project must resolve their redistribution terms and independently verify interpolation and linear-RGB application. Tritan support should be evaluated separately; a single model should not be assumed best for every deficiency.

Before a stable preset release, the optimiser must add:

- an explicit role map for every affected sprite region;
- a CVD simulation implementation with source and version identifiers;
- pairwise separation metrics under the selected simulation;
- lightness-distribution and contrast-change caps;
- measurements across representative backgrounds;
- user override and affected-user A/B evidence.
