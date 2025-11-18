namespace SharedProject.Models.Email;

public class EmailConfiguration
{
    public string To { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool SslConnection { get; set; }
}

