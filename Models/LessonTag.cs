using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mag14.discitur.Models
{
    [Table("discitur.LessonTag")]
    public class LessonTag
    {
        [Key]
        [Column(Order = 0)]
        public string LessonTagName { get; set; }
        [Key]
        [Column(Order = 1)]
        public int LessonId { get; set; }
        //[ForeignKey("LessonId")]
        //virtual public Lesson Lesson { get; set; }

    }
}