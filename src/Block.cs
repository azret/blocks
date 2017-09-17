using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public static unsafe class Blocks
{
    /// <summary>
    /// Genesis
    /// </summary>
    public static byte[] Genesis = Sign(System.Text.Encoding.ASCII.GetBytes("Genesis"));

    /// <summary>
    /// Block
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1, Size = 1024)]
    public unsafe struct Block
    {
        public const int MAX = 1024 - 32 - 32 - 4 - 4 - 4 - 8;
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
        /// data
        /// </summary>
        public fixed byte data[MAX];
        /// <summary>
        /// GetHash()
        /// </summary>
        public byte[] GetHash()
        {
            byte[] tmp = new byte[32];
            fixed (byte* p = hash)
            {
                for (var i = 0; i < 32; i++)
                {
                    tmp[i] = p[i];
                }
            }
            return tmp;
        }
        /// <summary>
        /// GetData()
        /// </summary>
        public byte[] GetData()
        {
            var size = len;
            if (size > MAX)
            {
                size = MAX;
            }
            byte[] tmp = new byte[size];
            fixed (byte* p = data)
            {
                for (var i = 0; i < size; i++)
                {
                    tmp[i] = p[i];
                }
            }
            return tmp;
        }
    }

    public static unsafe void Copy(byte* dst, byte* src, int count)
    {
        for (int i = 0; i < count; i++)
        {
            dst[i] = src[i];
        }
    }

    public static unsafe string Hex(byte[] value, int len = -1)
    {
        if (len < 0)
        {
            len = value.Length;
        }
        var hex = new System.Text.StringBuilder();
        for (int i = 0; i < len; i++)
        {
            hex.Append(value[i].ToString("x2"));
        }
        return hex.ToString();
    }

    public static unsafe byte[] GetPreviousHash(Block* block)
    {
        byte[] tmp = new byte[32];
        byte* p = block->previous;
        {
            for (var i = 0; i < 32; i++)
            {
                tmp[i] = p[i];
            }
        }
        return tmp;
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

    public static unsafe byte[] CreateBlock(Block* src, bool unhash)
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

    public static unsafe Block CreateBlock(byte[] block)
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

    public static unsafe Block CreateBlock(int no, byte[] previous, int nonce, byte[] data)
    {
        if (previous.Length != 32)
        {
            throw new System.ArgumentOutOfRangeException("previous", "Invalid hash key.");
        }

        System.TimeSpan t = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);

        Block block = new Block()
        {
            no = no,
            nonce = nonce
        };

        fixed (byte* p = previous)
        {
            Copy(block.previous, p, 32);
        }

        if (data != null)
        {
            block.len = data.Length;

            if (block.len > Block.MAX)
            {
                throw new System.ArgumentOutOfRangeException("data", "Invalid data size.");
            }

            fixed (byte* p = data)
            {
                Copy(block.data, p, block.len);
            }
        }

        block.timestamp = (int)t.TotalSeconds;

        byte[] hash = Sign(CreateBlock(&block, false));

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

    public static unsafe bool Compare(byte[] a, byte* b)
    {
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }

    public static unsafe bool IsValidBlock(Block* src, byte* previous)
    {
        if (src == null)
        {
            return false;
        }

        byte[] hash = src->GetHash();

        if (!Compare(Sign(CreateBlock(src, true)), hash))
        {
            return false;
        }

        if (previous != null)
        {
            if (!Compare(GetPreviousHash(src), previous))
            {
                return false;
            }
        }

        return true;
    }

    public static unsafe bool IsGenesis(Block* src)
    {
        if (Compare(GetPreviousHash(src), Genesis))
        {
            return true;
        }

        return false;
    }

    public static unsafe Block CreateBlock(Block* previous, int nonce, byte[] data)
    {
        return CreateBlock(previous->no + 1, previous->GetHash(), nonce, data);
    }
    
    static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential)]
    class SECURITY_ATTRIBUTES
    {
        internal int nLength;
        internal unsafe byte* pSecurityDescriptor;
        internal int bInheritHandle;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("Kernel32.dll", SetLastError = false)]
    static extern int GetLastError();

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("Kernel32.dll", SetLastError = false)]
    static extern void SetLastError(int lastError);

    [DllImport("Kernel32.dll", BestFitMapping = false, SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateFile(
        String lpFileName,
        UInt32 dwDesiredAccess,
        UInt32 dwShareMode,
        SECURITY_ATTRIBUTES lpSecurityAttributes,
        UInt32 dwCreationDisposition,
        UInt32 dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("Kernel32.dll", SetLastError = true)]
    unsafe static extern bool WriteFile(
        IntPtr hFile,
        IntPtr lpBuffer,
        int nNumberOfBytesToWrite,
        out int lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = false)]
    static extern bool CloseHandle(
         IntPtr hObject
    );

    public enum SEEK : uint
    {
        FROM_START = 0,
        FROM_CURRENT = 1,
        FROM_END = 2
    }

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "SetFilePointer")]
    unsafe static extern int SetFilePointerWin32(IntPtr h_File, int lo, int* hi, uint origin);

    internal unsafe static long SetFilePointer(IntPtr h_File, long offset, SEEK seek, out int hr)
    {
        hr = 0;

        int lo = (int)offset;
        int hi = (int)(offset >> 32);

        lo = SetFilePointerWin32(h_File, lo, &hi, (uint)seek);

        if (lo == -1 && ((hr = Marshal.GetLastWin32Error()) != 0))
            return -1;

        return (long)(((ulong)((uint)hi)) << 32) | ((uint)lo);
    }

    [DllImport("Kernel32.dll", SetLastError = true)]
    unsafe static extern bool ReadFile(
            IntPtr hFile,
            byte* lpBuffer,
            int nNumberOfBytesToRead,
            out int lpNumberOfBytesRead,
            void* overlapped);

    public unsafe static int AppendBlock(string file, Block* src, int retry = int.MaxValue)
    {
        System.IntPtr hFile = System.IntPtr.Zero;

        if (src == null)
        {
            throw new System.ArgumentNullException("src");
        }

        if (string.IsNullOrEmpty(file))
        {
            throw new System.ArgumentNullException("file", "File name is not specified.");
        }

        int @try = 0;

        try
        {
            int error;

            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;

            /* Obtain an exclusive write lock on a file */

            while (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
            {
                @try++;

                hFile = CreateFile(
                      file,
                      GENERIC_READ | GENERIC_WRITE,
                      0x00000001 /* FILE_SHARE_READ */,
                      null,
                      0x04 /* OPEN_ALWAYS */,
                      0x00000080 /* FILE_ATTRIBUTE_NORMAL */,
                      IntPtr.Zero);

                if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
                {
                    error = GetLastError();

                    if (error == 32 && @try < retry)
                    {
                        Thread.Sleep(0);
                        continue;
                    }

                    throw new Win32Exception();
                }
            }

            long size = SetFilePointer(hFile, 0, SEEK.FROM_END, out error);

            if (error != 0)
            {
                throw new Win32Exception(error);
            }

            long count = size / 1024;

            if (src->no != count)
            {
                return -1;
            }

            long end = SetFilePointer(hFile, -1024, SEEK.FROM_END, out error);

            if (error != 0 && 131 != error)
            {
                throw new Win32Exception(error);
            }

            Block comparand; int read = 0;

            bool ok = ReadFile(hFile, (byte*)(&comparand), 1024, out read, null);

            if (!ok)
            {
                throw new Win32Exception(GetLastError());
            }

            if (read == 0)
            {
                if (!IsValidBlock(src, null) || !IsGenesis(src))
                {
                    throw new ArgumentException("Invalid genesis block.");
                }
            }
            else if (read != 1024)
            {
                throw new ArgumentException("Invalid file.");
            }
            else
            {
                if (!IsValidBlock(src, comparand.hash))
                {
                    return -2;
                }                
            }

            int bytesWritten = 0;

            if (!WriteFile(hFile, new IntPtr(src), 1024, out bytesWritten, IntPtr.Zero))
            {
                throw new Win32Exception(GetLastError());
            }

            Debug.Assert(@try > 0);

            return @try;

        }
        finally
        {
            if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
            {
                CloseHandle(hFile);

                hFile = IntPtr.Zero;
            }
        }
    }

    public unsafe static bool GetLatestBlock(string file, Block* dst)
    {
        System.IntPtr hFile = System.IntPtr.Zero;

        if (dst == null)
        {
            throw new System.ArgumentNullException("dst");
        }

        if (string.IsNullOrEmpty(file))
        {
            throw new System.ArgumentNullException("file", "File name is not specified.");
        }

        try
        {
            if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
            {
                const uint GENERIC_READ = 0x80000000;
                const uint GENERIC_WRITE = 0x40000000;

                hFile = CreateFile(
                          file,
                          GENERIC_READ,
                          0x00000001 | 0x00000002 /* FILE_SHARE_READ | FILE_SHARE_WRITE */,
                          null,
                          0x04 /* OPEN_ALWAYS */,
                          0x00000080 /* FILE_ATTRIBUTE_NORMAL */,
                          IntPtr.Zero);

                if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
                {
                    throw new Win32Exception(GetLastError());
                }
            }

            int error;

            long size = SetFilePointer(hFile, 0, SEEK.FROM_END, out error);

            if (error != 0)
            {
                throw new Win32Exception(error);
            }

            long count = size / 1024;
  
            long end = SetFilePointer(hFile, -1024, SEEK.FROM_END, out error);

            if (error != 0 && 131 != error)
            {
                throw new Win32Exception(error);
            }

            Block previous; int read = 0;

            bool ok = ReadFile(hFile, (byte*)(&previous), 1024, out read, null);

            if (!ok)
            {
                throw new Win32Exception(GetLastError());
            }

            if (read == 0)
            {
                return false;
            }
            else if (read != 1024)
            {
                throw new ArgumentException("Invalid file.");
            }
            else
            {
                *dst = previous;
                return true;
            }
        }
        finally
        {
            if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
            {
                CloseHandle(hFile);

                hFile = IntPtr.Zero;
            }
        }
    }

    public unsafe static bool Map(string file, Action<IntPtr> body)
    {
        System.IntPtr hFile = System.IntPtr.Zero;

        if (string.IsNullOrEmpty(file))
        {
            throw new System.ArgumentNullException("file", "File name is not specified.");
        }

        try
        {
            if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
            {
                const uint GENERIC_READ = 0x80000000;

                hFile = CreateFile(
                          file,
                          GENERIC_READ,
                          0x00000001 | 0x00000002 /* FILE_SHARE_READ | FILE_SHARE_WRITE */,
                          null,
                          0x04 /* OPEN_ALWAYS */,
                          0x00000080 /* FILE_ATTRIBUTE_NORMAL */,
                          IntPtr.Zero);

                if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
                {
                    throw new Win32Exception(GetLastError());
                }
            }
            
L0:
            Block previous; int read = 0;

            bool ok = ReadFile(hFile, (byte*)(&previous), 1024, out read, null);

            if (!ok)
            {
                throw new Win32Exception(GetLastError());
            }

            if (read == 0)
            {
                return false;
            }
            else if (read != 1024)
            {
                throw new ArgumentException("Invalid file.");
            }
            else
            {
                if (body != null)
                {
                    body(new IntPtr(&previous));
                }

                goto L0;
            }

        }
        finally
        {
            if (hFile != IntPtr.Zero && hFile != INVALID_HANDLE_VALUE)
            {
                CloseHandle(hFile);

                hFile = IntPtr.Zero;
            }
        }
    }

    public static unsafe void Print(Block* block, TextWriter writer)
    {
        writer.WriteLine($"Hash: {Hex(block->GetHash())}");

        writer.WriteLine($"Previous: {Hex(GetPreviousHash(block))}");

        if (block->len > 0)
        {
            writer.WriteLine($"Data: {Hex(block->GetData())}");
        }

        int no = block->no;

        writer.WriteLine($"No: {no}");

        var timestamp = block->timestamp;

        writer.WriteLine($"Timestamp: {(new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(timestamp).ToLocalTime()}");

        var nonce = block->nonce;

        writer.WriteLine($"Nonce: {nonce}");

        writer.WriteLine($"Verified: {IsValidBlock(block, null)}");

        writer.WriteLine();
    }

    public static System.Random Seed = new System.Random(7919);

    static byte[] Data()
    {
        return System.BitConverter.GetBytes(Seed.NextDouble());
    }

    static int Nonce()
    {
        return Seed.Next();
    }

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
                var NewBlock = CreateBlock(LatestBlock.no + 1, LatestBlock.GetHash(), Nonce(), Data());

                if (AppendBlock(FILE, &NewBlock) <= 0)
                {
                    // Try again ...
                }
            }            
        });

        // Check data consistency ...

        byte[] tmp = null;

        Map(FILE, (i) => 
        {
            Block* b = (Block*)i;

            fixed (byte * p = tmp)
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