using System.Reflection;
using MyHttpServer.Framework.Attributes;

namespace MiniHttpServer.Framework.UnitTests.Attributes;

[TestClass]
public class HttpAttributeTests
{
    [TestMethod]
    public void HttpControllerAttribute_StoresRoute()
    {
        // Arrange
        var attribute = new HttpControllerAttribute("users");

        // Act
        string route = attribute.Route;

        // Assert
        Assert.AreEqual("users", route);
    }

    [TestMethod]
    public void GetAttribute_StoresRoute()
    {
        // Arrange
        var attribute = new GetAttribute("list");

        // Act
        string route = attribute.Route;

        // Assert
        Assert.AreEqual("list", route);
    }

    [TestMethod]
    public void PostAttribute_StoresRoute()
    {
        // Arrange
        var attribute = new PostAttribute("create");

        // Act
        string route = attribute.Route;

        // Assert
        Assert.AreEqual("create", route);
    }

    [TestMethod]
    public void ControllerMethod_HasExpectedHttpMethodAttribute()
    {
        // Arrange
        MethodInfo getMethod = typeof(SampleController).GetMethod(nameof(SampleController.Get))!;
        MethodInfo postMethod = typeof(SampleController).GetMethod(nameof(SampleController.Post))!;

        // Act
        var getAttribute = getMethod.GetCustomAttribute<GetAttribute>();
        var postAttribute = postMethod.GetCustomAttribute<PostAttribute>();

        // Assert
        Assert.IsNotNull(getAttribute);
        Assert.AreEqual("items", getAttribute.Route);
        Assert.IsNotNull(postAttribute);
        Assert.AreEqual("items", postAttribute.Route);
    }

    [HttpController("sample")]
    private sealed class SampleController
    {
        [Get("items")] public void Get() { }
        [Post("items")] public void Post() { }
    }
}
