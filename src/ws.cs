using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web
{
    public class WebSocks
    {
        struct Socket
        {
            // http://owin.org/spec/extensions/owin-WebSocket-Extension-v0.4.0.htm

            public Socket(IDictionary<string, object> ctx)
            {
                SendAsync = (Func<ArraySegment<byte>, int, bool, System.Threading.CancellationToken, Task>)ctx["websocket.SendAsync"];
                ReceiveAsync = (Func<ArraySegment<byte>, System.Threading.CancellationToken, Task<Tuple<int, bool, int>>>)ctx["websocket.ReceiveAsync"];
                CloseAsync = (Func<int, string, System.Threading.CancellationToken, Task>)ctx["websocket.CloseAsync"];
                CallCancelled = (CancellationToken)ctx["websocket.CallCancelled"];
            }

            public Func<
                ArraySegment<byte>      /* data */,
                int                     /* messageType */,
                bool                    /* endOfMessage */,
                CancellationToken       /* cancel */, Task> SendAsync;

            public Func<
                ArraySegment<byte>      /* data */,
                CancellationToken       /* cancel */,
                Task<Tuple<int          /* messageType */,
                bool                    /* endOfMessage */,
                int                     /* count */>>> ReceiveAsync;

            public Func<
                int                     /* closeStatus */,
                string                  /* closeDescription */,
                CancellationToken       /* cancel */, Task> CloseAsync;

            public CancellationToken CallCancelled;
        }

        public sealed class Subscription
        {
            public static int ConcurrencyLevel
            {
                get
                {
                    return Environment.ProcessorCount * 2;
                }
            }

            static readonly ConcurrentDictionary<string, Subscription> Subscriptions =
                    new ConcurrentDictionary<string, Subscription>(ConcurrencyLevel,
                        101,
                        StringComparer.InvariantCultureIgnoreCase);

            public static Subscription New(IDictionary<string, object> ctx)
            {
                Subscription sub = new Subscription(ctx);

                if (!Subscriptions.TryAdd(sub.Key, sub))
                {
                    throw new InvalidOperationException();
                }

                return sub;
            }

            public static void Broadcast(String msg)
            {
                Broadcast(msg, null);
            }

            public static void Broadcast(String msg, String except)
            {
                foreach (KeyValuePair<string, Subscription> i in Subscriptions)
                {
                    if (i.Key == except)
                    {
                        continue;
                    }

                    Subscription sub = null;

                    if (Subscriptions.TryGetValue(i.Key, out sub) && sub != null)
                    {
                        sub.Send(msg);
                    }
                }
            }

            public static void Send(String to, String msg)
            {
                if (String.IsNullOrWhiteSpace(to))
                {
                    return;
                }

                Subscription sub = null;

                if (Subscriptions.TryGetValue(to, out sub) && sub != null)
                {
                    sub.Send(msg);
                }
            }

            public static void Send(String[] to, String msg)
            {
                foreach (String id in to)
                {
                    if (String.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    Subscription sub = null;

                    if (Subscriptions.TryGetValue(id, out sub) && sub != null)
                    {
                        sub.Send(msg);
                    }
                }
            }

            public static void Close(String id)
            {
                Subscription sub = null;

                if (Subscriptions.TryGetValue(id, out sub) && sub != null)
                {
                    sub.Close();
                }
            }

            Object _lock = new Object();

            private Subscription(IDictionary<string, object> ctx)
            {
                _key = Guid.NewGuid().ToString("N");
                _ctx = ctx;
            }

            String _key = null;

            public String Key
            {
                get
                {
                    String key = _key;
                    return key;
                }
            }

            IDictionary<string, object> _ctx;

            void Abort()
            {
                try
                {
                    WebSocketContext ctx
                        = (WebSocketContext)_ctx["System.Net.WebSockets.WebSocketContext"];

                    ctx.WebSocket.Abort();
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (StackOverflowException)
                {
                    throw;
                }
                catch
                {
                    // Ignore other exceptions...
                }
            }

            internal void Open()
            {
            }

            internal void Close()
            {
                Subscription sub = null;

                if (!Subscriptions.TryRemove(_key, out sub))
                {
                    sub = null;
                }

                if (sub != null && sub != this)
                {
                    sub.Abort();
                }

                Abort();
            }

            static Task sender = Task.CompletedTask;

            public void Send(String msg)
            {
                Action<Task, Object> func = async (ante, state) =>
                {
                    Subscription sub = null;

                    if (!Subscriptions.TryGetValue(((KeyValuePair<String, String>)state).Key, out sub))
                    {
                        sub = null;
                    }

                    try
                    {
                        if (sub != null)
                        {
                            WebSocketContext ctx
                                      = (WebSocketContext)_ctx["System.Net.WebSockets.WebSocketContext"];

                            await ctx.WebSocket.SendAsync(
                                    new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(((KeyValuePair<String, String>)state).Value)),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);
                        }
                    }
                    catch
                    {
                        if (sub != null)
                        {
                            sub.Close();
                        }
                    }
                };

                lock (_lock)
                {
                    Object state = (Object)(new KeyValuePair<String, String>(Key, msg));

                    if (sender == null)
                    {
                        sender = Task.CompletedTask;
                    }

                    sender = sender.ContinueWith(func, state, TaskScheduler.Default).ContinueWith((ante) => {
                        // Continue with a No-Op...
                    });
                }

            }
        }

        static async Task Listen(IDictionary<string, object> ctx)
        {
            Socket Socket = new Socket(ctx);

            Action<Tuple<int, bool, int>, byte[], string> Dispatch = (received, buffer, sender) => {
                if (received.Item3 > 0)
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, received.Item3);

                        JObject jObj;
                        try
                        {
                            jObj = JObject.Parse(json);
                        }
                        catch
                        {
                            jObj = new JObject()
                            {
                                ["_type"] = "MSG",
                                ["msg"] = json
                            };
                        }

                        jObj["sender"] = sender;

                        switch (jObj.Value<String>("_type"))
                        {
                            case "MSG":
                                var jMsg = jObj["msg"];
                                if (jMsg != null)
                                {
                                //    Config.Redis.PubSub().Publish($"play://chatter?{sender}", jObj.ToString(), CommandFlags.FireAndForget);
                                }
                                break;
                        };
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            };

            Subscription Sub = Subscription.New(ctx);

            try
            {
                if (Sub != null)
                {
                    Sub.Open();
                }

                try
                {
                    Sub.Send("WELCOME");

                    byte[] buffer = new byte[1024];

                    Tuple<int, bool, int> received = await Socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), Socket.CallCancelled);

                    Dispatch(received, buffer, Sub.Key);

                    object status = 0;

                    while (!ctx.TryGetValue("websocket.ClientCloseStatus", out status) || (int)status == 0)
                    {
                        received = await Socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), Socket.CallCancelled);

                        Dispatch(received, buffer, Sub.Key);
                    }

                }
                finally
                {
                    WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure;

                    if (ctx.ContainsKey("websocket.ClientCloseStatus"))
                    {
                        status = (WebSocketCloseStatus)ctx["websocket.ClientCloseStatus"];
                    }

                    await Socket.CloseAsync(
                        (int)status,
                        (string)ctx["websocket.ClientCloseDescription"],
                        Socket.CallCancelled);
                }
            }
            finally
            {
                if (Sub != null)
                {
                    Sub.Close();
                }
            }
        }

        public static Task Connect(IOwinContext ctx)
        {
            var accept = ctx.Get<Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>>("websocket.Accept");

            var path = ctx.Request.Path;

            if (!path.HasValue || path.Value == "/")
            {
                if (String.Compare("GET", ctx.Request.Method, true) == 0)
                {
                    if (accept != null)
                    {
                        ctx.Response.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");
                        ctx.Response.Headers.Set("Pragma", "no-cache");
                        ctx.Response.Headers.Set("Expires", "0");

                        ctx.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                        ctx.Response.Headers.Append("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
                        ctx.Response.Headers.Append("Access-Control-Allow-Methods", "*");

                        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                        Object requestHeaders = null;
                        if (!ctx.Environment.TryGetValue("owin.RequestHeaders", out requestHeaders))
                        {
                            requestHeaders = null;
                        }
                        Dictionary<string, object> acceptOptions = null;
                        if (requestHeaders is IDictionary<string, string[]>)
                        {
                            string[] subProtocols;
                            if (((IDictionary<string, string[]>)requestHeaders).TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                            {
                                acceptOptions = new Dictionary<string, object>();
                                acceptOptions.Add("websocket.SubProtocol", subProtocols[0].Split(',').First().Trim());
                            }
                        }

                        Object responseBuffering = null;
                        if (!ctx.Environment.TryGetValue("server.DisableResponseBuffering", out responseBuffering))
                        {
                            requestHeaders = null;
                        }
                        if (responseBuffering is Action)
                        {
                            ((Action)responseBuffering)();
                        }

                        Object responseCompression = null;
                        if (!ctx.Environment.TryGetValue("systemweb.DisableResponseCompression", out responseCompression))
                        {
                            responseCompression = null;
                        }
                        if (responseCompression is Action)
                        {
                            ((Action)responseCompression)();
                        }

                        try
                        {
                            accept(acceptOptions, Listen);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        return Task.CompletedTask;
                    }
                }
            }

            throw new NotSupportedException();
        }
    }
}
