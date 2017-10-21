using Microsoft.Owin;
using Owin;
using System;

namespace Blocks
{
    public class App
    {
        public static string FILE = "Genesis";

        static IDisposable _listener;

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

            Log($"Starting a new node on http://localhost:{port}");

            _listener = Microsoft.Owin.Hosting.WebApp.Start<TStartup>(url: $"http://+:{port}");

            Log($"\r\nReady.");
        }

        public void Configuration(Owin.IAppBuilder host)
        {
            Owin.MapExtensions.Map(host, "/blocks", (app) =>
            {
                app.Run((IOwinContext ctx) => { return Blocks.Serv.Blocks(ctx); });
            });

            Owin.MapExtensions.Map(host, string.Empty, (app) =>
            {
                app.Run((IOwinContext ctx) => { return Blocks.Serv.Blocks(ctx); });
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

            Log($"Validating database...\r\n");

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
                    if (Database.IsValidBlock(b, p))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        if (p == null)
                        {
                            if (!Database.IsGenesis(b))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                }
                /*
                Database.Print(b, System.Console.Out);
                */
                Console.ResetColor();

                previous = b->GetHash();
            });

            if (!Database.GetLatestBlock(FILE, &block))
            {
                Error($"Could not get latest block.");
            }
            
            Database.Print(&block, System.Console.Out);

            Log($"Done.\r\n");

            try
            {
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