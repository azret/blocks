using Microsoft.Owin;
using Owin;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Blocks
{
    public class App
    {
        public static string FILE = "Genesis";

        static IDisposable _listener;

        static void Write(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(msg);
            Console.ResetColor();
        }

        static void Log(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void Yellow(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void Serv<TStartup>(int port)
        {
            if (_listener != null)
            {
                throw new InvalidOperationException();
            }

            _listener = Microsoft.Owin.Hosting.WebApp.Start<TStartup>(url: $"http://+:{port}");
        }

        public void Configuration(Owin.IAppBuilder host)
        {
            Owin.MapExtensions.Map(host, "/connect", (app) =>
            {
                app.Run((IOwinContext ctx) => { return System.Web.WebSocks.Connect(ctx); });
            });             

            Owin.MapExtensions.Map(host, "/blocks", (app) =>
            {
                app.Run((IOwinContext ctx) => { return Blocks.Serv.GetBlocks(ctx); });
            });

            Owin.MapExtensions.Map(host, string.Empty, (app) =>
            {
                app.Run((IOwinContext ctx) => { return Blocks.Serv.GetLatestBlock(ctx); });
            });
        }

        static unsafe void Main(string[] args)
        {
            int PORT = 8000;

            try
            {
                Console.Title = $"Node ({PORT})";
            }
            catch
            {
            }

            int? ExitCode = null;

            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            Block block;

            Log($"Validating blockchain: {System.IO.Path.GetFileName(FILE)}\r\n");

            if (!Database.GetLatestBlock(FILE, &block))
            {
                block = Database.CreateBlock(0, Database.Genesis, Database.Seed.Next(), null);

                System.Diagnostics.Debug.Assert(Database.IsGenesis(&block));

                if (Database.AppendBlock(FILE, &block) <= 0)
                {
                    Error($"Could not create genesis block.");
                }
            }

            byte[] previous = null;

            Database.Map(FILE, (i) =>
            {
                Block* b = (Block*)i;

                fixed (byte* p = previous)
                {
                    if (!Database.IsValidBlock(b, p))
                    {
                        throw new System.IO.InvalidDataException();
                    }
                }

                previous = b->GetHash();
            });

            if (!Database.GetLatestBlock(FILE, &block))
            {
                Error($"Could not get latest block.");
            }

            Console.ForegroundColor = ConsoleColor.Green;

            Utils.Print(&block, System.Console.Out);

            Console.ResetColor();

            try
            {
                Log($"Starting a new node");

                Serv<App>(PORT);
            }
            catch (Exception e)
            {
                System.Net.HttpListenerException error = e as System.Net.HttpListenerException;

                if (error == null)
                {
                    error = e.InnerException as System.Net.HttpListenerException;
                }

                if (error != null && error.NativeErrorCode == 0x5)
                {
                    Error($"\r\n{error.Message}");

                    Yellow($"\r\nUse: netsh http add urlacl url = http://+:{PORT}/ user=everyone listen=yes");
                }
                else
                {
                    if (e.InnerException != null) throw e.InnerException;

                    throw e;
                }
            }

            Log($"\r\nReady.\r\n");

            Yellow($"http://localhost:{PORT}");

            Log("\r\nPress any key to quit...\r\n");

            while (!ExitCode.HasValue)
            {
                ConsoleKeyInfo cki = System.Console.ReadKey(true);

                if (true || cki.Modifiers.HasFlag(ConsoleModifiers.Control) && cki.Key == ConsoleKey.C)
                {
                    ExitCode = 0;
                }
            }
        }
    }
}