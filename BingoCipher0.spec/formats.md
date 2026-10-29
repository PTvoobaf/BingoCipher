# Data Formats

The target language and platform is C# and .NET 10, so suggested data formats show C# types for volatile memory data structures. Suggested persistent storage options are JSON.

## Sequence of called numbers (whether UK or US)

- in volatile memory: `List<int>`
- persistent storage: a JSON array of integers

## UK-style Bingo ticket

- in volatile memory: a `List<int>` of size 27. Each element is either 0 for a blank space, a positive integer between 1 and 90 (inclusive) for an unmarked number, or a negative integer between -1 down through -90 (inclusive) for a marked number.
- persistent storage: a JSON array of 27 integers
- implicit: the 27 integers are in row major order: the first nine are in the top row, the second nine in the middle row, and the last nine in the bottom row.
- constraints, as described at [Bingo (British version)](https://en.wikipedia.org/wiki/Bingo_(British_version)): Each row contains five numbers and four blanks. Each number is distinct, meaning that no number is found at more than one coordinate of a ticket. Each column contains at least one number. The numbers in a column are constrained by distance from the left: the first column contains up to three numbers between 1 and 10 (-1 and -10 if marked), the second column contains numbers between 11 and 20, and so forth to the rightmost column that contains numbers between 81 and 90.

### UK-style Bingo strip

- in volatile memory: `List<List<int>>`
- persistent storage: a JSON array of arrays
- additional constraint: a strip has six tickets, with every number between 1 and 90 appearing exactly once in the unmarked strip.

The generator may use any per-ticket column-count partition of fifteen into nine values from 1 through 3, subject to five numbers in each row and at least one number in each column. Strip generation balances the column counts so every strip column contains ten numbers, while randomizing the valid occupancy profiles and row placement.

## US-style Bingo card

- in volatile memory: List<int>
- persistent storage: a JSON array of 25 integers
- implicit: the 25 integers are in row major order: the first five are the top row, the next five the second row, and so forth to the last five in the fifth (bottom) row.
- constraints, as described at [Bingo (American version)](https://en.wikipedia.org/wiki/Bingo_(American_version)): The center "free space" coordinate always contains the number `0`. The leftmost (first) column contains five distinct numbers between 1 and 15. The second column contains five distinct numbers between 16 and 30. The middle (third) column contains four distinct numbers between 31 and 45, reserving the middle "free space" coordinate to contain `0` instead. The fourth column contains numbers between 46 and 60. The rightmost (last) column contains five distinct numbers between 61 and 75. Initially, all numbers are unmarked; a marked number is represented by changing its sign to negative. No plaintext character is associated with a `0` space.

## Shared marking rules

For both ticket types, a positive number is unmarked and the corresponding negative number is marked. Marking changes the sign of the integer. Zero is reserved for blank UK spaces and the US free space; zero is never associated with a plaintext character.

The winning state determines which ticket or card is selected, but the cipher uses every marked numbered space on that winning ticket or card, not only the spaces that make up the winning line, row, column, or diagonal. The number of marked spaces is `k`, and the selected plaintext must contain exactly `k` characters.

If multiple tickets or cards win in the same simulated game state, one winner is selected for challenge generation according to a deterministic rule that will be specified before implementation. Only the selected winner contributes to the challenge.

Challenge simulations and randomized filler generation must be reproducible from an explicit seed. The challenge-generation process must select a plaintext whose length matches `k`; the UK library contains messages from 5 through 15 characters and the US library contains messages from 4 through 20 characters.

## Console pretty-print formats

`UKBingoTicket.ToString()` returns three newline-terminated rows. Each of the nine cells occupies four characters; numbered values are right justified and zero-valued blank spaces are four spaces. Marked values retain their negative sign.

`UKBingoStrip.ToString()` returns six ticket blocks in ticket order. Each block is the corresponding ticket representation, and consecutive blocks are separated by one blank line, for 23 newline-terminated lines in total.

`USBingoCard.ToString()` returns six newline-terminated lines. The first is the column header `   B   I   N   G   O`; the remaining five lines contain the card values in five four-character-wide, right-justified columns. The center free space is printed as `   0`.

## Cipher challenge JSON

Milestone 3 initially writes complete challenge objects. Both `plaintext` and `ciphertext` are present in every object; a later milestone may produce filtered competitor-facing files that omit the answer field.

```json
{
  "schemaVersion": 1,
  "challengeType": "UK",
  "mode": "encode",
  "challengeId": "example-id",
  "seed": 42,
  "plaintext": "...",
  "ciphertext": "...",
  "ciphertextEncoding": "Hex",
  "calledNumbers": [1, 2, 3],
  "winningTicket": []
}
```

`challengeType` is `UK` or `US`; `mode` is `encode` or `decode`; and `winningTicket` contains either the selected 27-element UK ticket or selected 25-element US card. The US center free space remains `0` in that array.

`ciphertextEncoding` is `Hex` or `Base64` for UK challenges and `Alphabetic` for US challenges. It makes the UK `--base64` representation explicit in the complete JSON object.

UK ciphertext is hexadecimal by default. The UK `--base64` switch changes only the ciphertext representation to Base64. It is invalid for US challenge generation.

JSON is indented multi-line JSON in both the output file and, when requested, terminal output. The `--terminal` switch writes the same formatted JSON to standard output in addition to the file.

## Plaintext libraries

UK and US use separate embedded resources with curated nerd-culture words or short original messages in exact-length buckets from 4 through 20 characters.

- UK candidates may contain any ASCII printable characters, codes 32 through 126.
- US candidates contain alphabetic characters only, `[A-Za-z]`.
- Each bucket contains one through nineteen candidates; smaller curated buckets are acceptable when they are more memorable than padded alternatives.
- A candidate is selected uniformly from the bucket matching the selected winner's marked-space count.
- The selected candidate is validated again before challenge construction.
