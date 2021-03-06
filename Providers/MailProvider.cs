﻿using AngulaDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Mag14.Providers
{
    public class MailProvider
    {
        private static SmtpConfig smtpConfig;

        public static MailProvider GetMailprovider() {
            MailProvider mp = new MailProvider();
            smtpConfig = (SmtpConfig)SmtpConfig.GetConfiguration();
            return mp;
        }

        public async Task<bool> SendActivationEmail(string strTo, string username, string newPassword, string activationKey, string absoluteURL)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();

            ActivationMailConfig config = MailConfigProvider.GetConfiguration<ActivationMailConfig>();
            string fromURL = string.IsNullOrEmpty(config.ActivationURL) ? absoluteURL : config.ActivationURL;

            replacements.Add("<%Username%>", username);
            replacements.Add("<%Password%>", newPassword);
            replacements.Add("<%ActivationKey%>", activationKey);
            replacements.Add("<%ActivationUrl%>", fromURL);
            replacements.Add("<%ActivationPath%>", config.ActivationPath);

            DisciturMailDef<ActivationMailConfig> md = new DisciturMailDef<ActivationMailConfig>(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, config.From, null);
        }

        public async Task<bool> SendForgottenPwdEmail(string strTo, string newPassword)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();
            ForgottenPwdMailConfig config = MailConfigProvider.GetConfiguration<ForgottenPwdMailConfig>();
            replacements.Add("<%Password%>", newPassword);

            DisciturMailDef<ForgottenPwdMailConfig> md = new DisciturMailDef<ForgottenPwdMailConfig>(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, config.From, null);
        }

        private async Task<bool> SendEmail(MailMessage mm, String strFrom, string strAttachmentPath)
        {
            mm.From = new MailAddress(strFrom);

            if (strAttachmentPath != null && strAttachmentPath != "")
            {
                //Add Attachment
                Attachment attachFile = new Attachment(strAttachmentPath);
                mm.Attachments.Add(attachFile);
            }

            SmtpClient smtp = new SmtpClient();
            try
            {
                smtp.Host = smtpConfig.Host;
                smtp.EnableSsl = smtpConfig.EnableSsl; //Depending on server SSL Settings true/false
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = smtpConfig.UserName;
                NetworkCred.Password = smtpConfig.Password;
                //smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = smtpConfig.Port;//Specify your port No;
                await smtp.SendMailAsync(mm);
                return true;
            }
            catch
            {
                mm.Dispose();
                smtp = null;
                return false;
            }

        }

    }
}