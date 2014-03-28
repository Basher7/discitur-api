using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AngulaDemo
{
    public class RegistrationMailConfig : ConfigurationSection
    {
        public static RegistrationMailConfig GetConfiguration()
        {
            RegistrationMailConfig configuration = 
                ConfigurationManager
                .GetSection("RegistrationMail")
                as RegistrationMailConfig;

            if (configuration != null)
                return configuration;
            throw new Exception("RegistrationMail Config NOT Present");
        }

        [ConfigurationProperty("template", IsRequired = false)]
        public string Template
        {
            get
            {
                return this["template"] as string;
            }
        }


        [ConfigurationProperty("from", IsRequired = false)]
        public string From
        {
            get
            {
                return this["from"] as string;
            }
        }

        [ConfigurationProperty("subject", IsRequired = false)]
        public string Subject
        {
            get
            {
                return this["subject"] as string;
            }
        }
    
    }
}