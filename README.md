# BingoCipher0

BingoCipher0 is a .NET 10 suite for generating reproducible UK and US Bingo games and Bingo-based CTF cipher challenges.

## Build and test

```console
dotnet clean
dotnet build
dotnet test
```

## Run

Run the UK and US game simulators with an explicit seed and player count:

```console
dotnet run --project src/UKBingoSim -- --seed 42 --players 21
dotnet run --project src/USBingoSim -- --seed 1776 --players 21
```

Add `--terminal` to either simulator command for its human-readable output instead of JSON.

Generate UK cipher challenges:

```console
dotnet run --project src/UKBingoCipherChallenge -- --seed 0xdeadbeef8badf00d --id uk-example --encode
dotnet run --project src/UKBingoCipherChallenge -- --seed 0x00babe0000babe00 --id uk-example --decode
```

Generate US cipher challenges:

```console
dotnet run --project src/USBingoCipherChallenge -- --seed 1776 --id us-example --encode
dotnet run --project src/USBingoCipherChallenge -- --seed 1776 --id us-example --decode
```

Challenge generators default to 21 players and the current output directory. They also accept `--players`, `--output-directory`, and `--terminal`. UK challenges additionally accept `--base64`; hexadecimal is the default UK ciphertext representation.


The standalone `--` separates options interpreted by `dotnet run` from arguments passed to the selected executable.

---

# SAINTCON26 Hackers Challenge

Bingo Ciphers were used in two challenges.

## Bingo Cipher 1

Here's the text of the first SAINTCON26 Bingo Cipher challenge:

> Agents Alida and Robbe operate in the United States. To exchange short secret messages, they use their local Bingo hall to create one-time shared secrets. They establish their shared information as follows:
> 
> 1. During a Bingo game, they record the sequence in which the numbers are called.
> 2. When a player calls "Bingo!" and wins, they obtain a copy of the winning Bingo card.
> 
> To send a secret message, an agent follows these steps:
> 
> 1. Write a plaintext message containing exactly as many letters as there are daubed squares on the winning card, except the center free space.
> 2. Encode the plaintext using ROT13.
> 3. Place the encoded letters in the daubed squares of the winning card, in the order their corresponding numbers were called.
> 4. Fill all remaining squares, including the center free space, with random letters.
> 5. Read the card row by row, from left to right and top to bottom, to produce a 25-character ciphertext.
> 6. Transmit the ciphertext to the receiving agent.
> 
> The receiving agent deciphers the message by reversing this procedure.
> 
> Your Challenge:
> 
> Robbe sends Alida the following ciphertext:
> 
> PEAqrNgZTQteECARZozqHZqrb
> 
> What plaintext message does Alida recover?
> 
> The sequence of called numbers and the winning Bingo card are provided below.
> 
> Called numbers (in order):
> 
> B-14,O-65,O-67,O-61,G-52,I-29,O-68,G-46,B-15,O-62,B-1,N-35,I-25,G-49,N-44,N-43,G-53,I-21,B-2,I-26,G-50,I-17,I-16
> 
> Winning Bingo card:
>

At SAINTCON26 Hackers Challenge, 45 competitors submitted a correct flag.

## Bingo Cipher 2

Here's the text of the second SAINTCON26 Bingo Cipher challenge:

> Agents Alida and Robbe have been relocated to London, where Bingo is played differently. They still use a local Bingo hall to establish their shared information:
> 
> 1. During a Bingo game, they record the sequence in which the numbers are called.
> 2. When a player wins a line, they obtain a copy of that player's winning Bingo ticket.
> 
> To send a secret message, an agent follows these steps:
> 
> 1. Write a plaintext message containing exactly as many characters as there are daubed squares on the winning ticket.
> 2. In a 3-row, 9-column grid, place the characters of the message in the daubed squares of the winning ticket, in the order their corresponding numbers were called.
> 3. Fill all remaining squares with random ASCII characters.
> 4. Read the grid row by row, from left to right and top to bottom, to produce a 27-character ASCII string.
> 5. Encrypt the 27-character string using IETF ChaCha20 with these parameters:
>   - 256-bit key: (hex) 23f99a75efdafdd8ac728bdeaf2feda1d6b2fc8fdfa2e9ddfdafdca26a6eb5ea, (base64) I/made/a/discovery/today/I/found/a/computer
>   - 96-bit nonce: (hex) b5e95e7289a6ba789c6ada2b, (base64) telecommunicator
>   - Initial block counter: 1
> 6. Transmit the ciphertext as a hexadecimal string.
> 
> The receiving agent deciphers the message by reversing this procedure.
> 
> Your Challenge: Alida sends Robbe the following ciphertext (hex):
> 
> 47af4e75f50a520f205abfbe923dc468ad27131b8f17cd08e0a032
> 
> What plaintext message does Robbe recover?
> 
> The sequence of called numbers and the winning Bingo ticket are provided below.
> 
> Called numbers (in order):
> 
> 68,61,83,85,58,79,76,41,4,51,47,12,88,54,57,13,2,19,32,81,31,18,11,21,22
> 
> Winning Bingo ticket:

At SAINTCON26 Hackers Challenge, 42 competitors submitted a correct flag.
