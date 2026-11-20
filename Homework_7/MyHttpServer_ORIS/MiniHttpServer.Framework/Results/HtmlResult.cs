using System.Net;
using System.Text;

namespace MyHttpServer.Framework.Results;

public class HtmlResult : IHttpResult
{
    private readonly string _html;

    public HtmlResult(string html) => _html = html;

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        var response = context.Response;
        byte[] buffer = GetPayload();

        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)HttpStatusCode.OK;

        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer, 0, buffer.Length);
        await output.FlushAsync();
    }

    internal byte[] GetPayload() => Encoding.UTF8.GetBytes(_html);
}