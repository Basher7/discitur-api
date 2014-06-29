using Mag14.discitur.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mag14.Controllers
{
    public class SEOLessonController : Controller
    {
        private DisciturContext db = new DisciturContext();

        public string Index()
        {
            return "This is my <b>default</b> action...";
        }

        public ActionResult List()
        {
            return View(db.Lessons.ToList());
        }

        public async Task<ActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Only lesson not (logically) deleted are returned
            Lesson lesson = await db.Lessons.FirstAsync(l => l.LessonId.Equals(id) && l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

	}
}