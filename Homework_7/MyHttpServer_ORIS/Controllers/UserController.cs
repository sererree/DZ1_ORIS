using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using MyHttpServer_ORIS.Framework;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController : BaseController
{
    [Get("profile")]
public IHttpResult Profile()
{
    var user = new
    {
        Name = "Alex", Age = 20, Email = "alex@example.com", IsActive = true,
        Hobbies = new[] { "Футбол", "Шахматы", "Музыка" }
    };
    return Page("User/Profile.html", user);
}

[Get("list")]
public IHttpResult List()
{
    var model = new
    {
        Users = new[]
        {
            new { Name = "Alex", IsActive = true },
            new { Name = "Maria", IsActive = false },
            new { Name = "Ivan", IsActive = true }
        }
    };
    return Page("User/List.html", model);
}
    
}