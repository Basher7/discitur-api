using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Mag14.discitur.Models
{
    public class DisciturContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public DisciturContext() : base("name=DisciturContext")
        {
        }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.Lesson> Lessons { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.UserActivation> UserActivations { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.LessonFeedback> LessonFeedbacks { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.LessonTag> LessonTags { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.LessonComment> LessonComments { get; set; }

        public System.Data.Entity.DbSet<Mag14.discitur.Models.LessonRating> LessonRatings { get; set; }
    }
}
