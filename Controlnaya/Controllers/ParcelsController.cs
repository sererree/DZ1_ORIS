using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;

namespace MyHttpServer.Parcels
{
    
    public class ParcelHtmlResult : IResponseResult
    {
        
        public const string ListToken = "@LIST@";

        private readonly int _status;
        private readonly string _html;
        public ParcelHtmlResult(int status, string html) { _status = status; _html = html; }

        public static string ListUrl(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            int i = path.IndexOf("/parcels", StringComparison.OrdinalIgnoreCase);
            return i < 0 ? "/parcels" : path.Substring(0, i + "/parcels".Length);
        }

        public async Task ExecuteAsync(HttpListenerContext context)
        {
            var response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(_html.Replace(ListToken, ListUrl(context)));
            response.StatusCode = _status;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public class ParcelSeeOtherResult : IResponseResult
    {
        public Task ExecuteAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.SeeOther;
            context.Response.RedirectLocation = ParcelHtmlResult.ListUrl(context);
            return Task.CompletedTask;
        }
    }

    public class ParcelNoContentResult : IResponseResult
    {
        public Task ExecuteAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            return Task.CompletedTask;
        }
    }

    
    [HttpController("/parcels")]
    public class ParcelsController
    {
        private const string L = ParcelHtmlResult.ListToken;

        
        private static readonly ParcelStore Store = new();

        private static string E(string s) => WebUtility.HtmlEncode(s ?? "");

        private static IResponseResult Html(string title, string body) =>
            new ParcelHtmlResult(200, Page(title, body));

        private static string Page(string title, string body) =>
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\">" +
            $"<title>{E(title)}</title></head><body>{body}</body></html>";

        private static IResponseResult ErrorPage(OpStatus status, string message)
        {
            int code = status == OpStatus.NotFound ? 404 : 400;
            string title = code == 404 ? "404 Not Found" : "400 Bad Request";
            return new ParcelHtmlResult(code, Page(title,
                $"<h1>{title}</h1><p>{E(message)}</p>" +
                $"<a href=\"{L}\">К списку</a> | <a href=\"javascript:history.back()\">Назад</a>"));
        }

        
        [Get("")]
        public IResponseResult List(string minWeight)
        {
            var r = Store.GetList(minWeight);
            if (r.Status != OpStatus.Ok) return ErrorPage(r.Status, r.Error);

            var sb = new StringBuilder();
            sb.Append("<h1>Посылки</h1>");
            sb.Append($"<form method=\"get\" action=\"{L}\">");
            sb.Append($"Минимальный вес: <input name=\"minWeight\" value=\"{E(minWeight)}\"> ");
            sb.Append("<button type=\"submit\">Фильтровать</button></form>");

            sb.Append("<table border=\"1\" cellpadding=\"4\"><tr><th>Id</th><th>Получатель</th><th>Вес, г</th><th>Статус</th></tr>");
            foreach (var p in r.Value.Items)
            {
                sb.Append($"<tr><td><a href=\"{L}/details?id={p.Id}\">{p.Id}</a></td>");
                sb.Append($"<td>{E(p.Recipient)}</td><td>{p.Weight}</td><td>{E(p.Status)}</td></tr>");
            }
            sb.Append("</table>");
            sb.Append($"<p><b>Общий вес посылок в выборке: {r.Value.TotalWeight} г</b></p>");
            sb.Append($"<p><a href=\"{L}/create\">Зарегистрировать посылку</a></p>");

            return Html("Посылки", sb.ToString());
        }

        
        [Get("/details")]
        public IResponseResult Details(string id)
        {
            var r = Store.GetById(id);
            if (r.Status != OpStatus.Ok) return ErrorPage(r.Status, r.Error);

            var p = r.Value;
            return Html("Посылка", 
                $"<h1>Посылка №{p.Id}</h1>" +
                $"<p>Получатель: {E(p.Recipient)}</p>" +
                $"<p>Вес: {p.Weight} г</p>" +
                $"<p>Статус: {E(p.Status)}</p>" +
                $"<a href=\"{L}/edit?id={p.Id}\">Редактировать</a> | <a href=\"{L}\">К списку</a>");
        }

        
        [Get("/create")]
        public IResponseResult CreateForm()
        {
            return Html("Новая посылка",
                "<h1>Зарегистрировать посылку</h1>" +
                $"<form method=\"post\" action=\"{L}/create\">" +
                "Получатель: <input name=\"recipient\"<br>" +
                "Вес (г): <input name=\"weight\" type=\"number\" min=\"1\" max=\"20000\" required><br>" +
                "<button type=\"submit\">Создать</button></form>" +
                $"<p><a href=\"{L}\">К списку</a></p>");
        }

        
        [Post("/create")]
        public IResponseResult Create(string recipient, string weight)
        {
            var r = Store.Create(recipient, weight);
            if (r.Status != OpStatus.Ok) return ErrorPage(r.Status, r.Error);
            return new ParcelSeeOtherResult();
        }

        
        [Get("/edit")]
        public IResponseResult EditForm(string id)
        {
            var r = Store.GetById(id);
            if (r.Status != OpStatus.Ok) return ErrorPage(r.Status, r.Error);

            var p = r.Value;
            string reg = p.Status == ParcelStore.Registered ? " selected" : "";
            string del = p.Status == ParcelStore.Delivered ? " selected" : "";

            var sb = new StringBuilder();
            sb.Append($"<h1>Редактирование посылки №{p.Id}</h1>");
            sb.Append("<p id=\"msg\" style=\"color:red\"></p>");
            sb.Append("<form id=\"f\">");
            sb.Append($"Получатель: <input name=\"recipient\" value=\"{E(p.Recipient)}\" required minlength=\"2\" maxlength=\"60\"><br>");
            sb.Append($"Вес (г): <input name=\"weight\" type=\"number\" value=\"{p.Weight}\" required><br>");
            sb.Append("Статус: <select name=\"status\">");
            sb.Append($"<option value=\"registered\"{reg}>registered</option>");
            sb.Append($"<option value=\"delivered\"{del}>delivered</option></select><br>");
            sb.Append("<button type=\"submit\">Сохранить</button></form>");
            sb.Append($"<p><a href=\"{L}/details?id={p.Id}\">Отмена</a></p>");

            sb.Append("<script>");
            sb.Append("document.getElementById('f').addEventListener('submit', async function (e) {");
            sb.Append("  e.preventDefault();");
            sb.Append("  var data = new URLSearchParams(new FormData(e.target));");
            sb.Append($"  var r = await fetch('{L}/update?id={p.Id}', {{ method: 'PUT', body: data }});");
            sb.Append($"  if (r.status === 204) {{ location.href = '{L}/details?id={p.Id}'; return; }}");
            sb.Append("  var t = await r.text();");
            sb.Append("  var p = new DOMParser().parseFromString(t, 'text/html').querySelector('p');");
            sb.Append("  document.getElementById('msg').textContent = p ? p.textContent : ('Ошибка ' + r.status);");
            sb.Append("});");
            sb.Append("</script>");

            return Html("Редактирование", sb.ToString());
        }

       
        [Put("/update")]
        public IResponseResult Update(string id, string recipient, string weight, string status)
        {
            var r = Store.Update(id, recipient, weight, status);
            if (r.Status != OpStatus.Ok) return ErrorPage(r.Status, r.Error);
            return new ParcelNoContentResult();
        }
    }
}