namespace MiniHttpServer.HtmlTemplating;

internal abstract class Node { }

// кусок обычного текста/HTML
internal class TextNode : Node
{
    public string Text { get; }
    public TextNode(string text) => Text = text;
}

// подстановка ${...}
internal class VariableNode : Node
{
    public string Path { get; }
    public VariableNode(string path) => Path = path;
}

// $if(...) ... $else ... $endif
internal class IfNode : Node
{
    public string Condition { get; }
    public List<Node> ThenBranch { get; } = new();
    public List<Node> ElseBranch { get; } = new();
    public IfNode(string condition) => Condition = condition;
}

internal class ForeachNode : Node
{
    public string ItemName { get; }
    public string CollectionPath { get; }
    public List<Node> Body { get; } = new();

    public ForeachNode(string itemName, string collectionPath)
    {
        ItemName = itemName;
        CollectionPath = collectionPath;
    }
}