using Azure.Communication.Email;
using Azure.Communication.Email.Models;
using VRefSolutions.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VRefSolutions.Service
{
    public class EmailService : IEmailService
    {
        public bool sendEmail(string emailAddress, string subject, string content)
        {
            try{
                var connectionString = Environment.GetEnvironmentVariable("ConnectionEmail");
                EmailClient emailClient = new EmailClient(connectionString);
                EmailContent emailContent = new EmailContent(subject);
                emailContent.PlainText = content;
                List<EmailAddress> emailAddresses = new List<EmailAddress> { new EmailAddress(emailAddress) { DisplayName = "Friendly Display Name" } };
                EmailRecipients emailRecipients = new EmailRecipients(emailAddresses);
                EmailMessage emailMessage = new EmailMessage(Environment.GetEnvironmentVariable("EmailSender"), emailContent, emailRecipients);
                SendEmailResult emailResult = emailClient.Send(emailMessage, CancellationToken.None);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
    }
}
