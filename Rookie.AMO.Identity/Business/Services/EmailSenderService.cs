using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Rookie.AMO.Identity.Business.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSenderService(IConfiguration config) {
            _config = config;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = _config.GetSection("ExternalProviders").GetSection("SendGrid").GetSection("ApiKey").Value;
            return Execute(apiKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("phucluan6052000@gmail.com", "Admin"),
                Subject = subject,
                PlainTextContent = message,
                
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            var res = client.SendEmailAsync(msg);

            return res;
        }
    }
}
