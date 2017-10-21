# What is a blockchain?

[From Wikipedia](https://en.wikipedia.org/wiki/Blockchain) : A blockchain is a distributed database that maintains a continuously-growing list of records called blocks secured from tampering and revision.


# Blocks

A sample blockchain data structure implementation in C# w/ data persistence.

## Starting a new node

```
blocks --port 8000
```

```
Validating blockchain: Genesis

Hash: 27412103ae0823aa9fc40cde0445fc48e358be206ca8a28b5915aeb5012159f5
Previous: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5
No: 0
Timestamp: 10/20/2017 7:54:39 PM
Nonce: 1744414568
Verified: True
Genesis: True

Ready.

http://localhost:8000

Press any key to quit...
```

## Get the latest block

```
curl -X GET http://localhost:8000/
```

```
{
	hash: "27412103ae0823aa9fc40cde0445fc48e358be206ca8a28b5915aeb5012159f5",
	previous: "81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5",
	no: 0,
	timestamp: "10/20/2017 7:54:39 PM",
	nonce: 1744414568,
}
```
