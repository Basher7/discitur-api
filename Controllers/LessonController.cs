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
using System.Diagnostics;

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
        public async Task<List<string>> FindDiscipline(string disciplineQ)
        {
            //List<string> disciplines = new List<string>();
            IQueryable<string> disciplines = db.Lessons
                                                .Where(l => l.Discipline.Contains(disciplineQ))
                                                .Select(l => l.Discipline).Distinct();

            return await disciplines.ToListAsync();
            //var results = (from lesson in db.Lessons select lesson.Discipline).Distinct();
        }

        [HttpGet]
        public async Task<List<string>> FindSchool(string schoolQ)
        {
            IQueryable<string> schools = db.Lessons
                                                .Where(l => l.School.Contains(schoolQ))
                                                .Select(l => l.School).Distinct();

            return await schools.ToListAsync();
        }

        [HttpGet]
        public async Task<List<string>> FindClassRoom(string classRoomQ)
        {
            IQueryable<string> classRooms = db.Lessons
                                                .Where(l => l.Classroom.Contains(classRoomQ))
                                                .Select(l => l.Classroom).Distinct();

            return await classRooms.ToListAsync();
        }

        [HttpGet]
        public async Task<List<string>> FindTags(string tagQ)
        {
            IQueryable<string> tags = db.LessonTags
                                                .Where(l => l.LessonTagName.Contains(tagQ))
                                                .Select(l => l.LessonTagName).Distinct();

            return await tags.ToListAsync();
        }


        [NonAction]
        private List<string> GetQueryArrayParameters(string arrayValuesQueryString)
        {
            var retval = new List<string>();
            foreach (var item in arrayValuesQueryString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                //var split = item.Split('=');
                retval.Add(item);
            }
            return retval;
        }

        [HttpGet]
        public async Task<PagedList<Lesson>> Search(
            string keyword=null, 
            bool inContent=false, 
            string discipline=null, 
            string school=null, 
            string classroom=null, 
            int rate=-1, 
            string tags=null,
            string publishedOn=null, 
            string publishedBy=null,
            int startRow=0,
            int pageSize=99999,
            string orderBy="PublishDate",
            string orderDir="ASC")
        {
            PagedList<Lesson> page = new PagedList<Lesson>();
            IQueryable<Lesson> lessons = db.Lessons;

            if (!String.IsNullOrEmpty(keyword))
            {
                if (inContent)
                    lessons = lessons.Where(l => l.Content.Contains(keyword) || l.Conclusion.Contains(keyword));
                else
                    lessons = lessons.Where(l => l.Title.Contains(keyword));

            }
            if (!String.IsNullOrEmpty(discipline))
                lessons = lessons.Where(l => l.Discipline.Equals(discipline));
            if (!String.IsNullOrEmpty(school))
                lessons = lessons.Where(l => l.School.Equals(school));
            if (!String.IsNullOrEmpty(classroom))
                lessons = lessons.Where(l => l.Classroom.Equals(classroom));
            if (rate>-1)
                lessons = lessons.Where(l => l.Rate.Equals(rate));
            if (!String.IsNullOrEmpty(tags))
            {
                foreach (string tag in GetQueryArrayParameters(tags))
                {
                    lessons = lessons.Where(l => l.Tags.Any(t => t.LessonTagName.Equals(tag)));
                }
                /*
                lessons = lessons.Include(l => l.Tags);
                foreach (string tag in GetQueryArrayParameters(tags))
                {
                    var query = from l in lessons
                                select new { Lesson = l, Tag = l.Tags.Where(t => t.LessonTagName.Equals(tag))};
                    lessons = query.AsQueryable().Select(p => p.Lesson);
                }
                */
            }
            if (!String.IsNullOrEmpty(publishedOn))
                lessons = lessons.Where(l => l.PublishDate.Equals(DateTime.Parse(publishedOn)));
            if (!String.IsNullOrEmpty(publishedBy))
                lessons = lessons.Where(l => l.Author.UserName.Equals(publishedBy));


            // Only published lessons are returned or private lessons (not published for user)
            lessons = lessons.Where(l => 
                l.Published.Equals(Constants.LESSON_PUBLISHED) ||
                (l.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && l.Author.UserName.Equals(publishedBy))
                );

            //lessons = lessons.Where(l => l.Published.Equals(Constants.LESSON_PUBLISHED));
            // Only active lessons are returned
            lessons = lessons.Where(l => l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));

            //if (startRow == 0)
            page.Count = lessons.Count();
            page.StartRow = startRow;
            page.PageSize = pageSize;

            /*
            if (orderDir.Equals("DESC"))
                lessons = lessons.OrderByDescending(getLessonField(orderBy)).Skip(startRow).Take(pageSize).AsQueryable();
            else
                lessons = lessons.OrderBy(getLessonField(orderBy)).Skip(startRow).Take(pageSize).AsQueryable();
            */
            lessons = lessons.OrderBy(orderBy + " " +orderDir).Skip(startRow).Take(pageSize);
            //lessons = lessons.Skip(startRow).Take(pageSize);

            page.Records = await lessons.ToListAsync<Lesson>();

            return page;
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
        [Authorize]
        [ResponseType(typeof(Lesson))]
        public async Task<IHttpActionResult> PutLesson(int id, Lesson lesson)
        {
            // Search for lesson by same version
            IQueryable<Lesson> _lessons = db.Lessons
                                        .Where(l => l.LessonId.Equals(lesson.LessonId) &&
                                            l.Vers.Equals(lesson.Vers) &&
                                            l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            Lesson _lesson = await _lessons.FirstAsync();
            if (_lesson == null)
            {
                return StatusCode(HttpStatusCode.Conflict);
            }

            if (_lesson.Title != lesson.Title)
                _lesson.Title = lesson.Title;
            if (_lesson.Discipline != lesson.Discipline)
                _lesson.Discipline = lesson.Discipline;

            _lesson.School = lesson.School;
            _lesson.Classroom = lesson.Classroom;
            _lesson.Discipline = lesson.Discipline;
            _lesson.Content = lesson.Content;
            _lesson.Conclusion = lesson.Conclusion;
            if (_lesson.PublishDate == null)
            {
                _lesson.PublishDate = DateTime.Now;
            }

            if (_lesson.Published != lesson.Published)
                _lesson.Published = lesson.Published;
            //_lesson.PublishDate = (_lesson.PublishDate == null ? DateTime.Now : _lesson.PublishDate);
            if (_lesson.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && lesson.Published.Equals(Constants.LESSON_PUBLISHED))
            {
                _lesson.Published = _lesson.Published;
                _lesson.PublishDate = DateTime.Now;
            }

            foreach (LessonFeedback fb in lesson.FeedBacks)
            {
                LessonFeedback mfb;
                if (fb.LessonFeedbackId<=0)
                {
                    mfb = new LessonFeedback();
                    mfb.LessonId = _lesson.LessonId;
                    mfb.Feedback = fb.Feedback;
                    mfb.Nature = fb.Nature;
                    db.LessonFeedbacks.Add(mfb);
                }
                else
                {
                    mfb = _lesson.FeedBacks.First(f => f.LessonFeedbackId.Equals(fb.LessonFeedbackId));
                    if (mfb != null && mfb.Feedback != fb.Feedback)
                    {
                        mfb.Feedback = fb.Feedback;
                        db.Entry(mfb).State = EntityState.Modified;
                    }
                }
            }


            foreach (LessonTag tag in lesson.Tags)
            {
                LessonTag t;
                if (tag.LessonId > 0 && tag.LessonId.Equals(_lesson.LessonId))
                {
                    if (tag.status.Equals("C"))
                    {
                        t = _lesson.Tags.First(_t => _t.LessonId.Equals(_lesson.LessonId) && _t.LessonTagName.Equals(tag.LessonTagName));
                        _lesson.Tags.Remove(t);
                    }
                    else if (tag.status.Equals("A"))
                    {
                        t = new LessonTag();
                        t.LessonId = _lesson.LessonId;
                        t.LessonTagName = tag.LessonTagName;
                        db.LessonTags.Add(t);
                    }
                }
                else
                    return BadRequest();//TODO: sistemare eccezioni
            }



            _lesson.LastModifUser = lesson.LastModifUser;
            _lesson.LastModifDate = DateTime.Now;
            _lesson.Vers += 1;

            db.Entry(_lesson).State = EntityState.Modified;
            try
            {
                db.Database.Log = s => Debug.WriteLine(s);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(_lesson);
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

        [Route("api/lesson/{lessonId}/comments")]
        [HttpGet]
        public async Task<List<LessonComment>> GetCommentsByLessonId(int lessonId)
        {
            IQueryable<LessonComment> comments = db.LessonComments
                                        .Where(c => c.LessonId.Equals(lessonId) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            return await comments.ToListAsync();
        }

        // POST api/lesson
        [Authorize]
        [Route("api/lesson/{lessonId}/comment")]
        [HttpPost]
        [ResponseType(typeof(LessonComment))]
        public async Task<IHttpActionResult> PostLessonComment(LessonComment comment)
        {
            LessonComment _comment = new LessonComment();
            _comment.Content = comment.Content;
            _comment.CreationDate = DateTime.Now;
            _comment.Date = DateTime.Now;
            _comment.LastModifDate = DateTime.Now;
            _comment.LessonId = comment.LessonId;
            _comment.Level = comment.Level;
            _comment.ParentId = comment.ParentId;
            _comment.Vers = 1;
            _comment.RecordState = Constants.RECORD_STATE_ACTIVE;
            _comment.UserId = comment.Author.UserId;
            try
            {
                _comment.Author = await db.Users.FindAsync(comment.Author.UserId);
                _comment.LastModifUser = _comment.Author.UserName;
            }
            catch(Exception e){
                return BadRequest(e.Message);
            }

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            db.LessonComments.Add(_comment);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(_comment);
            //return CreatedAtRoute("DefaultApi", new { id = comment.Id }, comment);
        }


        // PUT api/lesson/3/comment/13
        [Authorize]
        [Route("api/lesson/{lessonId}/comment/{id}")]
        [HttpPut]
        [ResponseType(typeof(LessonComment))]
        public async Task<IHttpActionResult> PutLessonComment(int id, LessonComment comment)
        {
            LessonComment _comment = await db.LessonComments.FindAsync(id);
            _comment.Content = comment.Content;
            _comment.Date = DateTime.Now;
            _comment.LastModifDate = DateTime.Now;
            _comment.Vers += 1;

            db.Entry(_comment).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(_comment);
        }

        // PUT api/lesson/3/comment/13/delete
        [Authorize]
        [Route("api/lesson/{lessonId}/comment/{id}/delete")]
        [HttpPut]
        [ResponseType(typeof(LessonComment))]
        public async Task<IHttpActionResult> DeleteLessonComment(int id, LessonComment comment)
        {
            // Search for child comments, if exists the comment can't be deleted
            IQueryable<LessonComment> comments = db.LessonComments
                                        .Where(c => (c.ParentId.HasValue && c.ParentId.Value.Equals(id)) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            int count = await comments.CountAsync();
            if (count == 0)
            {
                LessonComment _comment = await db.LessonComments.FindAsync(id);
                if (_comment == null)
                {
                    return NotFound();
                }
                _comment.LastModifDate = DateTime.Now;
                _comment.LastModifUser = comment.Author.UserName;
                _comment.RecordState = Constants.RECORD_STATE_DELETED;

                db.Entry(_comment).State = EntityState.Modified;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

                return Ok(_comment);

            }
            else
            {
                return BadRequest("comment linked by other user's comments");
            }
        }



        /**** Lesson Ratings ****/

        [Route("api/lesson/{lessonId}/ratings")]
        [HttpGet]
        public async Task<List<LessonRating>> GetRatingsByLessonId(int lessonId)
        {
            IQueryable<LessonRating> ratings = db.LessonRatings
                                        .Where(r => r.LessonId.Equals(lessonId) && r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            return await ratings.ToListAsync();
        }

        // POST api/lesson/5/rating
        [Authorize]
        [Route("api/lesson/{lessonId}/rating")]
        [HttpPost]
        [ResponseType(typeof(LessonRating))]
        public async Task<IHttpActionResult> PostLessonRating(LessonRating rating)
        {
            // Search for Rating submitted by the same Author on the same Lesson
            IQueryable<LessonRating> ratings = db.LessonRatings
                                        .Where(r => r.UserId.Equals(rating.Author.UserId) && 
                                            r.LessonId.Equals(rating.LessonId) &&
                                            r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            int count = await ratings.CountAsync();
            if (count == 0)
            {
                LessonRating _rating = new LessonRating();
                _rating.LessonId = rating.LessonId;
                _rating.UserId = rating.Author.UserId;
                _rating.Rating = rating.Rating;
                _rating.Content = rating.Content ?? string.Empty;
                _rating.CreationDate = DateTime.Now;
                _rating.LastModifDate = DateTime.Now;
                _rating.Vers = 1;
                _rating.RecordState = Constants.RECORD_STATE_ACTIVE;
                try
                {
                    _rating.Author = await db.Users.FindAsync(rating.Author.UserId);
                    _rating.LastModifUser = _rating.Author.UserName;
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                // Add new Lesson's Rating
                db.LessonRatings.Add(_rating);

                // Update Lesson Rating with Average of Ratings on the same Lesson
                IQueryable<int> _prevRatings = db.LessonRatings
                                            .Where(r => r.LessonId.Equals(rating.LessonId) &&
                                                r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                            .Select(r => r.Rating);
                List<int> _RatingsList = await _prevRatings.ToListAsync();
                _RatingsList.Add(_rating.Rating);

                Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
                _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()), 1);

                //TODO: insert table fields for history and versioning
                db.Entry(_lesson).State = EntityState.Modified;
                
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }

                return Ok(_rating);
            }
            else
            {
                return BadRequest("User (id:" + rating.Author.UserId + ") already has submitted a rating for lesson id:" + rating.LessonId);
            }

        }


        // PUT api/lesson/3/comment/13
        [Authorize]
        [Route("api/lesson/{lessonId}/rating/{id}")]
        [HttpPut]
        [ResponseType(typeof(LessonRating))]
        public async Task<IHttpActionResult> PutLessonRating(int id, LessonRating rating)
        {
            LessonRating _rating = await db.LessonRatings.FindAsync(id);
            _rating.Rating = rating.Rating;
            _rating.Content = rating.Content ?? string.Empty;
            _rating.LastModifDate = DateTime.Now;
            _rating.Vers += 1;
            db.Entry(_rating).State = EntityState.Modified;

            // Update Lesson Rating with Average of Ratings on the same Lesson
            IQueryable<int> _prevRatings = db.LessonRatings
                                        .Where(r => r.LessonId.Equals(rating.LessonId) &&
                                            !r.Id.Equals(id) &&
                                            r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                        .Select(r => r.Rating);
            List<int> _RatingsList = await _prevRatings.ToListAsync();
            _RatingsList.Add(_rating.Rating);

            Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
            _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()), 1);

            //TODO: insert table fields for history and versioning
            db.Entry(_lesson).State = EntityState.Modified;
                
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(_rating);
        }

        // PUT api/lesson/3/comment/13/delete
        [Authorize]
        [Route("api/lesson/{lessonId}/rating/{id}/delete")]
        [HttpPut]
        [ResponseType(typeof(LessonRating))]
        public async Task<IHttpActionResult> DeleteLessonRating(int id, LessonRating rating)
        {
            // Get the Rating on database
            LessonRating _rating = await db.LessonRatings.FindAsync(id);
            if (_rating == null)
            {
                return NotFound();
            }
            // Get the Lesson on database
            Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
            if (_lesson == null)
            {
                return BadRequest("LessonId not valid:" + rating.LessonId);
            }
            if (_lesson.Author.UserId.Equals(rating.Author.UserId))
            {
                return BadRequest("Author's Lesson CANNOT delete his rating");
            }
            // enrich rating data
            _rating.LastModifDate = DateTime.Now;
            _rating.LastModifUser = rating.Author.UserName;
            _rating.RecordState = Constants.RECORD_STATE_DELETED;
            db.Entry(_rating).State = EntityState.Modified;

            // Update Lesson Rating with Average of Ratings on the same Lesson, but le rating is going to be deleted
            IQueryable<int> _prevRatings = db.LessonRatings
                                        .Where(r => r.LessonId.Equals(rating.LessonId) &&
                                            !r.Id.Equals(id) &&
                                            r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                        .Select(r => r.Rating);
            List<int> _RatingsList = await _prevRatings.ToListAsync();

            _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()),1);
            //TODO: insert table fields for history and versioning
            db.Entry(_lesson).State = EntityState.Modified;
                
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(_rating);


        }





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