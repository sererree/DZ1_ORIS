using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public class TextResult : IResponseResult
    {
        private readonly string _text;

        public TextResult(string text)
        {
            _text = text;
        }

        public async Task ExecuteAsync(HttpListenerContext context)
        {
            var response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(_text);

            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.OK;

            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            await output.FlushAsync();
        }
    }
}