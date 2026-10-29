# Milestones

## proposed milestones

Each milestone is worked one at a time. Human review, adjustments of unit tests, and iterative refinement happens during each milestone. Human user must accept satisfactory milestone criteria before proceeding to work on the next milestone.

### 1. Create classes and methods to create Bingo tickets

1. Create a class UKBingoTicket with a default constructor that produces an empty ticket without any numbers (all 27 coordinates contain `0` representing empty), a method to initialize an empty ticket with given arguments after verifying that the resulting ticket adheres to the constraints of a UK style bingo ticket, a method to mark a given number if it is found on the ticket, a method to return a boolean value true if the ticket is a line-winner (has all five numbers in a row marked), and methods to serialize/deserialize a ticket in JSON format.
2. Create a class UKBingoStrip with a default constructor that produces and initializes a strip of six tickets, according to the constraints of UK-style tickets and strips.
3. Create a class USBingoCard with a default constructor that initializes its 25 elements with 0 in the center "free space" and the other 24 with random positive integers according to the constraints of a Bingo card, a method to mark a given number if it is found on the ticket, a method to return a boolean value true if the ticket is a winner (has all five numbers in a row or column or diagonal marked non-positive), and methods to serialize/deserialize a ticket in JSON format.

### 2. Create additional classes to create Bingo game simulators

1. Create necessary classes to simulate a UK-style Bingo game using a number caller object and one or more player objects, each playing one strip of six tickets. The caller draws and calls numbers until at least one player achieves a line and wins. The simulator outputs JSON of the sequence of numbers called and the winning ticket(s).
2. Create necessary classes to simulate a US-style Bingo game using a number caller object and one or more player objects, each playing one card. The caller draws and calls numbers until at least one player achieves a Bingo (row, column, or diagonal marked) and wins. The simulator outputs JSON of the sequence of numbers called and the winning ticket(s).

### After the simulators are implemented

Wait to specify further milestones leading to implementation of `UKBingoCipherChallenge` and `USBingoCipherChallenge` until after the first two milestones are completed.

## 1.1 Console representations

As a Milestone 1 change order, override `ToString()` on `UKBingoTicket`, `UKBingoStrip`, and `USBingoCard` for readable console output. UK tickets use three newline-terminated rows of four-character fields, UK strips contain six such ticket blocks separated by blank lines, and US cards use a header followed by five rows of four-character fields. This is an intermediate Milestone 1.1 change and does not advance or complete Milestone 2.

## 1.2 UK ticket occupancy generation

As a Milestone 1 change order, revise `UKBingoStrip` generation so valid partitions of fifteen numbered spaces across nine columns are not restricted to the former three-one/six-two pattern. The generator randomly balances four extra column placements per strip column across the six tickets, subject to no ticket column exceeding three numbers, then generates valid row occupancies. This admits all valid partition types, including columns containing three numbers, without using the former fixed profile. This is an intermediate Milestone 1.2 change and does not advance or complete Milestone 2.

## 3. UK and US cipher challenges — implementation in progress

Milestone 3 will add shared challenge-generation, crypto, and plaintext-library projects plus the `UKBingoCipherChallenge` and `USBingoCipherChallenge` executables. It will use .NET 10, xUnit v3, Microsoft.Testing.Platform, named command-line options, and seeded reproducibility.

The executable contract is currently:

- required `--seed`;
- required `--id`;
- mutually exclusive required `--encode` or `--decode`;
- optional `--players`, defaulting to `21`;
- optional `--output-directory`, defaulting to `.` and requiring an existing directory;
- optional `--terminal`, which also prints formatted JSON;
- optional UK-only `--base64`;
- existing output files are overwritten without warning.

Milestone 3 initially produces complete, indented JSON containing both plaintext and ciphertext. Competitor-facing field filtering is explicitly deferred. Output names are `UKencode-{id}.json`, `UKdecode-{id}.json`, `USencode-{id}.json`, and `USdecode-{id}.json`.

The implementation must verify the vendored ISC ChaCha20 source against RFC 8439 vectors and BingoCipher0 golden vectors, test deterministic winner selection, exact plaintext-length matching, independent seeded streams, uniform filler alphabets, JSON round trips, Base64 and hexadecimal UK output, and CLI validation.

The complete plaintext libraries and the relationship between human display names and machine challenge IDs remain subject to review with the CTF organizers. No later milestone is authorized by this planning entry.

### 3.1 Crypto incorporation

Copy the self-contained upstream `CSChaCha20.cs` source into `BingoCipher0.Challenges`, retain its original ISC copyright header, add third-party attribution, and expose a narrow BingoCipher0 adapter. Verify RFC 8439 section 2.3.2, the authoritative BingoCipher0 key/nonce/counter parameters, and invalid parameter handling. Pause for review and acceptance before beginning 3.2.

### 3.2 Shared challenge models and JSON

Define complete challenge objects, JSON serialization, schema versioning, `winningTicket`, and UK hexadecimal/Base64 ciphertext representation. Accepted on 2026-09-30.

### 3.3 CLI and file output

Implement named option parsing, validation, output-directory handling, overwrite behavior, formatted file output, and `--terminal`. Accepted on 2026-09-30.

### 3.4 Plaintext libraries and seed streams

Add separate embedded UK and US plaintext libraries, exact-length selection, validation, and independent deterministic seed streams. Accepted on 2026-10-02.

### 3.5 UK challenge generation

Implement UK grid placement, deterministic/randomized filler, ChaCha20 encryption/decryption, and UK executable integration. Pause for review and acceptance before beginning 3.6.

Accepted on 2026-10-03. The implementation extracts every marked numbered coordinate in call order, selects the first simulator winner in stable player/strip order, matches a UK plaintext from the 5–15 library to the marked-space count, uses underscores for encode filler and the domain-separated seeded UK ASCII-printable filler stream for decode filler, applies the established ChaCha20 parameters, and emits hexadecimal or optional Base64 ciphertext through `UKBingoCipherChallenge`. Independent manual verification reproduced an encode ciphertext with an external ChaCha20 tool, recovered a decode grid with the same tool, and confirmed Base64 rendering.

### 3.6 US challenge generation

Implement US ROT13, alphabetic filler, card extraction, and US executable integration. Pause for review and acceptance before beginning 3.7.

Accepted on 2026-10-03. The implementation extracts every marked numbered coordinate in call order while excluding the free space, selects the first simulator winner in stable player order, matches a US plaintext from the 4–20 alphabetic library to the marked-space count, applies case-preserving ROT13, uses `B` for deterministic encode filler and the domain-separated seeded `[A-Za-z]` stream for decode filler, and emits 25-character alphabetic ciphertext through `USBingoCipherChallenge`. Acceptance included manual execution of several hundred seeded encode challenges through the executable. Expanded UK and US plaintext libraries continued to pass all validation and solution tests.

### 3.7 End-to-end acceptance

Verify reproducibility, simultaneous winners, all supported plaintext lengths, both challenge modes, both UK ciphertext encodings, JSON round trips, and CLI failure cases. Human acceptance of 3.7 completes Milestone 3.

Implementation is paused pending exploration of the competition CTFd server and definition of solid requirements for how the challenge generators must interface with it. Do not begin sub-milestone 3.7 until those requirements are supplied and implementation is explicitly authorized.
