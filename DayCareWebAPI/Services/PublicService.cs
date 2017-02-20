using DayCareWebAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using DayCareWebAPI.Models;
using System.Net;

namespace DayCareWebAPI.Services
{
    public class PublicService
    {
        private readonly DayCareRepository _repo;
        public PublicService()
        {
            this._repo = new DayCareRepository();
        }

        public string SendContactEmail(PublicContact contact)
        {
            LogPublicContact(contact);
            var subject = contact.FirstName + "," + contact.LastName + " is interedted in knowing more about your daycare";
            var header = string.Format(Constants.HeaderForEmail, subject);
            var body = "<br/>";
            body = body + "<br/>Comments from " + contact.FirstName + "," + contact.LastName + ": " + contact.Comments + "<br/>";
            body = body + "Email: " + contact.Email + "<br/>Phone: " + contact.Phone + "<br/>";
            var footer = "<a href=\"" + Constants.LoginUrl + "\">LOGIN HERE!</a> | <a href=\"" + Constants.WebUrl + "\">VISIT US!</a>";
            var finalFooter = "From<br/>GIGGLES! TEAM<br/>Visit Us @ " + Constants.WebUrl;
            var dayServ = new DayCareService();
            var finalBody = dayServ.BuildEmailTemplate(header, string.Format(Constants.ParaForEmail, body.ToString()), footer, finalFooter);
            var mailMessage = new MailMessage(contact.Email, contact.To, subject, finalBody)
            { IsBodyHtml = true };
            var client = new SmtpClient(Constants.GmailSmtpServer, Constants.GmailSmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Constants.GmailSmtpUserId, Constants.GmailSmtpPassword),
                EnableSsl = true
            };
            client.Send(mailMessage);
            return string.Empty;
        }

        public string LogPublicContact(PublicContact contact)
        {
            return _repo.InsertPublicContact(contact);
        }
    }
}