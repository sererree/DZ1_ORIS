using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.UnitTests.Content;

[TestClass]
public class MimeTypeProviderTests
{
    [TestMethod]
    public void GetContentType_HtmlExtension_ReturnsTextHtml()
    {
        // Arrange
        string file = "index.html";

        // Act
        string result = MimeTypeProvider.GetContentType(file);

        // Assert
        Assert.AreEqual("text/html; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_CssExtension_ReturnsTextCss()
    {
        // Arrange & Act
        string result = MimeTypeProvider.GetContentType("styles/main.css");

        // Assert
        Assert.AreEqual("text/css; charset=utf-8", result);
    }

    [TestMethod]
    public void GetContentType_UpperCaseExtension_IsCaseInsensitive()
    {
        // Arrange & Act
        string result = MimeTypeProvider.GetContentType("PHOTO.PNG");

        // Assert
        Assert.AreEqual("image/png", result);
    }

    [TestMethod]
    public void GetContentType_UnknownExtension_ReturnsOctetStream()
    {
        // Arrange & Act
        string result = MimeTypeProvider.GetContentType("archive.xyz");

        // Assert
        Assert.AreEqual("application/octet-stream", result);
    }
}