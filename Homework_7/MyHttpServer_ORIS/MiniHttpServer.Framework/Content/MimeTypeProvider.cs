namespace MyHttpServer.Framework.Content;

public static class MimeTypeProvider
{
    public static string GetContentType(string filePathOrExtension)
    {
        string extension = Path.GetExtension(filePathOrExtension).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) && filePathOrExtension.StartsWith('.'))
        {
            extension = filePathOrExtension.ToLowerInvariant();
        }

        return extension switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            _ => "application/octet-stream"
        };
    }
}