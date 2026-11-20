using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer.Framework.Results;

public class JsonResult : IHttpResult
{
    private readonly object _value;

    public JsonResult(object value) => _value = value;

    public string Serialize() => JsonSerializer.Serialize(_value);

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(Serialize());
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        using Stream output = context.Response.OutputStream;
        await output.WriteAsync(buffer, 0, buffer.Length);
        await output.FlushAsync();
    }
}
