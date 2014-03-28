using AngulaDemo;
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
        private static ForgottenPwdMailConfig forgottnenMailConfig;
        private static RegistrationMailConfig registrationMailConfig;

        public static MailProvider GetMailprovider() {
            MailProvider mp = new MailProvider();
            smtpConfig = (SmtpConfig)SmtpConfig.GetConfiguration();
            forgottnenMailConfig = (ForgottenPwdMailConfig)ForgottenPwdMailConfig.GetConfiguration();
            registrationMailConfig = (RegistrationMailConfig)RegistrationMailConfig.GetConfiguration();
            return mp;
        }

        public async Task<bool> SendRegistrationEmail(string strTo, string username, string newPassword)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();
            replacements.Add("<%Username%>", username);
            replacements.Add("<%Password%>", newPassword);

            RegistrationMailDef md = new RegistrationMailDef(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, registrationMailConfig.From, null);
        }

        public async Task<bool> SendForgottenPwdEmail(string strTo, string newPassword)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();
            replacements.Add("<%Password%>", newPassword);

            ForgottenPwdMailDef md = new ForgottenPwdMailDef(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, forgottnenMailConfig.From, null);
        }

        public async Task<bool> SendEmail(MailMessage mm, String strFrom, string strAttachmentPath)
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