using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers
{
    public class StaticFileHandler : Handler
    {
        private readonly string _basePath;

        public StaticFileHandler(string basePath = "")
        {
            _basePath = basePath.TrimEnd('/');
        }

        public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url.LocalPath;

            
            if (!string.IsNullOrEmpty(_basePath) && path.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(_basePath.Length);
            }

            string relativePath = path.TrimStart('/');

            // Если обратились к корню — отдаем login.html
            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = Path.Combine("Login Form", "login.html");
            }

            string staticRootDir = Path.Combine(Directory.GetCurrentDirectory(), "static");
            string filePath = FindFileInStatic(staticRootDir, relativePath);

            if (filePath != null && File.Exists(filePath))
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    response.ContentType = fileInfo.Extension.ToLower() switch
                    {
                        ".html" => "text/html; charset=utf-8",
                        ".css" => "text/css; charset=utf-8",
                        ".js" => "application/javascript; charset=utf-8",
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        ".svg" => "image/svg+xml",
                        ".webp" => "image/webp",
                        ".ico" => "image/x-icon",
                        ".woff" => "font/woff",
                        ".woff2" => "font/woff2",
                        ".ttf" => "font/ttf",
                        _ => "application/octet-stream"
                    };

                    byte[] buffer = await File.ReadAllBytesAsync(filePath);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.OK;

                    using (Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        await output.FlushAsync();
                    }

                    response.Close();
                    return;
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await WriteResponseAsync(response, $"Ошибка при обработке файла: {ex.Message}");
                    return;
                }
            }

            // Передаем дальше по цепочке
            if (Successor != null)
            {
                await Successor.HandleRequestAsync(context);
            }
            else
            {
                // Если ни контроллер, ни файл не нашлись — это чистый 404!
                response.StatusCode = (int)HttpStatusCode.NotFound;
                
                string notFoundPath = FindFileInStatic(staticRootDir, "404.html");

                if (notFoundPath != null && File.Exists(notFoundPath))
                {
                    Console.WriteLine($"[404] Отдаем файл: {notFoundPath}");
                    byte[] buffer = await File.ReadAllBytesAsync(notFoundPath);
                    response.ContentType = "text/html; charset=utf-8";
                    response.ContentLength64 = buffer.Length;

                    using (Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        await output.FlushAsync();
                    }
                }
                
            }
        }

        /// <summary>
        /// Поиск файла по всей папке static и ее подпапкам.
        /// </summary>
        private string FindFileInStatic(string staticRootDir, string relativePath)
        {
            if (!Directory.Exists(staticRootDir)) return null;

            // Нормализуем слэши для файловой системы
            string normalizedRelative = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                                     .Replace('\\', Path.DirectorySeparatorChar);

            // 1. Прямой путь от папки static
            string directPath = Path.Combine(staticRootDir, normalizedRelative);
            if (File.Exists(directPath)) return directPath;

            // Ищем все файлы во всех подпапках static
            var allFiles = Directory.GetFiles(staticRootDir, "*", SearchOption.AllDirectories);

            // 2. Сначала ищем по точному совпадению конца пути (например, "Login Form\image\logo.png")
            foreach (var file in allFiles)
            {
                if (file.EndsWith(normalizedRelative, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            // 3. Если не нашли, берем только имя файла с расширением (например, "logo.png" или "bg.jpg")
            string fileName = Path.GetFileName(normalizedRelative);
            if (!string.IsNullOrEmpty(fileName))
            {
                foreach (var file in allFiles)
                {
                    if (Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        private static async Task WriteResponseAsync(HttpListenerResponse response, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}