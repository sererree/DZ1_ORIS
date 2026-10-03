using System.Net;
using System.Text;
using System.Text.Json;
using MyHttpServer.Framework.Core;


HttpServer server = new HttpServer();
server.Start();

Console.WriteLine("Введите 'exit' для остановки сервера...");

while (true)
{
    string command = Console.ReadLine();
    if (command?.ToLower() == "exit")
    {
        server.Stop();
        break;
    }
}









