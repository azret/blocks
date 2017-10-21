# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.


# Blocks

A sample blockchain data structure implementation in C# w/ data persistence.

```
blocks --port 8000
```

```
curl http://localhost:8000/blocks
```

```
Hash: 27412103ae0823aa9fc40cde0445fc48e358be206ca8a28b5915aeb5012159f5
Previous: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5
No: 0
Timestamp: 10/20/2017 7:54:39 PM
Nonce: 1744414568
Verified: True
Genesis: True
```

## Links

[The ABA Problem](https://en.wikipedia.org/wiki/ABA_problem)

[Non-Blocking and Blocking Concurrent Algorithms](http://www.research.ibm.com/people/m/michael/podc-1996.pdf)

Inpsired by - [A blockchain in 200 lines of code](https://medium.com/@lhartikk/a-blockchain-in-200-lines-of-code-963cc1cc0e54)
