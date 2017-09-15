public unsafe class Blocks
{
    public static unsafe void Copy(byte* dst, byte* src, int count)
    {
        for (int i = 0; i < count; i++)
        {
            dst[i] = src[i];
        }
    }
    
    public static unsafe string Hex(byte[] value)
    {
        var hex = new System.Text.StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            hex.Append(value[i].ToString("x2"));
        }
        return hex.ToString();
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1, Size = 1024)]
    public unsafe struct Block
    {
        public const int MAX = 1024 - 32 - 32 - 4 - 4 - 4 - 8 - 8;
        /// <summary>
        /// previous
        /// </summary>
        public fixed byte previous[32]; // 32 bytes
        /// <summary>
        /// hash
        /// </summary>
        public fixed byte hash[32]; // 32 bytes
        /// <summary>
        /// len
        /// </summary>
        public int len; // 4 bytes
        /// <summary>
        /// nonce
        /// </summary>
        public int nonce; // 4 bytes
        /// <summary>
        /// no
        /// </summary>
        public int no; // 4 bytes 
        /// <summary>
        /// timestamp
        /// </summary>
        public double timestamp; // 8 bytes
        /// <summary>
        /// reserved
        /// </summary>
        public long ptr; // 8 bytes
        /// <summary>
        /// secret
        /// </summary>
        public fixed byte secret[MAX];
        /// <summary>
        /// GetHash()
        /// </summary>
        public byte[] GetHash()
        {
            byte[] tmp = new byte[32];
            fixed (byte* p = hash)
            {
                for (var i = 0; i > 32; i++)
                {
                    tmp[i] = p[i];
                }
            }
            return tmp;
        }
    }

    public static unsafe byte[] Sign(byte[] value)
    {
        System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
        try
        {
            return sha256.ComputeHash(value);
        }
        finally
        {
            sha256.Dispose();
        }
    }

    public static unsafe byte[] Create(Block* src, bool unhash)
    {
        byte[] buf = new byte[1024];
        Block tmp = *src;
        if (unhash)
        {
            byte* p = tmp.hash;
            {
                for (int i = 0; i < 32; i++)
                {
                    p[i] = 0;
                }
            }
        }
        tmp.ptr = 0;
        fixed (byte* p = buf)
        {
            byte* s = (byte*)&tmp;
            for (int i = 0; i < 1024; i++)
            {
                p[i] = s[i];
            }
        }
        return buf;
    }

    public static unsafe Block Create(byte[] block)
    {
        Block tmp = new Block();
        fixed (byte* p = block)
        {
            byte* s = (byte*)&tmp;
            for (int i = 0; i < 1024; i++)
            {
                p[i] = s[i];
            }
        }
        return tmp;
    }

    public static System.Random Seed = new System.Random(7919);

    public static unsafe Block Create(int no, byte[] previous, byte[] data)
    {
        if (previous.Length != 32)
        {
            throw new System.ArgumentOutOfRangeException("previous", "Invalid hash key.");
        }

        System.TimeSpan t = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);

        Block block = new Block()
        {
            no = no
        };

        block.nonce = Seed.Next();

        fixed (byte* p = previous)
        {
            Copy(block.previous, p, 32);
        }

        if (data != null)
        {
            block.len = data.Length;

            if (block.len > Block.MAX)
            {
                throw new System.ArgumentOutOfRangeException("secret", "Invalid secret size.");
            }

            fixed (byte* p = data)
            {
                Copy(block.secret, p, block.len);
            }
        }

        block.timestamp = (int)t.TotalSeconds;

        byte[] hash = Sign(Create(&block, false));

        fixed (byte* p = hash)
        {
            Copy(block.hash, p, 32);
        }

        return block;
    }

    public static bool Compare(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }

    /*
Genesis: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5

Hash: 00cb2fa661db0751ee4e9b564ab6fc63d69fd0f0121300f019043a63956c6fd3
Previous: 81ddc8d248b2dccdd3fdd5e84f0cad62b08f2d10b57f9a831c13451e5c5c80a5
Secret: Genesis
No: 0
Timestamp: 9/14/2017 12:32:54 AM
Nonce: 1744414568
    */

    public static unsafe bool Verify(Block* src)
    {
        if (src == null)
        {
            return false;
        }

        byte[] hash = new byte[32];

        fixed (byte* p = hash)
        {
            Copy(p, src->hash, 32);
        }

        if (!Compare(Sign(Create(src, true)), hash))
        {
            return false;
        }

        return true;
    }

    static Block* Nodes = null;

    public static unsafe Block* Local()
    {
        Block* nodes = Nodes;

        if (nodes == null)
        {
            lock (Seed)
            {
                nodes = Nodes;

                if (nodes == null)
                {
                    Block* ptr = (Block*)System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Block)));

                    *ptr = Create(0, Sign(System.Text.Encoding.ASCII.GetBytes("Genesis")), null);

                    nodes = ptr;
                }
            }
        }

        return nodes;
    }

    public static unsafe bool Take(Block* src)
    {
        lock (Seed)
        {
            Block* ptr;

            var nodes = Nodes;

            if (nodes == null)
            {
                ptr = (Block*)System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Block)));

                *ptr = Create(0, Sign(System.Text.Encoding.ASCII.GetBytes("Genesis")), null);

                nodes = ptr;
            }

            if (!Verify(src))
            {
                return false;
            }

            ptr = (Block*)System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(typeof(Block)));

            *ptr = *src;

            ptr->ptr = ((System.IntPtr)nodes).ToInt64();

            return true;
        }

    }

    public static unsafe void Main(string[] args)
    {
        var Last = *Local();

        byte[] hash = new byte[32];

        fixed (byte* p = hash)
        {
            Copy(p, Last.hash, 32);
        }

        int no = Last.no + 1;

        var block = Create(no, hash, System.Text.Encoding.ASCII.GetBytes(no.ToString()));

        fixed (byte* p = hash)
        {
            Copy(p, block.hash, 32);
        }

        System.Console.WriteLine($"Hash: {Hex(hash)}");

        byte[] previous = new byte[32];

        fixed (byte* p = previous)
        {
            Copy(p, block.previous, 32);
        }

        System.Console.WriteLine($"Previous: {Hex(previous)}");
        
        byte[] secret = new byte[1024];

        if (block.len > Block.MAX)
        {
            block.len = Block.MAX;
        }

        fixed (byte* p = secret)
        {
            Copy(p, block.secret, block.len);
        }

        if (block.len > 0)
        {
            System.Console.WriteLine($"Secret: {System.Text.Encoding.UTF8.GetString(secret, 0, block.len)}");
        }

        no = block.no;

        System.Console.WriteLine($"No: {no}");

        var timestamp = block.timestamp;

        System.Console.WriteLine($"Timestamp: {(new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(timestamp).ToLocalTime()}");

        var nonce = block.nonce;

        System.Console.WriteLine($"Nonce: {nonce}");

        System.Console.WriteLine($"Verified: {Verify(&block)}");

        System.Console.ReadKey();

    }
}