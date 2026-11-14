using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public interface IResponseResult
    {
        Task ExecuteAsync(HttpListenerContext context);
    }
}