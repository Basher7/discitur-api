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

        public static MailProvider GetMailprovider() {
            MailProvider mp = new MailProvider();
            smtpConfig = (SmtpConfig)SmtpConfig.GetConfiguration();
            return mp;
        }

        public async Task<bool> SendForgottenPwdEmail(string strTo, string newPassword)
        {
            string strBody = "<p>Bentornato,<br>la tua nuova password è:<b>" + newPassword + "</b> </p><p>Ti invitiamo a modificarla al primo accesso dalla pagina del tuo Profilo.<br><br>Discitur: <b>insieme</b> si migliora!</p>";
            return await this.SendEmail(smtpConfig.From, strTo, smtpConfig.Subject, strBody, null, true);
        }


        public async Task<bool> SendEmail(String strFrom, string strTo, string strSubject, string strBody, string strAttachmentPath, bool IsBodyHTML)
        {
            //SmtpConfig smtpConfig = (SmtpConfig)SmtpConfig.GetConfiguration();
            MailMessage mm = new MailMessage();
            mm.From = new MailAddress(strFrom);
            mm.Subject = strSubject;
            mm.To.Add(new MailAddress(strTo));
            mm.Body = strBody;
            mm.IsBodyHtml = IsBodyHTML;

            Array arrToArray;
            char[] splitter = { ';' };
            arrToArray = strTo.Split(splitter);

            foreach (string s in arrToArray)
            {
                mm.To.Add(new MailAddress(s));
            }
            if (strAttachmentPath!= null && strAttachmentPath != "")
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