# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.


# Blocks

A blockchain data structure implementation in C# w/ data persistence.

```csharp
Block LatestBlock;

if (GetLatestBlock(FILE, &LatestBlock))
{
	// Compare and append semantics

	var NewBlock = CreateBlock(&LatestBlock, Nonce(), Data());

	if (AppendBlock(FILE, &NewBlock) <= 0)
	{
		// Try again
	}
}
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)

[Non-Blocking and Blocking Concurrent Algorithms](http://www.research.ibm.com/people/m/michael/podc-1996.pdf)

Inpsired by - [A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
