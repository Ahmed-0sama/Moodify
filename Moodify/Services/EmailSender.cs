using Azure.Core;
using System.Net;
using System.Net.Mail;

namespace Moodify.Services
{
	public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _config;

		public EmailSender(IConfiguration config)
		{
			_config = config;
		}

		public async  Task SendEmailAsync(string toEmail, string subject, string message)
		{
			var smtpClient = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential(
					_config["EmailSettings:Username"],
					_config["EmailSettings:Password"]),
				EnableSsl = true,
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(_config["EmailSettings:From"]),
				Subject = subject,
				Body = message,
				IsBodyHtml = true,
			};

			mailMessage.To.Add(toEmail);

			await smtpClient.SendMailAsync(mailMessage);
		}
	}
}
