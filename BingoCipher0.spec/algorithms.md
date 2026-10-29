# Algorithms

## UK Bingo encipher procedure

Inputs: a sequence of called numbers, a ticket (UK) that wins with that sequence, and a plaintext message whose length equals the number of marked numbered spaces on the ticket after the winning state is reached. All marked numbered spaces on the ticket are used, not only the five spaces in a winning line. The single-letter labels used in this document are mathematical notation; implementation names should be descriptive.

1. In a 3-row by 9-column grid, place the characters of the message at the same coordinates as the marked numbers on the winning ticket `t` in the order `s` that they were marked.
2. If a deterministic ciphertext flag is desired, place an underscore `_` character at the remaining coordinates; otherwise place seeded-random ASCII printable characters (codes 32 through 126) at the remaining coordinates.
3. Concatenate the three rows of nine characters into a single 27-character string.
4. Encode the 27-character string as ASCII, then encrypt those bytes using the specified ChaCha20 parameters: 256-bit key hex `23f99a75efdafdd8ac728bdeaf2feda1d6b2fc8fdfa2e9ddfdafdca26a6eb5ea`, 96-bit nonce hex `b5e95e7289a6ba789c6ada2b`, and initial counter `1`.

Return value: The ChaCha20 encrypted message as a hex string `c`.

## UK Bingo decipher procedure

Inputs: a sequence of called numbers, a winning UK ticket, and a ciphertext message represented as a hex string.

1. Decrypt `c` using the same ChaCha20 parameters and decode the resulting bytes as ASCII.
2. Place the 27 decrypted characters into a 3-row by 9-column grid: the first nine characters in the top row, middle nine characters in the middle row, and last 9 characters in the bottom row.
3. One at a time, locate every numbered space marked on the winning ticket `t`, in the order `s` that the corresponding numbers were called, and build a sequence of length `k` of the decrypted characters at the corresponding coordinates.

Return value: The original plaintext of length `k`.

## US Bingo encipher procedure

Inputs: a sequence of called numbers, a winning US card, and an alphabetic plaintext message whose length equals the number of marked numbered spaces on the card after the winning state is reached. All marked numbered spaces on the card are used, not only the spaces in the winning row, column, or diagonal.

1. Encrypt the plaintext using ROT13.
2. In a 5-row by 5-column grid, place the characters of the message at the same coordinates as the marked numbers on the winning card `t` in the order `s` that they were marked.
3. If a deterministic ciphertext flag is desired, place a `B` character at each of the remaining coordinates, including the "free space" center coordinate; otherwise place seeded-random alphabetic characters from `[A-Za-z]` at the remaining coordinates.
4. Concatenate the five rows of five characters into a single 25-character string `c`.

Return value: The 25-character alphabetic string `c`.

## US Bingo decipher procedure

Inputs: a sequence of called numbers, a winning US card, and an alphabetic 25-letter ciphertext message.

1. Place the 25 ciphertext characters into a 5-by-5 grid, with the first five characters in the top row, the next five in the next row, and so forth to the last five characters in the bottom row.
2. One at a time, locate every numbered space marked on the winning card `t`, in the order `s` that the corresponding numbers were called, and build a sequence of length `k` of the ciphertext characters at the corresponding coordinates. The `0` free space is never extracted.
3. Use ROT13 to decrypt the extracted sequence of length `k`.

Return value: The original plaintext of length `k`.
