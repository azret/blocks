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
Nonce: 1744414568
Verified: True

Hash: 747f7ba27840597777b7b717291169f31174668b6f280dfec1d1075008c54712
Previous: 0f483271df649415eb6eb167a35c4d3869d4f125e90d9832e8004054c0fe68c7
Data: 9ff623714ffbd13f
No: 10
Timestamp: 9/17/2017 3:16:36 AM
Nonce: 1120141121
Verified: True

Hash: 5ed83cf77598273447887e6fb2e03a7bc5fff2bf17edaf4295587b58c69592c0
Previous: 747f7ba27840597777b7b717291169f31174668b6f280dfec1d1075008c54712
Data: fb7afc627d3dee3f
No: 11
Timestamp: 9/17/2017 3:22:03 AM
Nonce: 989897734
Verified: True

Hash: 6e70c1dfdd7c70c5d692cc2d550e03e633ea58a296e710f965fb022806efc553
Previous: 5ed83cf77598273447887e6fb2e03a7bc5fff2bf17edaf4295587b58c69592c0
Data: 9ff623714ffbd13f
No: 12
Timestamp: 9/24/2017 12:12:46 AM
Nonce: 115790775
Verified: True

Hash: c3ed0e5c295fc1fea57ebc839c0b5142d8f6cdb71b2ba23474c9452ac6ba7350
Previous: 6e70c1dfdd7c70c5d692cc2d550e03e633ea58a296e710f965fb022806efc553
Data: 9ff623714ffbd13f
No: 13
Timestamp: 9/24/2017 12:24:08 AM
Nonce: 857614893
Verified: True

Hash: bd63c7ee8eb2bf8beb97a41fd830a8531b65a7cee77396c095977d660c6b0289
Previous: c3ed0e5c295fc1fea57ebc839c0b5142d8f6cdb71b2ba23474c9452ac6ba7350
Data: 006261d0ffb0e03f
No: 14
Timestamp: 9/24/2017 1:49:16 AM
Nonce: 1744414568
Verified: True

Hash: ad930c1fda63344d05010bf2fd8c6188130301a606284f660b6aaea5833b2d14
Previous: bd63c7ee8eb2bf8beb97a41fd830a8531b65a7cee77396c095977d660c6b0289
Data: a4003b035280dd3f
No: 15
Timestamp: 10/2/2017 9:57:51 AM
Nonce: 888299495
Verified: True

Hash: 3ff882ff75752f40291dca2225232b8a5eac3ec9f3ad26eb1486c9915c087742
Previous: ad930c1fda63344d05010bf2fd8c6188130301a606284f660b6aaea5833b2d14
Data: d0283dd36794ee3f
No: 16
Timestamp: 10/19/2017 7:11:46 PM
Nonce: 857614893
Verified: True
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)

[Non-Blocking and Blocking Concurrent Algorithms](http://www.research.ibm.com/people/m/michael/podc-1996.pdf)

Inpsired by - [A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
