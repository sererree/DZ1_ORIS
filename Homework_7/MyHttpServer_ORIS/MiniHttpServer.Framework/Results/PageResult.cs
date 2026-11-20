using System.Net;
using MiniHttpServer.HtmlTemplating;

namespace MyHttpServer.Framework.Results;

public class PageResult : IHttpResult
{
    private readonly string _templatePath;
    private readonly object _model;
    private readonly IHtmlTemplateRenderer _renderer;

    public PageResult(string templatePath, object model, IHtmlTemplateRenderer? renderer = null)
    {
        _templatePath = templatePath;
        _model = model;
        _renderer = renderer ?? new HtmlTemplateRenderer();
    }

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", _templatePath);

        if (!File.Exists(fullPath))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        string html = RenderTemplate(fullPath);
        await new HtmlResult(html).ExecuteAsync(context);
    }

    internal string RenderTemplate(string fullPath) => _renderer.RenderFromFile(fullPath, _model);
}