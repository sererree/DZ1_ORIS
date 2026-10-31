using System;
using System.Collections.Generic;
using System.Text;

namespace MyHttpServer.Framework.Attributes
{

    public class HttpControllerAttribute : Attribute
    {
        public string Route { get; init; }

        public HttpControllerAttribute(string route)
        {
            Route = route;
        }
    }
}
