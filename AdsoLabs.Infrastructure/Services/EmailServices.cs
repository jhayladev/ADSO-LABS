using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Infrastructure.Services
{
    public class EmailServices : IEmailServices
    {

        private readonly IConfiguration _config;

        public EmailServices(IConfiguration config) 
        {
        
            _config = config;
        
        }

        public async Task EnviarAsync(string correoDestino, string asunto, string cuerpoHtml)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(
                _config["Email:NombreRemitente"],
                _config["Email:Usuario"]));
            mensaje.To.Add(MailboxAddress.Parse(correoDestino));
            mensaje.Subject = asunto;
            mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config["Email:Host"],
                int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["Email:Usuario"],
                _config["Email:Password"]);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
        }

    }
}
