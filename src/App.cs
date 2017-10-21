using Microsoft.Owin;
using Owin;
using System;

namespace Blocks
{
    class App
    {
        static IDisposable _listener;

        static void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void Serv<TStartup>(int port)
        {
            if (_listener != null)
            {
                throw new InvalidOperationException();
            }

            Log($"Staring http://localhost:{port}");

            _listener = Microsoft.Owin.Hosting.WebApp.Start<TStartup>(url: $"http://+:{port}");

            Log($"\r\nReady.");
        }

        public void Configuration(Owin.IAppBuilder host)
        {
            Owin.MapExtensions.Map(host, string.Empty, (app) =>
            {
                app.Run((IOwinContext ctx) => { return Blocks.Serv.Welcome(ctx); });
            });
        }

        public static unsafe void Main(string[] args)
        {
            int? ExitCode = null;

            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            int port = 8000;

            try
            {
                Serv<App>(port);
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
                    Log($"\r\n{error.Message}");

                    Log($"\r\nUse: netsh http add urlacl url = http://+:{port}/ user=everyone listen=yes");
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
                if (true || cki.Modifiers.HasFlag(ConsoleModifiers.Control)
    && cki.Key == ConsoleKey.C)
                {
                    ExitCode = 0;
                }
            }
        }
    }
}