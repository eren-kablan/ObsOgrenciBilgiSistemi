using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

public interface IEmailService
{
    // Servisin uygulamaya sunduğu e-posta işlemleri
    Task SendActivationEmailAsync(string toEmail, string studentName, string activationToken);
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    // SMTP ayarları ve hata günlüğü bağımlılıkları
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Genel e-posta gönderimi
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Sunucu, port ve gönderici bilgileri appsettings dosyasından alınır.
        var smtpSection = _configuration.GetSection("SmtpSettings");
        var host = smtpSection["Server"];
        var senderEmail = smtpSection["SenderEmail"];
        var senderName = smtpSection["SenderName"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(senderEmail) ||
            !int.TryParse(smtpSection["Port"], out var port))
            throw new InvalidOperationException("SMTP ayarları eksik veya geçersiz.");

        // İçerik HTML olarak hazırlanır ve alıcı mesaja eklenir.
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            IsBodyHtml = true,
            Body = body
        };

        mailMessage.To.Add(toEmail);

        // SMTP bağlantısı işlem sonunda otomatik olarak kapatılır.
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = bool.TryParse(smtpSection["EnableSsl"], out var enableSsl) && enableSsl
        };

        // Kimlik bilgileri tanımlıysa SMTP sunucusuna doğrulamalı bağlanılır.
        if (!string.IsNullOrEmpty(smtpSection["Username"]))
        {
            client.Credentials = new NetworkCredential(smtpSection["Username"], smtpSection["Password"]);
        }

        // Gönderim hataları loglanır, kullanıcıya teknik detay gösterilmez.
        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP e-postası gönderilemedi. Alıcı: {Recipient}, Konu: {Subject}", toEmail, subject);
            throw new InvalidOperationException("E-posta şu anda gönderilemedi. Lütfen daha sonra tekrar deneyin.", ex);
        }
    }

    // Öğrenci aktivasyon e-postası
    public async Task SendActivationEmailAsync(string toEmail, string studentName, string activationToken)
    {
        // Token URL içinde güvenli taşınacak biçime çevrilir
        var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
        var activationLink = $"{frontendUrl.TrimEnd('/')}/aktivasyon?token={Uri.EscapeDataString(activationToken)}";

        // Aktivasyon bağlantısını içeren HTML e-posta şablonu
        string body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8fafc;'>
                <div style='max-width: 500px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 12px; border: 1px solid #e2e8f0;'>
                    <h2 style='color: #1d4ed8; margin-top: 0;'>Hoş Geldiniz, {WebUtility.HtmlEncode(studentName)}</h2>
                    <p style='color: #475569;'>Öğrenci Bilgi Sistemi hesabınız admin tarafından oluşturulmuştur.</p>
                    <p style='color: #475569;'>Sisteme giriş yapabilmek için lütfen aşağıdaki butona tıklayarak şifrenizi belirleyin:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{activationLink}' style='background-color: #2563eb; color: #ffffff; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;'>Şifre Oluştur ve Aktif Et</a>
                    </div>
                    <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Bu bağlantı 24 saat geçerlidir. Bu e-posta otomatik olarak gönderilmiştir.</p>
                </div>
            </div>";

        // Hazırlanan şablon ortak gönderim metodu üzerinden iletilir
        await SendEmailAsync(toEmail, "Düzce Üniversitesi OBS - Hesap Aktivasyonu", body);
    }
}

