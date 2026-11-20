using System.Collections;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniHttpServer.HtmlTemplating;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        List<Node> tree = Parse(htmlTemplate);
        return Render(tree, dataModel, new Dictionary<string, object?>());
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        string template = File.ReadAllText(filePath);
        return RenderFromString(template, dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        string result = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, result);
        return result;
    }

    // ---------- 1. Токены ----------
    private static readonly Regex TokenRegex = new(
        @"\$\{(?<var>[\w.]+)\}" +
        @"|\$if\((?<if>[\w.]+)\)" +
        @"|(?<else>\$else)" +
        @"|(?<endif>\$endif)" +
        @"|\$foreach\(\s*var\s+(?<item>\w+)\s+in\s+(?<coll>[\w.]+)\s*\)" +
        @"|(?<endfor>\$endfor)" +
        @"|(?<badfor>\$foreach)" +
        @"|(?<badif>\$if)");

    // ---------- 2. Строим дерево ----------
    private static List<Node> Parse(string template)
    {
        var root = new List<Node>();
        var open = new Stack<Node>();            // открытые блоки ($if и $foreach)
        var target = new Stack<List<Node>>();    // куда сейчас складываем узлы
        target.Push(root);

        int pos = 0;

        foreach (Match m in TokenRegex.Matches(template))
        {
            if (m.Index > pos)
                target.Peek().Add(new TextNode(template.Substring(pos, m.Index - pos)));
            pos = m.Index + m.Length;

            if (m.Groups["var"].Success)
            {
                target.Peek().Add(new VariableNode(m.Groups["var"].Value));
            }
            else if (m.Groups["if"].Success)
            {
                var node = new IfNode(m.Groups["if"].Value);
                target.Peek().Add(node);
                open.Push(node);
                target.Push(node.ThenBranch);
            }
            else if (m.Groups["else"].Success)
            {
                IfNode? ifNode = open.Count > 0 ? open.Peek() as IfNode : null;
                if (ifNode == null)
                    throw new InvalidOperationException("Найден $else вне блока $if");
                if (target.Peek() == ifNode.ElseBranch)
                    throw new InvalidOperationException($"Второй $else в блоке $if({ifNode.Condition})");

                target.Pop();
                target.Push(ifNode.ElseBranch);
            }
            else if (m.Groups["endif"].Success)
            {
                if (open.Count == 0)
                    throw new InvalidOperationException("Найден $endif без $if");
                if (open.Peek() is not IfNode)
                    throw new InvalidOperationException("Найден $endif, но сначала нужно закрыть $foreach через $endfor");

                open.Pop();
                target.Pop();
            }
            else if (m.Groups["item"].Success)
            {
                var node = new ForeachNode(m.Groups["item"].Value, m.Groups["coll"].Value);
                target.Peek().Add(node);
                open.Push(node);
                target.Push(node.Body);
            }
            else if (m.Groups["endfor"].Success)
            {
                if (open.Count == 0)
                    throw new InvalidOperationException("Найден $endfor без $foreach");
                if (open.Peek() is not ForeachNode)
                    throw new InvalidOperationException("Найден $endfor, но сначала нужно закрыть $if через $endif");

                open.Pop();
                target.Pop();
            }
            else if (m.Groups["badfor"].Success)
            {
                throw new InvalidOperationException(
                    "Некорректно задана конструкция $foreach. Ожидается: $foreach(var item in Items)");
            }
            else if (m.Groups["badif"].Success)
            {
                throw new InvalidOperationException(
                    "Некорректно задана конструкция $if. Ожидается: $if(Property)");
            }
        }

        if (pos < template.Length)
            target.Peek().Add(new TextNode(template.Substring(pos)));

        if (open.Count > 0)
        {
            Node unclosed = open.Peek();
            if (unclosed is IfNode u)
                throw new InvalidOperationException($"Отсутствует $endif для $if({u.Condition})");
            if (unclosed is ForeachNode f)
                throw new InvalidOperationException($"Отсутствует $endfor для $foreach(var {f.ItemName} in {f.CollectionPath})");
        }

        return root;
    }

    // ---------- 3. Обходим дерево ----------
    private static string Render(List<Node> nodes, object model, Dictionary<string, object?> locals)
    {
        var sb = new StringBuilder();

        foreach (Node node in nodes)
        {
            switch (node)
            {
                case TextNode t:
                    sb.Append(t.Text);
                    break;

                case VariableNode v:
                    object? value = GetValue(model, locals, v.Path);
                    sb.Append(WebUtility.HtmlEncode(value?.ToString() ?? ""));
                    break;

                case IfNode i:
                    bool condition = GetValue(model, locals, i.Condition) is true;
                    sb.Append(Render(condition ? i.ThenBranch : i.ElseBranch, model, locals));
                    break;

                case ForeachNode f:
                    object? collection = GetValue(model, locals, f.CollectionPath);
                    if (collection == null)
                        break; // пустое значение считаем пустым списком

                    if (collection is string || collection is not IEnumerable)
                        throw new InvalidOperationException(
                            $"'{f.CollectionPath}' не является коллекцией, по ней нельзя выполнить $foreach");

                    foreach (object? item in (IEnumerable)collection)
                    {
                        // копия переменных + текущий элемент под именем из шаблона
                        var inner = new Dictionary<string, object?>(locals) { [f.ItemName] = item };
                        sb.Append(Render(f.Body, model, inner));
                    }
                    break;
            }
        }

        return sb.ToString();
    }

    // ---------- Поиск значения по пути ----------
    private static object? GetValue(object model, Dictionary<string, object?> locals, string path)
    {
        string[] parts = path.Split('.');
        object? current;
        int start;

        if (locals.TryGetValue(parts[0], out object? local))
        {
            current = local;   // первая часть пути — переменная цикла
            start = 1;
        }
        else
        {
            current = model;   // иначе ищем свойство у модели
            start = 0;
        }

        for (int i = start; i < parts.Length; i++)
        {
            if (current == null)
                return null;

            Type type = current.GetType();
            PropertyInfo? property = type.GetProperty(parts[i]);

            if (property == null)
                throw new InvalidOperationException(
                    $"Свойство '{parts[i]}' не найдено у типа '{type.Name}' (выражение '{path}')");

            current = property.GetValue(current);
        }

        return current;
    }
}