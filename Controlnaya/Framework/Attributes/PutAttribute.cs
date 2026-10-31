using System;

namespace MyHttpServer.Framework.Attributes
{
    public class PutAttribute : Attribute
    {
        public string Route { get; init; }

        public PutAttribute(string route)
        {
            Route = route;
        }
    }
}