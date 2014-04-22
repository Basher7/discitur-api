using Mag14.discitur.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
            Lesson lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

	}
}