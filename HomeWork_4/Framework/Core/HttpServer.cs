using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.Framework.Core
{
    public class HttpServer
    {
        private HttpListener _listener;
        private bool _isRunning;
        private string _basePath = "";
        private Handler _handlerChain;

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

                _basePath = "/" + setting.Server.Path.Trim('/');
                if (_basePath == "/") _basePath = "";

                // Инициализация цепочки обработчиков
                var controllerHandler = new ControllerHandler(_basePath);
                var staticFileHandler = new StaticFileHandler(_basePath);
                

                controllerHandler.Successor = staticFileHandler;
                _handlerChain = controllerHandler;

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
            while (_isRunning)
            {
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
            try
            {
                await _handlerChain.HandleRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }
    }
}