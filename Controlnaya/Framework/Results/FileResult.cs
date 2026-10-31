using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Results
{
    public class FileResult : IResponseResult
    {
        private readonly string _filePath;
        private readonly string _contentType;

        public FileResult(string filePath, string contentType = "text/html; charset=utf-8")
        {
            _filePath = filePath;
            _contentType = contentType;
        }

        public async Task ExecuteAsync(HttpListenerContext context)
        {
            var response = context.Response;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "static", _filePath);

            if (!File.Exists(fullPath))
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            byte[] buffer = await File.ReadAllBytesAsync(fullPath);
            response.ContentType = _contentType;
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.OK;

            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            await output.FlushAsync();
        }
    }
}