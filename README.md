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
