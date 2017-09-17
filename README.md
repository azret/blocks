# Blocks - a blockchain database in C#

A blockchain implementation inspired by [naivechain](https://github.com/lhartikk/naivechain) with **data persistence**.

# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.

```csharp
Block LatestBlock;

// Get the latest known block from disk ...

if (!TryGetLatestBlock(FILE, &LatestBlock))
{
    // Start a new blockchain with a genesis block ...

    var GenesisBlock = CreateBlock(0, Genesis, Nonce(), null);

    if (TryAppendBlock(FILE, &GenesisBlock) <= 0)
    {
        // Try again ...
    }
}

// Atomic compare and append semantics ...

var NewBlock = CreateBlock(&LatestBlock, Nonce(), Data());

if (TryAppendBlock(FILE, &NewBlock) <= 0)
{
    // Try again ...
}
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)
[A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
