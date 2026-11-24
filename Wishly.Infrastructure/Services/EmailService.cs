using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Wishly.Infrastructure.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        var nomExpediteur = _config["EmailSettings:SenderName"] ?? "Wishly";
        var expediteur = _config["EmailSettings:SenderEmail"] ?? "";
        var serveur = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var port = int.TryParse(_config["EmailSettings:SmtpPort"], out var p) ? p : 587;
        var motDePasse = _config["EmailSettings:AppPassword"] ?? "";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(nomExpediteur, expediteur));
        message.To.Add(new MailboxAddress("", destinataire));
        message.Subject = sujet;
        message.Body = new TextPart("html") { Text = corpsHtml };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(serveur, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(expediteur, motDePasse);
            await client.SendAsync(message);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}