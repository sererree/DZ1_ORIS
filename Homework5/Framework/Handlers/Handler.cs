using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers
{
    public abstract class Handler
    {
        public Handler Successor { get; set; }
        public abstract Task HandleRequestAsync(HttpListenerContext context);
    }
}