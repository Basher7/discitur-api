using AngulaDemo;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;

namespace Mag14.Providers
{
    public class RegistrationMailDef 
    {
        public string MailTemplate { get; set; }
        public ListDictionary Replacements { get; set; }
        private MailDefinition md { get; set; }
        private RegistrationMailConfig config { get; set; }

        public MailMessage CreateMailMessage(string strTo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath(MailTemplate)))
            {
                body = reader.ReadToEnd();
            }

            return md.CreateMailMessage(strTo, Replacements, body, new System.Web.UI.Control());
        }

        public RegistrationMailDef(ListDictionary replacements)
            : base()
        {
            config = (RegistrationMailConfig)RegistrationMailConfig.GetConfiguration();
            MailTemplate = config.Template;
            Replacements = replacements;
            md = new MailDefinition();
            md.IsBodyHtml = true;
            md.Subject = config.Subject;
            md.From = config.From;
        }
    }
}