# As-built Record

## Milestone 1 acceptance

Milestone 1 was accepted on 2026-09-26.

The repository now contains a .NET 10 solution with a core class library and xUnit v3 tests executed through Microsoft.Testing.Platform. The following classes were implemented:

- `UKBingoTicket`: validated 3-by-9 ticket construction, initialization, signed-number marking, UK line-win detection, and JSON array serialization/deserialization.
- `UKBingoStrip`: seeded or default-random generation of six valid UK tickets, exact coverage of numbers 1 through 90 across the strip, strip-level marking, and JSON array-of-arrays serialization/deserialization.
- `USBingoCard`: seeded or default-random generation of valid 5-by-5 US cards, column/range and center-free-space validation, signed-number marking, row/column/diagonal winning detection, and JSON array serialization/deserialization.

The US center coordinate is permanently represented by `0`, participates as a free space in winning-state evaluation, and is never treated as a numbered/plaintext space.

The test suite covers valid and invalid construction, documented invariants, marking behavior, winning-state behavior, seeded reproducibility where generation is randomized, and JSON round trips. The complete suite currently contains 35 passing tests.

## Milestone 2 implementation checkpoint

Work on Milestone 2 began after design review, but Milestone 2 is not yet accepted or complete.

The repository currently contains:

- `BingoCipher0.Sim`: a shared .NET 10 simulator library containing the stateful `NumberCaller`, deterministic random-stream derivation, named-argument parsing, UK/US player models, and simulation result types.
- `UKBingoSim`: a .NET 10 executable that simulates one strip per anonymous zero-based player and outputs JSON or terminal-formatted results.
- `USBingoSim`: a .NET 10 executable that simulates one card per anonymous zero-based player and outputs JSON or terminal-formatted results.
- `BingoCipher0.Sim.Tests`: simulator tests covering caller permutations and state, seeded reproducibility, winner detection, player-count validation, and decimal/hexadecimal command-line seeds.

The caller stores both the complete seeded permutation and the sequence called so far. Simulators stop at the first draw that produces a winner and return all winning cards/tickets present at that draw. UK strip constraints mean that one strip can produce at most one newly winning ticket on a draw; multiple winners remain possible across players. Cipher challenge selection and detailed JSON schema decisions remain deferred.

The combined test suite currently contains 42 passing tests. Cipher challenges and later milestones have not been started.

## Milestone 2 acceptance

Milestone 2 was accepted on 2026-09-27.

The repository now contains deterministic UK and US Bingo game simulators built on the Milestone 1 ticket, strip, and card classes:

- `BingoCipher0.Sim` provides shared simulator infrastructure, including the stateful `NumberCaller`, complete seeded no-replacement draw permutations, called-number state, deterministic purpose-specific random streams, named command-line argument parsing, player models, and simulation result types.
- `UKBingoSim` simulates one UK strip per anonymous zero-based player, draws through the first line winner, and returns all winning tickets produced at that draw.
- `USBingoSim` simulates one US card per anonymous zero-based player, draws through the first Bingo winner, and returns all winning cards produced at that draw.
- Both executables support decimal and hexadecimal `--seed` values, `--players`, and the `--terminal` switch. Default output is JSON; terminal output includes the called sequence and fixed-width pretty-printed winning tickets/cards.
- Simulator JSON retains the called sequence and marked winning ticket/card arrays. Detailed challenge JSON schemas remain deferred to later work.

The Milestone 1.1 console representation change added fixed-width `ToString()` output for UK tickets, UK strips, and US cards. The Milestone 1.2 UK occupancy change replaced the former fixed three-single/six-double column profile with randomized balanced strip generation that permits all valid ticket partition types while preserving five numbers per row, at least one number per column, and exact strip coverage of numbers 1 through 90.

The test suite covers caller state and permutations, simulator reproducibility, winner detection, validation, CLI seed forms, pretty-print formatting, and the existing Core invariants. The complete suite currently contains 46 passing tests. Cipher challenge implementation and later milestones have not been started.

## Milestone 3 sub-milestones

Milestone 3 is being implemented as seven review checkpoints. Work pauses after each checkpoint for human acceptance: 3.1 crypto incorporation, 3.2 shared challenge models and JSON, 3.3 CLI and file output, 3.4 plaintext libraries and seed streams, 3.5 UK challenge generation, 3.6 US challenge generation, and 3.7 end-to-end acceptance.

### Milestone 3.1 implementation checkpoint

Sub-milestone 3.1 is implemented and awaiting review. The repository now contains:

- `BingoCipher0.Challenges`, a .NET 10 class library;
- the self-contained `CSChaCha20.cs` source copied from upstream commit `dfbc5344b6c05d0ae9ddc22b5ba42234402e07be` under `ThirdParty`;
- `ChaCha20Cipher`, a narrow adapter that validates RFC 8439 key/nonce lengths and exposes symmetric encrypt/decrypt operations;
- `BingoCipher0.Challenges.Tests`, using xUnit v3 and Microsoft.Testing.Platform;
- `THIRD-PARTY-NOTICES.md` documenting upstream attribution and the ISC license.

The 3.1 tests cover the RFC 8439 section 2.3.2 known-answer vector, the BingoCipher0 key/nonce/initial-counter parameters, a fixed ciphertext regression value, round-trip decryption, and invalid key/nonce lengths. Upstream's existing NUnit suite also passed 26 tests during source verification. No JSON, CLI, plaintext-library, or challenge-generation work has begun.

### Milestone 3.2 implementation checkpoint

