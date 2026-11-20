using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using System;

namespace MyHttpServer.Controllers
{
    [HttpController("auth")]
    public class AuthController
    {
        [Get("login")]
        public IHttpResult Login()
        {
            // Указываем относительный путь к файлу внутри папки static
            return new FileResult("Login Form/login.html", "text/html; charset=utf-8");
        }

        [Post("login")]
        public IHttpResult Login(string login, string password)
        {
            Console.WriteLine($"[AuthController] Получены данные авторизации:");
            Console.WriteLine($" -> Логин: {login}");
            Console.WriteLine($" -> Пароль: {password}");

            return new TextResult($"Пользователь '{login}' успешно обработан.");
        }

    }
}