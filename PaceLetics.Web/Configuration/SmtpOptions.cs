namespace PaceLetics.Web.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public const string PasswordEnvironmentVariable = "PaceLeticsSmtpPw";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Sender { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new InvalidOperationException("Smtp:Host must be configured.");

        if (Port <= 0)
            throw new InvalidOperationException("Smtp:Port must be configured.");

        if (string.IsNullOrWhiteSpace(Sender))
            throw new InvalidOperationException("Smtp:Sender must be configured.");
    }
}
