using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Blocks
{
    public static class Serv
    {
        static void Cors(Microsoft.Owin.IOwinContext ctx)
        {
            ctx.Response.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Response.Headers.Set("Pragma", "no-cache");
            ctx.Response.Headers.Set("Expires", "0");

            ctx.Response.Headers.Set("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Set("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
            ctx.Response.Headers.Set("Access-Control-Allow-Methods", "*");

            ctx.Response.Headers.Set("X-Content-Type-Options", "nosniff");
        }

        static Task OK(Microsoft.Owin.IOwinContext ctx, string data)
        {
            ctx.Response.StatusCode = 200;

            if (!string.IsNullOrWhiteSpace(data))
            {
                ctx.Response.ContentType = "text/plain";
                return ctx.Response.WriteAsync(data);
            }

            return Task.CompletedTask;
        }

        static Task JSON(Microsoft.Owin.IOwinContext ctx, string data)
        {
            ctx.Response.StatusCode = 200;

            if (!string.IsNullOrWhiteSpace(data))
            {
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(data);
            }

            return Task.CompletedTask;
        }

        static Task Error(Microsoft.Owin.IOwinContext ctx, int statusCode = 500, string data = null)
        {
            if (statusCode == 200)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            ctx.Response.StatusCode = statusCode;

            if (!string.IsNullOrWhiteSpace(data))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Write(data);
            }

            return Task.CompletedTask;
        }

        public unsafe static Task GetBlocks(Microsoft.Owin.IOwinContext ctx)
        {
            Cors(ctx);

            var plain = new StringBuilder();

            using (System.IO.TextWriter dst = new StringWriter(plain))
            {
                Database.Map(App.FILE, (i) =>
                {
                    Block* block = (Block*)i;

                    Utils.Print(
                            block, 
                            dst);
                });
            }

            return OK(ctx, plain.ToString());            
        }

        public unsafe static Task GetLatestBlock(Microsoft.Owin.IOwinContext ctx)
        {            
            Block block;

            if (!Database.GetLatestBlock(App.FILE, &block))
            {
                return Error(ctx);
            }

            Cors(ctx);

            var json = new StringBuilder();

            using (System.IO.TextWriter dst = new StringWriter(json))
            {
                Utils.JSON(
                        &block,
                        dst);
            }

            return JSON(ctx, json.ToString());
        }
    }
}
