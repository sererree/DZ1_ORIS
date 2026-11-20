using System.Reflection;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Framework.Handlers;

/// <summary>Finds a controller action by HTTP method and its attribute-defined route.</summary>
public static class ControllerRouteResolver
{
    public static MethodInfo? FindAction(Type controllerType, string httpMethod, string requestPath, string basePath = "")
    {
        var controller = controllerType.GetCustomAttribute<HttpControllerAttribute>();
        if (controller == null) return null;

        string path = requestPath;
        string normalizedBase = basePath.TrimEnd('/');
        if (!string.IsNullOrEmpty(normalizedBase) && path.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            path = path[normalizedBase.Length..];
        path = "/" + path.Trim('/');

        string controllerRoute = "/" + controller.Route.Trim('/');
        foreach (MethodInfo method in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            string? actionRoute = httpMethod.ToUpperInvariant() switch
            {
                "GET" => method.GetCustomAttribute<GetAttribute>()?.Route,
                "POST" => method.GetCustomAttribute<PostAttribute>()?.Route,
                _ => null
            };

            if (actionRoute == null) continue;
            string fullRoute = (controllerRoute + "/" + actionRoute.Trim('/')).TrimEnd('/');
            if (string.IsNullOrEmpty(fullRoute)) fullRoute = "/";
            if (fullRoute.Equals(path, StringComparison.OrdinalIgnoreCase)) return method;
        }

        return null;
    }
}
