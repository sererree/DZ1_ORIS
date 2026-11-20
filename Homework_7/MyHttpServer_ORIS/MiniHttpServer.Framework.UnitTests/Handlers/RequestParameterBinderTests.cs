using System.Collections.Specialized;
using System.Reflection;
using MyHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework.UnitTests.Handlers;

[TestClass]
public class RequestParameterBinderTests
{
    [TestMethod]
    public void Bind_UsesQueryParameters()
    {
        // Arrange
        ParameterInfo[] parameters = typeof(SampleActions).GetMethod(nameof(SampleActions.Search))!.GetParameters();
        var query = new NameValueCollection { ["page"] = "3", ["term"] = "books" };

        // Act
        object?[] result = RequestParameterBinder.Bind(parameters, query, new NameValueCollection());

        // Assert
        Assert.AreEqual(3, result[0]);
        Assert.AreEqual("books", result[1]);
    }

    [TestMethod]
    public void Bind_BodyParametersOverrideQueryParameters()
    {
        // Arrange
        ParameterInfo[] parameters = typeof(SampleActions).GetMethod(nameof(SampleActions.Login))!.GetParameters();
        var query = new NameValueCollection { ["login"] = "query-user", ["password"] = "query-pass" };
        var body = new NameValueCollection { ["login"] = "body-user", ["password"] = "body-pass" };

        // Act
        object?[] result = RequestParameterBinder.Bind(parameters, query, body);

        // Assert
        Assert.AreEqual("body-user", result[0]);
        Assert.AreEqual("body-pass", result[1]);
    }

    [TestMethod]
    public void Bind_UsesEmailAsLoginFallback()
    {
        // Arrange
        ParameterInfo[] parameters = typeof(SampleActions).GetMethod(nameof(SampleActions.Login))!.GetParameters();
        var query = new NameValueCollection { ["email"] = "person@example.com", ["password"] = "secret" };

        // Act
        object?[] result = RequestParameterBinder.Bind(parameters, query, new NameValueCollection());

        // Assert
        Assert.AreEqual("person@example.com", result[0]);
    }

    [TestMethod]
    public void Bind_InvalidNumber_ThrowsFormatException()
    {
        // Arrange
        ParameterInfo[] parameters = typeof(SampleActions).GetMethod(nameof(SampleActions.Search))!.GetParameters();
        var query = new NameValueCollection { ["page"] = "not-a-number", ["term"] = "books" };

        // Act & Assert
        Assert.ThrowsExactly<FormatException>(() =>
            RequestParameterBinder.Bind(parameters, query, new NameValueCollection()));
    }

    private sealed class SampleActions
    {
        public void Search(int page, string term) { }
        public void Login(string login, string password) { }
    }
}
