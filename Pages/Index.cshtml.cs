using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;

public class IndexModel : PageModel
{
    public class ContactFormModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }
    }

    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string fromEmail, string fromName, string toEmail, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    // Replace these with your SMTP server details
                    await client.ConnectAsync("smtp.your-email-provider.com", 587, false);
                    await client.AuthenticateAsync("your-email@example.com", "your-email-password");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    return true;
                }
                catch (Exception ex)
                {
                    // Log the exception
                    _logger.LogError(ex, "An error occurred while sending the email.");
                    return false;
                }
            }
        }
    }

    private readonly EmailService _emailService;

    public IndexModel(ILogger<EmailService> logger)
    {
        _emailService = new EmailService(logger);
    }

    [BindProperty]
    public ContactFormModel Contact { get; set; }

    public bool EmailSent { get; set; }
    public bool EmailFailed { get; set; }

    public void OnGet()
    {
        // This method is intentionally left empty
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var emailSent = await _emailService.SendEmailAsync(
            Contact.Email,
            Contact.Name,
            "recipient@example.com", // Replace with the recipient's email
            "Contact Form Message",
            Contact.Message);

        if (emailSent)
        {
            EmailSent = true;
        }
        else
        {
            EmailFailed = true;
        }

        return Page();
    }
}
