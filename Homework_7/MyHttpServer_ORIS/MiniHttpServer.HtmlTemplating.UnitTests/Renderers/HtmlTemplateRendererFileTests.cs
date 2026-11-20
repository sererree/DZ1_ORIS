using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.HtmlTemplating.UnitTests.Renderers;

[TestClass]
public class HtmlTemplateRendererFileTests
{
    // n. RenderFromFile
    [TestMethod]
    public void RenderFromFile_ValidTemplateFile_ReturnsRenderedHtml()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string inputPath = Path.GetTempFileName();
        File.WriteAllText(inputPath, "<h1>Hello, ${Name}!</h1>");
        var model = new { Name = "Regina" };

        try
        {
            // Act
            string result = renderer.RenderFromFile(inputPath, model);

            // Assert
            Assert.AreEqual("<h1>Hello, Regina!</h1>", result);
        }
        finally
        {
            if (File.Exists(inputPath))
                File.Delete(inputPath);
        }
    }

    // o. RenderToFile
    [TestMethod]
    public void RenderToFile_ValidTemplate_WritesRenderedHtmlToOutputFile()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string inputPath = Path.GetTempFileName();
        string outputPath = Path.GetTempFileName();
        File.WriteAllText(inputPath, "<p>${Message}</p>");
        var model = new { Message = "Done" };

        try
        {
            // Act
            string returned = renderer.RenderToFile(inputPath, outputPath, model);
            string fileContent = File.ReadAllText(outputPath);

            // Assert
            Assert.AreEqual("<p>Done</p>", returned);
            Assert.AreEqual("<p>Done</p>", fileContent);
        }
        finally
        {
            if (File.Exists(inputPath)) File.Delete(inputPath);
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }
}