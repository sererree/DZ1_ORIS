using System.Text;
using System.Text.Json;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.UnitTests.Results;

[TestClass]
public class HttpResultPayloadTests
{
    [TestMethod]
    public void JsonResult_SerializesObjectToJson()
    {
        // Arrange
        var result = new JsonResult(new { Name = "Alex", Age = 20 });

        // Act
        string json = result.Serialize();
        using JsonDocument document = JsonDocument.Parse(json);

        // Assert
        Assert.AreEqual("Alex", document.RootElement.GetProperty("Name").GetString());
        Assert.AreEqual(20, document.RootElement.GetProperty("Age").GetInt32());
    }

    [TestMethod]
    public void HtmlResult_GetPayload_ReturnsUtf8HtmlBytes()
    {
        // Arrange
        var result = new HtmlResult("<h1>Привет</h1>");

        // Act
        byte[] payload = result.GetPayload();

        // Assert
        Assert.AreEqual("<h1>Привет</h1>", Encoding.UTF8.GetString(payload));
    }
}
