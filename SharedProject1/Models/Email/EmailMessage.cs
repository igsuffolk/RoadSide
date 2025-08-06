
namespace SharedProject1.Models;

public class EmailMessage
{
    public List<string> To { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public List<string>? ReplyTo { get; set; }
    public string? From { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string? TemplateFile { get; set; }
    public List<KeyValuePair<string, string>> EmbeddedResourcesElementPaths { get; set; } = new();
    public List<string>? Attachments { get; set; }
    public bool Success { get; set; } = true;   

}
