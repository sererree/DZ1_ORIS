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

            // Убираем _basePath (например, /connection) из пути запроса
            if (!string.IsNullOrEmpty(_basePath) && path.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(_basePath.Length);
            }

            path = path.TrimEnd('/');
            if (string.IsNullOrEmpty(path)) path = "/";
            string httpMethod = request.HttpMethod;

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            // Важно: ищем как public, так и internal контроллеры
            var controllerTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<HttpControllerAttribute>() != null);

            foreach (var type in controllerTypes)
            {
                var controllerAttr = type.GetCustomAttribute<HttpControllerAttribute>();
                string controllerRoute = ("/" + controllerAttr.Route.Trim('/')).TrimEnd('/');

                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    string methodRoute = "";
                    bool isMatch = false;

                    if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        var getAttr = method.GetCustomAttribute<GetAttribute>();
                        if (getAttr != null)
                        {
                            methodRoute = "/" + getAttr.Route.Trim('/');
                            isMatch = true;
                        }
                    }
                    else if (httpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        var postAttr = method.GetCustomAttribute<PostAttribute>();
                        if (postAttr != null)
                        {
                            methodRoute = "/" + postAttr.Route.Trim('/');
                            isMatch = true;
                        }
                    }
                    else if (httpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
                        {
                            var putAttr = method.GetCustomAttribute<PutAttribute>();
                            if (putAttr != null)
                            {
                                methodRoute = "/" + putAttr.Route.Trim('/');
                                isMatch = true;
                            }
                        }

                    if (!isMatch) continue;

                    string fullRoute = (controllerRoute + methodRoute).TrimEnd('/');
                    if (string.IsNullOrEmpty(fullRoute)) fullRoute = "/";

                    if (fullRoute.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
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

                            var methodParams = method.GetParameters();
                            object[] args = new object[methodParams.Length];

                            for (int i = 0; i < methodParams.Length; i++)
                            {
                                string paramName = methodParams[i].Name;
                                // Проверяем значение по имени параметра, или по логину/email
                                string value = formParams[paramName] ?? queryParams[paramName];

                                if (value == null && paramName.Equals("login", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = formParams["email"] ?? queryParams["email"];
                                }

                                args[i] = value != null
                                    ? Convert.ChangeType(value, methodParams[i].ParameterType)
                                    : null;
                            }

                            object result = method.Invoke(instance, args);

                            if (result is IResponseResult responseResult)
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
                }
            }

            if (Successor != null)
            {
                await Successor.HandleRequestAsync(context);
            }
        }
    }
}