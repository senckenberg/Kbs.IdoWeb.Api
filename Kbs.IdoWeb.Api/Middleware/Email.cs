using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Kbs.IdoWeb.Api.Middleware
{
	public class EMail
	{
		private static string _mailingTemplatePath = "~/EmailTemplate";
		private static IConfiguration _smtpConfig;

		public static void CreateAndSendMailMessage(MailAddress from, MailAddress to, string subject, string htmlBody, string plainBody, List<Attachment> attachements, bool useHtmlTemplate, bool sharedMail)
		{
			MailMessage mailMessage = new MailMessage();
			if (from != null)
			{
				mailMessage.From = from;
			} else
            {
				mailMessage.From = new MailAddress(_smtpConfig.GetValue("ApplicationSettings:Smtp:FromAddress", "defaultfromaddress"));
			}
			mailMessage.To.Add(to);
			mailMessage.Subject = subject;
			mailMessage.Body = plainBody;
			mailMessage.IsBodyHtml = true;
			if (!string.IsNullOrEmpty(htmlBody))
			{
				List<LinkedResource> list = new List<LinkedResource>();
				if (useHtmlTemplate)
				{
					StreamReader streamReader = new StreamReader(File.OpenRead(Path.Combine(EMail._mailingTemplatePath, sharedMail ? "StandardShared.htm" : "Standard.htm")), Encoding.UTF8);
					string text = streamReader.ReadToEnd();
					streamReader.Close();
					streamReader.Dispose();
					htmlBody = text.Replace("{Content}", htmlBody).Replace("{Subject}", mailMessage.Subject);
					MatchCollection matchCollection = Regex.Matches(text, " cid:(?<linkedRessource>.*?)[\"|)] ", RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
					foreach (Match match in matchCollection)
					{
						string value = match.Groups["linkedRessource"].Value;
						list.Add(new LinkedResource(Path.Combine(EMail._mailingTemplatePath, value), "image/" + Path.GetExtension(value))
						{
							ContentId = value,
							TransferEncoding = TransferEncoding.Base64
						});
					}
				}
				AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody);
				alternateView.ContentType = new ContentType("text/html");
				foreach (LinkedResource current in list)
				{
					alternateView.LinkedResources.Add(current);
				}
				mailMessage.AlternateViews.Add(alternateView);
			}
			if (attachements != null && attachements.Count > 0)
			{
				foreach (Attachment current2 in attachements)
				{
					mailMessage.Attachments.Add(current2);
				}
			}
			EMail.SendMail(mailMessage);
		}

		public static void SendMail(object mailMessage)
		{
			try
			{
				string host = _smtpConfig.GetValue<string>("ApplicationSettings:Smtp:Server", "defaultmailserver");
				int port = _smtpConfig.GetValue<int>("ApplicationSettings:Smtp:Port", 25);
				string fromAddress = _smtpConfig.GetValue<string>("ApplicationSettings:Smtp:FromAddress", "defaultfromaddress");
				string pw = _smtpConfig.GetValue<string>("ApplicationSettings:Smtp:Password", "defaultpassword");
				var basicCredential = new NetworkCredential(fromAddress, pw);

				SmtpClient smtpClient = new SmtpClient(host, port);
				smtpClient.UseDefaultCredentials = false;
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtpClient.EnableSsl = true;
				smtpClient.Credentials = basicCredential;
				smtpClient.Send((MailMessage)mailMessage);
				Trace.WriteLine("SendMail:", string.Concat(new object[]
				{
					((MailMessage)mailMessage).To,
					"->",
					((MailMessage)mailMessage).Subject,
					Environment.NewLine
				}));
			}
			catch (Exception ex)
			{
				Trace.WriteLine("SendmailException: ", string.Concat(new object[]
				{
					ex.Message,
					" : ",
					((MailMessage)mailMessage).To,
					"->",
					((MailMessage)mailMessage).Subject,
					Environment.NewLine
				}));
			}
		}

		public static void SendFeedbackMail(string email, string text, Microsoft.Extensions.Configuration.IConfiguration smtpConfig)
		{
			_smtpConfig = smtpConfig;
			string str = email ?? "keine Angabe";
			string htmlBody = text + "<br /> Absender: " + str;
			string plainBody = text + "<br /> Absender: " + str;
			string subject = "Feedback zur App";
			string mailTo = _smtpConfig.GetValue<string>("ApplicationSettings:Smtp:FromAddress");
			if(!String.IsNullOrEmpty(mailTo))
            {
				EMail.CreateAndSendMailMessage(null, new MailAddress(mailTo), subject, htmlBody, plainBody, null, false, false);
			}
		}

		public static void SendResetMail(string userEmail, string token, Microsoft.Extensions.Configuration.IConfiguration smtpConfig)
		{
			_smtpConfig = smtpConfig;
			string htmlBody = "Hallo,<br/>Ihr Passwort kann mit folgendem Token geändert werden:<br/><br/>"+ token + "<br/><br/>Um das Passwort zu ändern kopieren Sie bitte das Token in das Feld 'Token' auf der Website und vergeben Sie ein neues Passwort. Bei Problemen wenden Sie sich bitte an den Administrator von <a href='https://www.bodentierhochvier.de'>BodentierHoch4</a> oder antworten Sie direkt auf diese e-Mail.<br/><br/>Viel Erfolg!<br/>";
			string plainBody = "Hallo, Ihr Passwort kann mit folgendem Token geändert werden: " + token + "  Um das Passwort zu ändern kopieren Sie bitte das Token in das Feld 'Token' auf der Website und vergeben Sie ein neues Passwort. Bei Problemen wenden Sie sich bitte an den Administrator von <a href='https://www.bodentierhochvier.de'>BodentierHoch4</a> oder antworten Sie direkt auf diese e-Mail. Viel Erfolg!";
			string subject = "BodentierHoch4: Passwort Zurücksetzen";
			string mailTo = userEmail;
			if (!String.IsNullOrEmpty(mailTo))
            {
				EMail.CreateAndSendMailMessage(null, new MailAddress(mailTo), subject, htmlBody, plainBody, null, false, false);
			}
		}
	}
}
