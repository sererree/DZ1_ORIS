using System.Collections.Specialized;
using System.Reflection;

namespace MyHttpServer.Framework.Handlers;

/// <summary>Converts query-string and form-body values to controller action arguments.</summary>
public static class RequestParameterBinder
{
    public static object?[] Bind(ParameterInfo[] parameters, NameValueCollection queryParams, NameValueCollection bodyParams)
    {
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            string parameterName = parameters[i].Name ?? string.Empty;
            string? value = bodyParams[parameterName] ?? queryParams[parameterName];

            if (value == null && parameterName.Equals("login", StringComparison.OrdinalIgnoreCase))
                value = bodyParams["email"] ?? queryParams["email"];

            if (value == null)
            {
                args[i] = null;
                continue;
            }

            Type targetType = Nullable.GetUnderlyingType(parameters[i].ParameterType) ?? parameters[i].ParameterType;
            args[i] = Convert.ChangeType(value, targetType);
        }

        return args;
    }
}
