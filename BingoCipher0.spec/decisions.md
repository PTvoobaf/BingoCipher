# Decisions and open questions

## Decided

- The repository is named `BingoCipher0` and will be private on `github.com/gibbonsc`.
- The default Git branch is `main`.
- The initial repository includes a short `README.md`, a .NET `.gitignore`, and a Creative Commons license notice.
- UK and US cipher messages use every marked numbered space on the selected winning ticket or card, not merely the spaces required for the winning state.
- Positive ticket/card values are unmarked; marking changes the sign to negative. Zero is reserved for UK blanks and the US free space and never carries plaintext.
- Text is restricted to the ASCII subset of UTF-8. UK randomized filler uses ASCII codes 32 through 126; US randomized filler uses `[A-Za-z]`.
- Bingo simulation and challenge generation are reproducible from an explicit seed.
- Challenge IDs are random by default and may be overwritten by an externally assigned arbitrary string.
- Each application produces two challenges per run: one decode challenge and one deterministic encode challenge.
- If a game simulation produces multiple winning tickets or cards, exactly one winner is selected for challenge generation; the others are not used for that challenge.
- The initial plaintext libraries use UK messages from 5 through 15 characters and US messages from 4 through 20 characters, with selection constrained to the number of marked spaces `k`.
- The UK winning ticket contributes between 5 and 15 marked numbered spaces for challenge-library purposes; the US winning card contributes between 4 and 20. The challenge generator must use a plaintext matching the selected winner's marked-space count.
- Implementations should use descriptive C# names for fields, properties, parameters, and local variables rather than mechanically preserving the single-letter mathematical labels in `algorithms.md`.
- User-facing options should be command-line named parameters with arguments, such as `--seed 42`; persistent configuration files are not preferred for these console applications.
- Console pretty-print representations use four-character-wide right-justified fields. UK ticket output has three newline-terminated rows; UK strip output has six ticket blocks separated by blank lines; US card output begins with `   B   I   N   G   O` and then has five newline-terminated rows. Marked signed values remain signed, and UK blank spaces are four spaces.
- UK strip generation randomly balances four extra column placements per strip column across six tickets, with no ticket column exceeding three numbers. This permits all four valid unordered partition types of fifteen into nine values from 1 through 3 and does not hard-code the former three-one/six-two profile.
- VS Code is the expected development and review environment.
- The repository-wide Creative Commons license applies to the specification markdown files.
- Milestone 3 challenge executables use `--encode` or `--decode`, exactly one of which is required.
- Challenge executables require `--seed` and `--id`; `--players` defaults to `21`; `--output-directory` defaults to `.`.
- The output directory must already exist. Existing challenge files are overwritten without warning.
- `--terminal` writes formatted JSON both to the output file and to standard output.
- UK challenges accept `--base64` for Base64 ciphertext instead of hexadecimal; the option is invalid for US challenges.
- Challenge output filenames are `UKencode-{id}.json`, `UKdecode-{id}.json`, `USencode-{id}.json`, and `USdecode-{id}.json`.
- Milestone 3 initially writes complete JSON objects containing both `plaintext` and `ciphertext`; competitor-facing filtering is deferred.
- The selected UK ticket or US card is serialized under the field name `winningTicket`.
- UK and US use separate embedded plaintext libraries. UK candidates may contain any ASCII printable characters; US candidates contain alphabetic characters only. UK has exact-length buckets from 5 through 15; US has exact-length buckets from 4 through 20. One through nineteen candidates are permitted per bucket so that curated counts can approximate the anticipated winner distributions while preserving memorable messages.
- Plaintext selection is uniform within the selected exact-length bucket, and randomized filler is uniform over the documented alphabet.
- The upstream ChaCha20 implementation is incorporated as source rather than a NuGet dependency, using RFC 8439/IETF ChaCha20 with the authoritative 256-bit key, 96-bit nonce, and directly supplied 32-bit initial counter `1`.
- Incorporated ChaCha source remains ISC-licensed. Its complete notice, upstream URL, and exact source revision will be recorded in third-party attribution material.
- The preferred attribution arrangement is a dedicated third-party notices document containing the complete ISC notice, upstream project URL, author attribution, and the exact source revision used. The BingoCipher0 source-code license remains a separate decision; Apache-2.0 is the current recommendation for permissive reuse with explicit patent language and warranty disclaimers.

## Open before implementation

- Record the exact upstream ChaCha20 source revision and verify it against RFC 8439 known-answer vectors and BingoCipher0-specific golden vectors.
- The supplied unpadded Base64 key and nonce labels have been verified to decode exactly to the hex key and nonce in `algorithms.md`; retain both forms until the chosen library/API is documented.
- Consult the CTF organizers about whether `--id` is assigned, selected by competitors, or derived from a separate Unicode display name. The machine ID must remain filename-safe and unique.
- Decide whether a later milestone should add an explicit competitor-facing JSON-filtering/publishing command.
- Curate and review the complete UK and US plaintext candidate libraries.
- Decide and document the BingoCipher0 source-code license, separately from the CC BY 4.0 specification/documentation license.
