using System;
using System.Collections.Generic;
using System.Text;

namespace MyHttpServer.Framework.Attributes
{
    public class PostAttribute : Attribute
    {
        public string Route { get; init; }

        public PostAttribute(string route)
        {
            Route = route;
        }
    }
}
