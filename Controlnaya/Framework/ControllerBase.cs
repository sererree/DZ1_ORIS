using System;
using System.Net;

namespace MyHttpServer_ORIS.Framework;

public class ControllerBase
{
    protected HttpListenerContext Context {get; private set;}

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

}
