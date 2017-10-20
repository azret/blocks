using System;
using System.Diagnostics;
using System.Threading.Tasks;

using static Blocks;

class App
{
    public static unsafe void Main(string[] args)
    {
        string FILE = "Genesis";

        Block GenesisBlock;

        // Create a new file if needed ...

        if (!GetLatestBlock(FILE, &GenesisBlock))
        {
            GenesisBlock = CreateBlock(0, Genesis, Seed.Next(), null);

            Debug.Assert(IsGenesis(&GenesisBlock));

            if (AppendBlock(FILE, &GenesisBlock) <= 0)
            {
                Console.Error?.WriteLine("Could not create genesis block.");
            }
        }

        // Generate new blocks ...

        Parallel.For(0, 7, (i) =>
        {
            Block LatestBlock;

            if (GetLatestBlock(FILE, &LatestBlock))
            {
                var NewBlock = CreateBlock(LatestBlock.no + 1, LatestBlock.GetHash(), Blocks.Nonce(), Data());

                if (AppendBlock(FILE, &NewBlock) <= 0)
                {
                    // Block is rejected. Try again.
                }
            }
        });

        // Check data consistency ...

        byte[] tmp = null;

        Map(FILE, (i) =>
        {
            Block* b = (Block*)i;

            fixed (byte* p = tmp)
            {
                if (IsValidBlock(b, p))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            }

            Print(b, System.Console.Out);

            Console.ResetColor();

            tmp = b->GetHash();
        });

        System.Console.ReadKey();
    }
}
