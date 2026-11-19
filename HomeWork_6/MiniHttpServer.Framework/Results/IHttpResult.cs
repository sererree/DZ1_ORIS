using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public interface IHttpResult
    {
        Task ExecuteAsync(HttpListenerContext context);
    }
}