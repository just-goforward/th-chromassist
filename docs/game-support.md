# Game support

## MVP: Touhou 18

The current adapter supports one locally observed installation only:

| Field | Value |
|---|---|
| Game | Touhou 18 ~ Unconnected Marketeers |
| Version label | v1.00a |
| Distribution | Steam original |
| EXE SHA-256 | `9ED66E6952459E81515C17A671410BEE7014A83E3C6CC6A7E360E7B4904C62F4` |
| DAT SHA-256 | `3949E7C01BDEF9C3FE75711E088BFE4E195F3A657585C79B6A1AFB9D117DC800` |
| Minimum tested thcrap | `2025-12-02` |

These hashes identify the user's local installation observed on 2026-07-15. They are not claimed to represent every legitimate Steam installation. Other hashes fail closed until independently inspected and added with evidence.

The adapter reads `bullet.anm` from `th18.dat` and uses THTK's unique texture extraction. The six current runtime replacement paths are:

```text
th18/bullet/bullet1@bullet@0.png
th18/bullet/bullet2@bullet@1.png
th18/bullet/bullet3@bullet@2.png
th18/bullet/bullet4@bullet@6.png
th18/bullet/bullet5@bullet@7.png
th18/bullet/bullet6@bullet@9.png
```

Items, lasers, effects, backgrounds, and additional sprite archives are outside the first implementation scope. Adding them requires role classification and separate fairness validation.
