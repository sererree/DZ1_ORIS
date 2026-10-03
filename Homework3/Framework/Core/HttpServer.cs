using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private HttpListener _listener;
        private bool _isRunning;
        private string _basePath = "";

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start()
        {
            try
            {
                if (!File.Exists("settings.json"))
                {
                    Console.WriteLine("Ошибка: Файл settings.json не найден! Текущая папка: " + Directory.GetCurrentDirectory());
                    return;
                }

                string settingsJson = File.ReadAllText("settings.json");
                Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

                // Сохраняем Path из настроек, чтобы корректно отрезать его при маршрутизации
                _basePath = "/" + setting.Server.Path.Trim('/');
                if (_basePath == "/") _basePath = "";

                string urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}{_basePath}/";

                _listener.Prefixes.Clear();
                _listener.Prefixes.Add(urlPrefix);

                _listener.Start();
                _isRunning = true;
                Console.WriteLine("Сервер запущен и слушает: " + urlPrefix);

                _ = ListenAsync();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Ошибка доступа к сети (нужны права администратора?): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка при запуске: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер завершил работу");
        }

        private async Task ListenAsync()
        {
            while (true)
            {
                if (!_isRunning) break;

                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка прослушивания: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;

            try
            {
                // 1. Извлекаем локальный путь из URL-адреса
                string localPath = request.Url.AbsolutePath;

                // Убираем базовый путь (например, /connection), если он есть
                if (!string.IsNullOrEmpty(_basePath) && localPath.StartsWith(_basePath))
                {
                    localPath = localPath.Substring(_basePath.Length);
                }

                // Убираем начальный слеш, чтобы получить относительный путь к файлу
                localPath = localPath.TrimStart('/');

                if (string.IsNullOrEmpty(localPath))
{
    localPath = "search.html";
}

string fileName = Path.GetFileName(localPath);
string filePath = string.Empty;

// Сначала пробуем найти точный путь относительно текущей директории
string directPath = Path.Combine(Directory.GetCurrentDirectory(), localPath);
if (File.Exists(directPath))
{
    filePath = directPath;
}
else
{
    // Если точного пути нет, ищем файл по всему проекту (во всех подпапках) по его имени
    string[] foundFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), fileName, SearchOption.AllDirectories);

    // Исключаем системные папки вроде bin и obj, чтобы не цеплять старые кэши
    var validFiles = foundFiles.Where(f => !f.Contains("/bin/") && !f.Contains("/obj/")).ToArray();

    if (validFiles.Length > 0)
    {
        filePath = validFiles[0]; // Берем первый попавшийся файл с таким именем
    }
}

                // 3. Проверяем существование запрашиваемого файла
                if (!File.Exists(filePath))
                {

                    Console.WriteLine($"Ошибка: Файл {filePath} не найден!");
                    response.StatusCode = (int)HttpStatusCode.NotFound;

                    string notFoundPagePath = Path.Combine("static", "404.html");
                    string notFoundPath = Path.Combine("static", "404.html");

                    if (File.Exists(notFoundPath))
                    {
                        byte[] notFoundBytes = await File.ReadAllBytesAsync(notFoundPath);
                        response.ContentType = "text/html; charset=utf-8";
                        response.ContentLength64 = notFoundBytes.Length;

                        using (Stream errOutput = response.OutputStream)
                        {
                            await errOutput.WriteAsync(notFoundBytes, 0, notFoundBytes.Length);
                            await errOutput.FlushAsync();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[Ошибка] Файл 404.html не найден по пути: {Path.GetFullPath(notFoundPath)}");
                        response.Close();
                    }
                    return;
                }

                // 4. Читаем файл как массив байтов (безопасно для картинок)
                byte[] buffer = File.ReadAllBytes(filePath);

                // 5. Устанавливаем правильный Content-Type
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                response.ContentType = extension switch
                {
                    ".html" => "text/html; charset=utf-8",
                    ".css" => "text/css; charset=utf-8",
                    ".js" => "application/javascript; charset=utf-8",
                    ".svg" => "image/svg+xml",
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".ico" => "image/x-icon",
                    _ => "application/octet-stream"
                };

                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();

                Console.WriteLine($"Запрос обработан: {localPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Close();
            }
        }
    }
}