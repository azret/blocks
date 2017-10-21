using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Blocks
{
    public static class Serv
    {
        static Task Text(Microsoft.Owin.IOwinContext ctx, string text)
        {
            ctx.Response.Headers.Set("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Response.Headers.Set("Pragma", "no-cache");
            ctx.Response.Headers.Set("Expires", "0");

            ctx.Response.Headers.Set("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Set("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
            ctx.Response.Headers.Set("Access-Control-Allow-Methods", "*");

            ctx.Response.Headers.Set("X-Content-Type-Options", "nosniff");

            ctx.Response.ContentType = "text/plain";
            ctx.Response.StatusCode = 200;

            ctx.Response.Write(text);

            return Task.CompletedTask;
        }

        public unsafe static Task Blocks(Microsoft.Owin.IOwinContext ctx)
        {
            var buffer = new StringBuilder();

            using (System.IO.TextWriter writer = new StringWriter(buffer))
            {
                Database.Map(App.FILE, (i) =>
                {
                    Block* block = (Block*)i;

                    Database.Print(
                            block, 
                            writer);
                });
            }

            return Text(ctx, buffer.ToString());            
        }
    }
}
