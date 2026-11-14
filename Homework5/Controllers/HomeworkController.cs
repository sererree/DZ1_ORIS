using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using MyHttpServer_ORIS.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Controllers
{
    [HttpController("home-work")]
    public class HomeworkController : ControllerBase
    {
        [Get("send")]
        public IResponseResult Send(string to)
        {
            Console.WriteLine($"[HomeworkController] Запрос получен! to = {to}");
            if (string.IsNullOrEmpty(to))
            {
                return new TextResult("Ошибка: параметр 'to' не указан.");
            }

            
            _ = Task.Run(async () =>
            {
                try
                {
                    string zipFileName = "MyHttpServer_ORIS.zip"; 
                    string zipPath = Path.Combine(Directory.GetCurrentDirectory(), zipFileName);

                    if (!File.Exists(zipPath))
                    {
                        Console.WriteLine($"[HomeworkController ФОН] Ошибка: файл архива '{zipFileName}' не найден в папке: {Directory.GetCurrentDirectory()}");
                        return;
                    }

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress("rialimova@kpfu.ru"); 
                        message.To.Add(to);
                        message.Subject = "Домашнее задание: MyHttpServer";
                        message.Body = "Группа: 11-541\nФИО: Алимова Регина Ильдаровна";

                        message.Attachments.Add(new Attachment(zipPath));

                        using (var smtpClient = new SmtpClient("smtp.yandex.ru", 587))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential("rialimova@kpfu.ru", "fmjrxkblqurxbnvl");
                            smtpClient.EnableSsl = true;
                            
                            await smtpClient.SendMailAsync(message);
                            Console.WriteLine("[HomeworkController ФОН] Письмо успешно отправлено!");
                        }
                    }
                }
                catch (Exception bgEx)
                {
                    Console.WriteLine($"[HomeworkController ФОН ОШИБКА]: {bgEx.ToString()}");
                }
            });

            
            string responseText = $"Запрос успешно принят в обработку!\nПолучатель: {to}\nГруппа: 11-541\nФИО: Алимова Регина Ильдаровна";
            return new TextResult(responseText);
        }
    }
}