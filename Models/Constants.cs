using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mag14.discitur.Models
{
    public class Constants
    {
        public const int RECORD_STATE_ACTIVE = 0;
        public const int RECORD_STATE_DELETED = 2;
        public const int LESSON_PUBLISHED = 1;
        public const int LESSON_NOT_PUBLISHED = 0;
        public const string LESSON_SEARCH_ORDER_FIELD = "PublishDate";
        public const string LESSON_SEARCH_ORDER_DIR = "DESC";

        public const string DISCITUR_ERRORS = "discerrors";
        public const string DISCITUR_ERROR_UNHANDLED = "discerr00";
        public const string DISCITUR_ERROR_USERNAME_USED = "discerr01";
        public const string DISCITUR_ERROR_EMAIL_USED = "discerr02";

    }
}