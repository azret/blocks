# Blocks - a blockchain implementation in C#

An blockchain implementation inspired by [naivechain](https://github.com/lhartikk/naivechain) with atomic block writes.


```csharp
Block LatestBlock;

if (!TryGetLatestBlock(FILE, &LatestBlock))
{
    var GenesisBlock = CreateBlock(0, Genesis, Nonce(), null);

	if (TryAppendBlock(FILE, &GenesisBlock) <= 0)
	{
		// Try again ...
	}
}

var NewBlock = CreateBlock(&LatestBlock, Nonce(), Data());

if (TryAppendBlock(FILE, &NewBlock) <= 0)
{
    // Try again ...
}
```

## Links

[A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
