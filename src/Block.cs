using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

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
                for (var i = 0; i < 32; i++)
                {
                    tmp[i] = p[i];
                }
            }
            return tmp;
        }
        /// <summary>
        /// GetSecret()
        /// </summary>
        public byte[] GetSecret()
        {
            var size = len;
            if (size > MAX)
            {
                size = MAX;
            }
            byte[] tmp = new byte[size];
            fixed (byte* p = secret)
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

    public static unsafe byte[] GetPrevious(Block* block)
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

    public static unsafe Block Create(int no, byte[] previous, int nonce, byte[] data)
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

    public static unsafe bool Compare(byte[] a, byte* b)
    {
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }

    public static unsafe bool IsValid(Block* src, byte* previous)
    {
        if (src == null)
        {
            return false;
        }

        byte[] hash = src->GetHash();

        if (!Compare(Sign(Create(src, true)), hash))
        {
            return false;
        }

        if (previous != null)
        {
            if (!Compare(GetPrevious(src), previous))
            {
                return false;
            }
        }

        return true;
    }

    public static unsafe bool IsGenesis(Block* src)
    {
        if (Compare(GetPrevious(src), Genesis))
        {
            return true;
        }

        return false;
    }

    public static unsafe Block Create(Block* previous, int nonce, byte[] data)
    {
        return Create(previous->no + 1, previous->GetHash(), nonce, data);
    }
    
    public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES
    {
        internal int nLength;
        internal unsafe byte* pSecurityDescriptor;
        internal int bInheritHandle;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("Kernel32.dll", SetLastError = false)]
    public static extern int GetLastError();

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport("Kernel32.dll", SetLastError = false)]
    public static extern void SetLastError(int lastError);

    [DllImport("Kernel32.dll", BestFitMapping = false, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateFile(
        String lpFileName,
        UInt32 dwDesiredAccess,
        UInt32 dwShareMode,
        SECURITY_ATTRIBUTES lpSecurityAttributes,
        UInt32 dwCreationDisposition,
        UInt32 dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("Kernel32.dll", SetLastError = true)]
    internal unsafe static extern bool WriteFile(
        IntPtr hFile,
        IntPtr lpBuffer,
        int nNumberOfBytesToWrite,
        out int lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = false)]
    public static extern bool CloseHandle(
         IntPtr hObject
    );

    public enum SEEK : uint
    {
        FROM_START = 0,
        FROM_CURRENT = 1,
        FROM_END = 2
    }

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "SetFilePointer")]
    private unsafe static extern int SetFilePointerWin32(IntPtr h_File, int lo, int* hi, uint origin);

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
    internal unsafe static extern bool ReadFile(
            IntPtr hFile,
            byte* lpBuffer,
            int nNumberOfBytesToRead,
            out int lpNumberOfBytesRead,
            void* overlapped);

    public unsafe static void Append(string file, Block* src)
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

        try
        {
            if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE_VALUE)
            {
                const uint GENERIC_READ = 0x80000000;
                const uint GENERIC_WRITE = 0x40000000;

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

            if (src->no != count)
            {
                throw new ArgumentException("Invalid block.");
            }

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
                if (!IsValid(src, null) || !IsGenesis(src))
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
                if (!IsValid(src, previous.hash))
                {
                    throw new ArgumentException("Invalid genesis block.");
                }
            }

            int bytesWritten = 0;

            if (!WriteFile(hFile, new IntPtr(src), 1024, out bytesWritten, IntPtr.Zero))
            {
                throw new Win32Exception(GetLastError());
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
                          GENERIC_READ | GENERIC_WRITE,
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

            if (dst->no != count)
            {
                throw new ArgumentException("Invalid block.");
            }

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
                *dst = Create(0, Sign(System.Text.Encoding.ASCII.GetBytes("Genesis")), Seed.Next(), Secret());
                return true;
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

    public static unsafe void Print(Block block)
    {
        System.Console.WriteLine($"Hash: {Hex(block.GetHash())}");

        System.Console.WriteLine($"Previous: {Hex(GetPrevious(&block))}");

        if (block.len > 0)
        {
            System.Console.WriteLine($"Data: {Hex(block.GetSecret())}");
        }

        int no = block.no;

        System.Console.WriteLine($"No: {no}");

        var timestamp = block.timestamp;

        System.Console.WriteLine($"Timestamp: {(new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(timestamp).ToLocalTime()}");

        var nonce = block.nonce;

        System.Console.WriteLine($"Nonce: {nonce}");

        System.Console.WriteLine($"Verified: {IsValid(&block, null)}");

        System.Console.WriteLine();
    }

    public static System.Random Seed = new System.Random(7919);

    static byte[] Secret()
    {
        return System.BitConverter.GetBytes(Seed.NextDouble());
    }

    public static unsafe void Main(string[] args)
    {
        string FILE = "Genesis";

        Block LatestBlock;

        GetLatestBlock(FILE, &LatestBlock);

        if (IsGenesis(&LatestBlock) && IsValid(&LatestBlock, null))
        {
            Append(FILE, &LatestBlock);

            Print(LatestBlock);
        }

        for (var i = 0; i < 1024; i++)
        {
            LatestBlock = Create(&LatestBlock, Seed.Next(), Secret());

            Append(FILE, &LatestBlock);

            Print(LatestBlock);
        }

        System.Console.ReadKey();
    }

}