Sub-milestone 3.2 was accepted on 2026-09-30. `BingoCipher0.Challenges` contains a validated `CipherChallenge` model and indented JSON round-trip serialization for complete challenge objects. The model records schema version, UK/US type, encode/decode mode, challenge ID, seed, plaintext, ciphertext, ciphertext encoding, called numbers, and the selected `winningTicket`. Tests cover UK hexadecimal and US alphabetic objects, complete-field preservation, formatting, and validation failures.

### Milestone 3.3 implementation checkpoint

Sub-milestone 3.3 was accepted on 2026-09-30. `BingoCipher0.Challenges` contains named command-line parsing and formatted file output. The parser supports required `--seed`, `--id`, and exactly one mode switch; defaults `--players` to 21 and `--output-directory` to `.`; validates existing output directories and conservative filename-safe machine IDs; supports `--terminal`; and rejects `--base64` for US challenges. `ChallengeFileWriter` writes indented JSON to the agreed mode-specific filename and overwrites existing files. Challenge and full-solution regression tests cover the behavior.

### Milestone 3.4 implementation checkpoint

Sub-milestone 3.4 was accepted on 2026-10-02. `BingoCipher0.Challenges` contains separate embedded UK and US plaintext resources with UK buckets from 5 through 15 and US buckets from 4 through 20, one through nineteen candidates per bucket, strict variant-specific validation, exact-length selection, and domain-separated SHA-256-derived random streams for plaintext selection and future UK/US filler streams. The corpus excludes candidates with uppercase `X+` suffixes and includes recognizable suggestions such as `PrincessDonut`, `AntiochHandGrenade`, `SignalToNoiseRatio`, and `NeverGonnaGiveYouUp`. Candidate counts are curated to approximate anticipated winning-daub distributions, and UK candidates duplicated in the US corpus were removed. Challenge tests cover every bucket, character restrictions, seeded reproducibility, and stream-purpose separation.

### Milestone 3.5 implementation checkpoint

Sub-milestone 3.5 was accepted on 2026-10-03. The repository contains:

- `UKBingoCipher`, which validates winning-ticket state against the called sequence, extracts every marked numbered coordinate in call order, enforces exact plaintext-length matching, places plaintext in the 27-coordinate grid, and deciphers it through the same coordinate order;
- deterministic underscore filler for encode challenges and reproducible filler drawn only for unmarked coordinates from the domain-separated UK stream, uniformly over ASCII codes 32 through 126, for decode challenges;
- ChaCha20 transformation of the 27 ASCII grid bytes using the vendored ISC implementation, the established 256-bit key, 96-bit nonce, and initial counter `1`;
- lowercase hexadecimal ciphertext by default and Base64 ciphertext with `--base64`;
- `UKChallengeGenerator`, which runs the UK simulator, deterministically selects the first winner in stable ascending-player/strip-ticket order, selects a plaintext from the embedded 5–15 UK buckets matching the winner's complete marked-space count, and constructs the complete challenge object;
- the .NET 10 `UKBingoCipherChallenge` executable, integrated with the established named options, formatted complete JSON file output, and optional terminal output.

Challenge JSON now serializes `ciphertextEncoding` as the documented string values `Hex`, `Base64`, and `Alphabetic`. Tests cover call-order extraction and placement, length mismatch rejection, filler range and stream consumption, hex/Base64 ChaCha20 round trips, mark/call consistency, deterministic selection from simultaneous simulator winners, reproducible encode/decode generation, complete JSON retention, and executable smoke runs. The complete solution contains 88 passing tests under xUnit v3 and Microsoft.Testing.Platform on .NET 10. Sub-milestone 3.6 has not begun.

Acceptance included independent manual verification against an external ChaCha20 tool: deterministic encode grid encryption reproduced the generated hexadecimal ciphertext, decode ciphertext recovered a grid whose called-number coordinates spelled the expected plaintext after extraction, and `--base64` correctly changed the ciphertext representation.

### Milestone 3.6 implementation checkpoint

Sub-milestone 3.6 was accepted on 2026-10-03. The repository contains:

- `USBingoCipher`, which validates winning-card state against the called sequence, extracts every marked numbered coordinate in call order without extracting the center free space, enforces exact plaintext-length matching, and applies case-preserving ROT13 for encryption and decryption;
- deterministic `B` filler for encode challenges, including the free-space coordinate, and reproducible filler drawn only for non-plaintext coordinates from the domain-separated US stream, uniformly over `[A-Za-z]`, for decode challenges;
- `USChallengeGenerator`, which runs the US simulator, deterministically selects the first winner in stable ascending-player order, selects a plaintext from the embedded 4–20 alphabetic buckets matching the winner's complete marked-space count, and constructs the complete challenge object;
- the .NET 10 `USBingoCipherChallenge` executable, integrated with the established named options, 25-character alphabetic ciphertext, formatted complete JSON file output, and optional terminal output.

Tests cover uppercase and lowercase ROT13, minimum four-daub free-space wins, call-order extraction and placement, exact length validation, deterministic and randomized filler including the free space, `[A-Za-z]` stream consumption, alphabetic ciphertext validation, encode/decode round trips, simultaneous simulator winner selection, reproducible generation, and complete JSON retention. Both encode and decode executable smoke runs succeeded. Acceptance included manual execution of several hundred seeded `USBingoCipherChallenge` encode invocations. The expanded UK and US plaintext libraries pass their full validation coverage. The complete solution contains 106 passing tests under xUnit v3 and Microsoft.Testing.Platform on .NET 10.

Sub-milestone 3.7 has not begun and is intentionally paused. Requirements for integrating the challenge generators with the competition CTFd server will be defined after hands-on exploration of that server; no 3.7 implementation is authorized before those requirements are available.
