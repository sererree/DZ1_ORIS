using System;
using System.Net;
using MyHttpServer.Framework.Results;

namespace MyHttpServer_ORIS.Framework;

public class BaseController
{
    protected HttpListenerContext Context {get; private set;}

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IHttpResult Page(string templatePath, object model) => new PageResult(templatePath, model);
    protected IHttpResult Html(string html) => new HtmlResult(html);

}
