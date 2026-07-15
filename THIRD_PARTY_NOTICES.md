# Third-party notices

## Touhou Toolkit (THTK)

Portable release packages include selected binaries from [thpatch/thtk](https://github.com/thpatch/thtk), obtained from the official x64 nightly channel. THTK uses a BSD-style license. Its distribution also includes libpng and zlib notices. The packaging script copies `COPYING.txt`, `COPYING.libpng.txt`, and `COPYING.zlib.txt` beside the tools.

The repository does not commit the THTK binaries. `scripts/package-portable.ps1` downloads one allowlisted archive and rejects any other SHA-256 before extraction.

Observed and allowlisted on 2026-07-15:

- Source commit: `892114a0fcaa0bbdaaecf3cb4ad56f758683fb40`
- x64 nightly ZIP SHA-256: `4B7C193434A52CA6FC418F243637B6971E6409AE1FB0F7C01F18E0645405EDFB`
- `thdat.exe` SHA-256: `EF494069A048238948E4B8769A955935DAAD6061870A99B8CB230B64BD84AF9B`
- `thanm.exe` SHA-256: `3182BE234BFBCC8480A883A52E84D7218F1AB7C28D5E75A2D98A4E498F6B9B8C`

The official nightly URL is mutable. Updating the allowlist requires reviewing the upstream source, capabilities, licenses, and hashes.

## thcrap

Chromassist interoperates with a thcrap installation supplied by the user. It does not redistribute thcrap. thcrap is a separate project with its own license and notices.
