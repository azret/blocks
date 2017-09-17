# Blocks - a blockchain implementation in C#

An blockchain implementation inspired by [naivechain](https://github.com/lhartikk/naivechain) with **atomic** block writes and **data persistence**.

# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.

```csharp
Block LatestBlock;

// Get the latest known block *on disk* ...

if (!TryGetLatestBlock(FILE, &LatestBlock))
{
    // Start a new blockchain with a genesis block ...

    var GenesisBlock = CreateBlock(0, Genesis, Nonce(), null);

    if (TryAppendBlock(FILE, &GenesisBlock) <= 0)
    {
        // Try again ...
    }
}

// Atomic compare* & append ...

var NewBlock = CreateBlock(&LatestBlock, Nonce(), Data());

if (TryAppendBlock(FILE, &NewBlock) <= 0)
{
    // Try again ...
}
```

## Links

[A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
