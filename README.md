# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.


# Blocks

A blockchain data structure implementation in C# w/ data persistence.

```csharp
var FILE = "Genesis";

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

```csharp
Map(FILE, (i) => 
{
    Print(i);
});
```

```
Hash: 68219427362a787082d283cfeaac6b76f46286f99a647b3550926b8aa83d8a54
Previous: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5
No: 0
Timestamp: 9/17/2017 1:52:09 AM
Nonce: 1744414568
Verified: True
Genesis: True

Hash: 261902ae45c85e0891692651fe8d5e679cd15ddbab0a435903e8ee43ee97f386
Previous: 68219427362a787082d283cfeaac6b76f46286f99a647b3550926b8aa83d8a54
Data: d0283dd36794ee3f
No: 1
Timestamp: 9/17/2017 1:52:09 AM
Nonce: 1120141121
Verified: True

Hash: 845169a255484bebb6ff70506f2a5fd693b9afd8821ce5a15f66b3d08e340206
Previous: 261902ae45c85e0891692651fe8d5e679cd15ddbab0a435903e8ee43ee97f386
Data: 9fafa3a5cfd7e13f
No: 2
Timestamp: 9/17/2017 1:52:09 AM
Nonce: 2029385099
Verified: True

Hash: 404fe835fef348c9dc7327d7ebea03a6808627befbc730bc9345b9c5944a0292
Previous: 845169a255484bebb6ff70506f2a5fd693b9afd8821ce5a15f66b3d08e340206
Data: fb7afc627d3dee3f
No: 3
Timestamp: 9/17/2017 1:56:16 AM
Nonce: 115790775
Verified: True
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)

[Non-Blocking and Blocking Concurrent Algorithms](http://www.research.ibm.com/people/m/michael/podc-1996.pdf)

Inpsired by - [A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
