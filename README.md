# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.


# Blocks

A sample blockchain data structure implementation in C# w/ data persistence.


```csharp
var FILE = "Genesis";

Block LatestBlock;

if (GetLatestBlock(FILE, &LatestBlock))
{
    // compare and append semantics

    var NewBlock = CreateBlock(&LatestBlock, Nonce(), Data());

    if (AppendBlock(FILE, &NewBlock) <= 0)
    {
        // reject
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
Hash: cc3cdc73e13d28addd1645212577049bc2d8b41dd8b298b58f86e30b40b74bd6
Previous: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5
No: 0
Timestamp: 10/19/2017 7:14:54 PM
Nonce: 1744414568
Verified: True
Genesis: True

Hash: ee30d591012e0fbafe53f4866aa63a5acbb868bc851fa1a18236f96901e713ad
Previous: cc3cdc73e13d28addd1645212577049bc2d8b41dd8b298b58f86e30b40b74bd6
Data: 006261d0ffb0e03f
No: 1
Timestamp: 10/19/2017 7:14:54 PM
Nonce: 857614893
Verified: True

Hash: f9021e543a4a489e923a804a10ba8eb19374a3432295ce1dc8be148d814e233a
Previous: ee30d591012e0fbafe53f4866aa63a5acbb868bc851fa1a18236f96901e713ad
Data: 927c2f3f49bed73f
No: 2
Timestamp: 10/19/2017 7:14:54 PM
Nonce: 1197424278
Verified: True
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)

[Non-Blocking and Blocking Concurrent Algorithms](http://www.research.ibm.com/people/m/michael/podc-1996.pdf)

Inpsired by - [A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
