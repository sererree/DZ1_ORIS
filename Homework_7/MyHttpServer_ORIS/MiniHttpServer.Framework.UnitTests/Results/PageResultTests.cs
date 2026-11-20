using System.Net;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.Framework.UnitTests.Results;

[TestClass]
public class PageResultTests
{
    [TestMethod]
    public void RenderTemplate_UsesInjectedRendererAndModel()
    {
        // Arrange
        string templatePath = Path.GetTempFileName();
        var model = new { Name = "Alex" };
        var renderer = new StubTemplateRenderer("<h1>Alex</h1>");
        var page = new PageResult("unused.html", model, renderer);

        try
        {
            // Act
            string result = page.RenderTemplate(templatePath);

            // Assert
            Assert.AreEqual("<h1>Alex</h1>", result);
            Assert.AreEqual(templatePath, renderer.ReceivedPath);
            Assert.AreSame(model, renderer.ReceivedModel);
        }
        finally
        {
            File.Delete(templatePath);
        }
    }

    private sealed class StubTemplateRenderer(string result) : IHtmlTemplateRenderer
    {
        public string? ReceivedPath { get; private set; }
        public object? ReceivedModel { get; private set; }
        public string RenderFromString(string htmlTemplate, object dataModel) => result;
        public string RenderFromFile(string filePath, object dataModel)
        {
            ReceivedPath = filePath;
            ReceivedModel = dataModel;
            return result;
        }
        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel) => result;
    }
}
