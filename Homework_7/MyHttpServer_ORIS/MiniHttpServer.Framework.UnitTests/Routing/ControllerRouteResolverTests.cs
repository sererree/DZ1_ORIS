using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework.UnitTests.Routing;

[TestClass]
public class ControllerRouteResolverTests
{
    [TestMethod]
    public void FindAction_GetRoute_ReturnsGetMethod()
    {
        // Arrange
        Type controller = typeof(SampleController);

        // Act
        var method = ControllerRouteResolver.FindAction(controller, "GET", "/users/list");

        // Assert
        Assert.IsNotNull(method);
        Assert.AreEqual(nameof(SampleController.List), method.Name);
    }

    [TestMethod]
    public void FindAction_PostRoute_ReturnsPostMethod()
    {
        // Arrange
        Type controller = typeof(SampleController);

        // Act
        var method = ControllerRouteResolver.FindAction(controller, "POST", "/users/create");

        // Assert
        Assert.IsNotNull(method);
        Assert.AreEqual(nameof(SampleController.Create), method.Name);
    }

    [TestMethod]
    public void FindAction_WrongHttpMethod_ReturnsNull()
    {
        // Arrange
        Type controller = typeof(SampleController);

        // Act
        var method = ControllerRouteResolver.FindAction(controller, "DELETE", "/users/list");

        // Assert
        Assert.IsNull(method);
    }

    [TestMethod]
    public void FindAction_WithBasePath_ResolvesControllerRoute()
    {
        // Arrange
        Type controller = typeof(SampleController);

        // Act
        var method = ControllerRouteResolver.FindAction(controller, "GET", "/api/users/list", "/api");

        // Assert
        Assert.IsNotNull(method);
        Assert.AreEqual(nameof(SampleController.List), method.Name);
    }

    [HttpController("users")]
    private sealed class SampleController
    {
        [Get("list")] public void List() { }
        [Post("create")] public void Create() { }
    }
}
