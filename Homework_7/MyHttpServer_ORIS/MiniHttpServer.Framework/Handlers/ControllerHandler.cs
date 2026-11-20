using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Framework.Handlers
{
    public class ControllerHandler : Handler
    {
        private readonly string _basePath;

        public ControllerHandler(string basePath = "")
        {
            _basePath = basePath.TrimEnd('/');
        }

        public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url.LocalPath;
            path = path.TrimEnd('/');
            if (string.IsNullOrEmpty(path)) path = "/";
            string httpMethod = request.HttpMethod;

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            // Важно: ищем как public, так и internal контроллеры
            var controllerTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<HttpControllerAttribute>() != null);

            foreach (var type in controllerTypes)
            {
                MethodInfo? method = ControllerRouteResolver.FindAction(type, httpMethod, path, _basePath);
                if (method == null) continue;

                try
                {
                    object instance = Activator.CreateInstance(type);
                    NameValueCollection queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                    NameValueCollection formParams = new NameValueCollection();

                    if (request.HasEntityBody)
                    {
                        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                        string bodyStr = await reader.ReadToEndAsync();
                        formParams = HttpUtility.ParseQueryString(bodyStr);
                    }

                    object?[] args = RequestParameterBinder.Bind(method.GetParameters(), queryParams, formParams);
                    object result = method.Invoke(instance, args);

                    if (result is IHttpResult responseResult)
                    {
                        await responseResult.ExecuteAsync(context);
                        response.Close();
                        return;
                    }

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в контроллере: {ex.Message}");
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Close();
                    return;
                }
            }

            if (Successor != null)
            {
                await Successor.HandleRequestAsync(context);
            }
        }
    }
}