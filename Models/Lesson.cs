using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mag14.discitur.Models
{
    [Table("discitur.Lesson")]
    public class Lesson
    {
        public Lesson()
        {
            this.Tags = new List<LessonTag>();
            this.FeedBacks = new HashSet<LessonFeedback>();
        }

        public int LessonId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Discipline { get; set; }
        [Required]
        public string School { get; set; }
        [Required]
        public string Classroom { get; set; }
        [Required]
        public int Rate { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        virtual public User Author { get; set; }
        [Required]
        public DateTime PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
        public string Conclusion { get; set; }

        public virtual ICollection<LessonTag> Tags { get; private set; }
        public virtual ICollection<LessonFeedback> FeedBacks { get; private set; }
    }
}