using System.Threading.Tasks;

namespace Blocks
{
    public class Serv
    {
        public static Task Welcome(Microsoft.Owin.IOwinContext ctx)
        {
            ctx.Response.Write("OK");

            return Task.CompletedTask;
        }
    }
}
