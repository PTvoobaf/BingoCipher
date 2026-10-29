# BingoCipher suite overview

## Purpose

`UKBingoCipherChallenge` and `USBingoCipherChallenge` are distinct .NET 10 console applications. Each generates a competition ready pair of CTF-style cipher challenges derived from a single Bingo game.

Competitors will be referred to [Bingo (British version)](https://en.wikipedia.org/wiki/Bingo_(British_version)) and [Bingo (American version)](https://en.wikipedia.org/wiki/Bingo_(American_version)) for Bingo game rules.

Each executable invocation of `UKBingoCipherChallenge` or `USBingoCipherChallenge` produces one selected challenge mode from a reproducible Bingo simulation. The required `--encode` or `--decode` switch determines whether the output is an encode or decode challenge; invoking the executable separately produces the other mode.

- A **decode challenge**: the competitor is given a ciphertext and game context, and must recover the corresponding plaintext flag to submit.
- An **encode challenge**: the competitor is given a plaintext and game context, and must produce the deterministic ciphertext flag to submit. The deterministic form uses fixed filler characters for unmarked spaces, so the expected ciphertext is reproducible.

Challenge generation, including the Bingo simulation and randomized filler used by decode challenges, is reproducible from a supplied seed. Challenge IDs are generated randomly by default, but may be replaced by an externally assigned string.

## Competition Use

| File | What the competitor receives | The flag the competitor must produce |
| --- | --- | --- |
| `UKencode-{challengeId}.json` | Plaintext, call sequence, winning strip | 27-character ciphertext |
| `UKdecode-{challengeId}.json` | 27-character ciphertext, call sequence, winning strip | Plaintext message |
| `USencode-{challengeId}.json` | Plaintext, call sequence, winning card | 25-character ciphertext |
| `USdecode-{challengeId}.json` | 25-character ciphertext, call sequence, winning card | Plaintext message |

Milestone 3 initially writes complete JSON objects containing both `plaintext` and `ciphertext`; competitor-facing filtering is deferred to a later milestone. The complete schema and formatted-output rules are recorded in `formats.md`.

## Incorporated specifications

- `algorithms.md` encode and decode procedures
- `formats.md` JSON file formats and examples
- `milestones.md` development milestones for the console applications
- `decisions.md` recorded decisions and unresolved planning questions
