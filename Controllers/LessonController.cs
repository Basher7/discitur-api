using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Mag14.discitur.Models;
using Mag14.Models;

namespace Mag14.Controllers
{
    public class LessonController : ApiController
    {
        private DisciturContext db = new DisciturContext();

        /*
        // GET api/Lesson
        public IQueryable<Lesson> GetLessons()
        {
            return db.Lessons;
        }
        */

        /*
        public async Task<IEnumerable<Lesson>> GetLessons()
        {
            return await db.Lessons.ToListAsync<Lesson>();
        }
        */

        [NonAction]
        private Func<Lesson, Object> getLessonField(string fieldName)
        {
            Func<Lesson, Object> orderByFunc = null;
            switch (fieldName)
            {
                case "Title":
                    orderByFunc = sl => sl.Title;
                    break;
                case "PublishDate":
                    orderByFunc = sl => sl.PublishDate;
                    break;
                case "Rate":
                    orderByFunc = sl => sl.Rate;
                    break;
                // so on
                default:
                    orderByFunc = sl => sl.PublishDate;
                    break;
            }
            return orderByFunc;
        }
        
        [HttpGet]
        public async Task<IEnumerable<Lesson>> Search(
            string keyword=null, 
            bool inContent=false, 
            string discipline=null, 
            string school=null, 
            string classroom=null, 
            int rate=-1, 
            string publishedOn=null, 
            string publishedBy=null,
            int startRow=0,
            int pageSize=99999,
            string orderBy="PublishDate",
            string orderDir="ASC")
        {
            IQueryable<Lesson> lessons = db.Lessons;

            if (!String.IsNullOrEmpty(keyword))
            {
                if (inContent)
                    lessons = lessons.Where(l => l.Content.Contains(keyword) || l.Conclusion.Contains(keyword));
                else
                    lessons = lessons.Where(l => l.Title.Contains(keyword));

            }
            if (!String.IsNullOrEmpty(discipline))
                lessons = lessons.Where(l => l.Discipline.Contains(discipline));
            if (!String.IsNullOrEmpty(school))
                lessons = lessons.Where(l => l.School.Contains(school));
            if (!String.IsNullOrEmpty(classroom))
                lessons = lessons.Where(l => l.Classroom.Contains(classroom));
            if (rate>-1)
                lessons = lessons.Where(l => l.Rate.Equals(rate));
            if (!String.IsNullOrEmpty(publishedOn))
                lessons = lessons.Where(l => l.PublishDate.Equals(DateTime.Parse(publishedOn)));
            if (!String.IsNullOrEmpty(publishedBy))
                lessons = lessons.Where(l => (l.Author.Name + l.Author.Surname).Contains(publishedBy));
            /*
            if (orderDir.Equals("DESC"))
                lessons = lessons.OrderByDescending(getLessonField(orderBy)).Skip(startRow).Take(pageSize).AsQueryable();
            else
                lessons = lessons.OrderBy(getLessonField(orderBy)).Skip(startRow).Take(pageSize).AsQueryable();
            */
            lessons = lessons.OrderBy(orderBy + " " +orderDir).Skip(startRow).Take(pageSize);
            //lessons = lessons.Skip(startRow).Take(pageSize);

            return await lessons.ToListAsync<Lesson>();
            //return await db.Lessons.Where(l => l.Discipline.Equals(discipline)).ToListAsync<Lesson>();

            //return await db.Lessons.Where(l => l.Discipline.Equals(discipline)).ToListAsync<Lesson>();
        }
        

        

        // GET api/Lesson/5
        [ResponseType(typeof(Lesson))]
        public async Task<IHttpActionResult> GetLesson(int id)
        {
            Lesson lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }

        // PUT api/Lesson/5
        public async Task<IHttpActionResult> PutLesson(int id, Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != lesson.LessonId)
            {
                return BadRequest();
            }

            db.Entry(lesson).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Lesson
        [ResponseType(typeof(Lesson))]
        public async Task<IHttpActionResult> PostLesson(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Lessons.Add(lesson);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = lesson.LessonId }, lesson);
        }

        // DELETE api/Lesson/5
        [ResponseType(typeof(Lesson))]
        public async Task<IHttpActionResult> DeleteLesson(int id)
        {
            Lesson lesson = await db.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();

            return Ok(lesson);
        }

        /*
        [Route("search")]
        public IQueryable<Lesson> Search(string discipline = null, string school = null, string longitude = null)
        {

            Lesson lesson = db.Lessons.S;
            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }
        */


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LessonExists(int id)
        {
            return db.Lessons.Count(e => e.LessonId == id) > 0;
        }
    }
}