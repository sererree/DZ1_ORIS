using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.HtmlTemplating.UnitTests.Renderers;

[TestClass]
public class HtmlTemplateRendererTests
{
    // 1. Подстановка одного свойства
    [TestMethod]
    public void RenderFromString_ValidProperty_ReplacesPlaceholder()
    {
        // Arrange
        IHtmlTemplateRenderer renderer = new HtmlTemplateRenderer();
        string template = "<h1>${Name}</h1>";
        var model = new { Name = "Alex" };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<h1>Alex</h1>", result);
    }

    // 2. Подстановка нескольких свойств
    [TestMethod]
    public void RenderFromString_MultipleProperties_ReplacesAllPlaceholders()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "<p>${FirstName} ${LastName}, age ${Age}</p>";
        var model = new { FirstName = "Ivan", LastName = "Petrov", Age = 20 };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<p>Ivan Petrov, age 20</p>", result);
    }

    // 3. Пустой шаблон
    [TestMethod]
    public void RenderFromString_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        var model = new { Name = "Alex" };

        // Act
        string result = renderer.RenderFromString(string.Empty, model);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    // 4. Условная конструкция $if (истинное условие)
    [TestMethod]
    public void RenderFromString_TrueCondition_RendersIfBlock()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(IsActive)<span>Online</span>$endif";
        var model = new { IsActive = true };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<span>Online</span>", result);
    }

    // 5. Цикл $foreach
    [TestMethod]
    public void RenderFromString_Collection_RendersAllItems()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "<ul>$foreach(var u in Users)<li>${u.Name}</li>$endfor</ul>";
        var model = new
        {
            Users = new[]
            {
                new { Name = "Anna" },
                new { Name = "Bob" }
            }
        };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<ul><li>Anna</li><li>Bob</li></ul>", result);
    }

    // c. Повторная подстановка одного свойства
    [TestMethod]
    public void RenderFromString_RepeatedProperty_ReplacesAllOccurrences()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "<p>${Name} и ещё раз ${Name}</p>";
        var model = new { Name = "Alex" };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<p>Alex и ещё раз Alex</p>", result);
    }

    // d. Обращение к вложенным свойствам
    [TestMethod]
    public void RenderFromString_NestedProperty_ReplacesPlaceholder()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "<p>${User.Address.City}</p>";
        var model = new { User = new { Address = new { City = "Kazan" } } };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<p>Kazan</p>", result);
    }

    // g. Условие $if при ложном значении
    [TestMethod]
    public void RenderFromString_FalseCondition_OmitsIfBlock()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "Start$if(IsAdmin)<span>Admin</span>$endifEnd";
        var model = new { IsAdmin = false };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("StartEnd", result);
    }

    // h. Конструкция $if/$else (проверим оба варианта: true и false)
    [TestMethod]
    public void RenderFromString_TrueConditionWithElse_RendersThenBlock()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(IsOnline)Online$elseOffline$endif";
        var model = new { IsOnline = true };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("Online", result);
    }

    [TestMethod]
    public void RenderFromString_FalseConditionWithElse_RendersElseBlock()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(IsOnline)Online$elseOffline$endif";
        var model = new { IsOnline = false };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("Offline", result);
    }

    // j. Цикл $foreach с пустой коллекцией
    [TestMethod]
    public void RenderFromString_EmptyCollection_ReturnsEmptyLoopBody()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "<ul>$foreach(var u in Users)<li>${u.Name}</li>$endfor</ul>";
        var model = new { Users = Array.Empty<object>() };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<ul></ul>", result);
    }

    // k. Вложенные условные конструкции
    [TestMethod]
    public void RenderFromString_NestedIf_RendersInnerBlockWhenBothTrue()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(IsAuth)$if(IsAdmin)Panel$endif$endif";
        var model = new { IsAuth = true, IsAdmin = true };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("Panel", result);
    }

    // l. Вложенные циклы
    [TestMethod]
    public void RenderFromString_NestedForeach_RendersAllNestedItems()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$foreach(var g in Groups)[$foreach(var i in g.Items)${i.Title}$endfor]$endfor";
        var model = new
        {
            Groups = new[]
            {
                new { Items = new[] { new { Title = "A" }, new { Title = "B" } } },
                new { Items = new[] { new { Title = "C" } } }
            }
        };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("[AB][C]", result);
    }

    // m. Совместная работа $if и $foreach
    [TestMethod]
    public void RenderFromString_IfInsideForeach_RendersOnlyMatchingItems()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$foreach(var u in Users)$if(u.IsActive)<span>${u.Name}</span>$endif$endfor";
        var model = new
        {
            Users = new[]
            {
                new { Name = "Anna", IsActive = true },
                new { Name = "Bob", IsActive = false },
                new { Name = "Kate", IsActive = true }
            }
        };

        // Act
        string result = renderer.RenderFromString(template, model);

        // Assert
        Assert.AreEqual("<span>Anna</span><span>Kate</span>", result);
    }

    // p. Некорректные конструкции шаблонов
    [TestMethod]
    public void RenderFromString_UnclosedIf_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string brokenTemplate = "$if(IsActive)<span>Hi</span>";
        var model = new { IsActive = true };

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            renderer.RenderFromString(brokenTemplate, model));
    }

    [TestMethod]
    public void RenderFromString_ElseWithoutIf_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string brokenTemplate = "<div>$else</div>";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            renderer.RenderFromString(brokenTemplate, new { }));
    }


    [TestMethod]
    public void RenderFromString_UnclosedForeach_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$foreach(var user in Users)<p>${user.Name}</p>";
        var model = new { Users = new[] { new { Name = "Alex" } } };

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString(template, model));
    }

    [TestMethod]
    public void RenderFromString_EndifWithoutIf_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString("$endif", new { }));
    }

    [TestMethod]
    public void RenderFromString_EndforWithoutForeach_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString("$endfor", new { }));
    }

    [TestMethod]
    public void RenderFromString_SecondElse_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(Show)First$elseSecond$elseThird$endif";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString(template, new { Show = true }));
    }

    [TestMethod]
    public void RenderFromString_InvalidForeachSyntax_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$foreach(user in Users)Hello$endfor";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString(template, new { }));
    }

    [TestMethod]
    public void RenderFromString_InvalidIfSyntax_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if Show Hello$endif";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => renderer.RenderFromString(template, new { }));
    }

    [TestMethod]
    public void RenderFromString_EndifBeforeEndfor_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$if(Show)$foreach(var user in Users)Hello$endif$endfor";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            renderer.RenderFromString(template, new { Show = true, Users = Array.Empty<object>() }));
    }

    [TestMethod]
    public void RenderFromString_EndforBeforeEndif_ThrowsInvalidOperationException()
    {
        // Arrange
        var renderer = new HtmlTemplateRenderer();
        string template = "$foreach(var user in Users)$if(Show)Hello$endfor$endif";

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            renderer.RenderFromString(template, new { Show = true, Users = Array.Empty<object>() }));
    }

}