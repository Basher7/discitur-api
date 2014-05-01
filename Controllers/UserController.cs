using ImageResizer;
using Mag14.discitur.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Mag14.Controllers
{
    [Authorize]
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private DisciturContext db = new DisciturContext();

        // GET api/User
        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        // GET api/User/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // PUT api/User
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> PutUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userName = User.Identity.GetUserName();
            User cUser =  await db.Users.Where(u => u.UserName.Equals(userName)).FirstAsync<Mag14.discitur.Models.User>();

            if (cUser.UserId != user.UserId)
            {
                return BadRequest();
            }

            cUser.Email = user.Email;

            db.Entry(cUser).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(cUser);
        }

        // POST api/User
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        // DELETE api/User/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Ok(user);
        }

        // GET api/User/anyEmail
        [AllowAnonymous]
        [ResponseType(typeof(bool))]
        [Route("anyEmail")]
        [HttpGet]
        public async Task<IHttpActionResult> isAnyEmail(string email)
        {
            try
            {
                return Ok(await db.Users.AnyAsync(u => u.Email.Equals(email)));
            }
            catch
            {
                return Ok(false);
            }
        }

        // POST api/User/Image
        [Route("Image")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostUserImage()
        {
            // Check if the request contains multipart/form-data.
            if (Request.Content.IsMimeMultipartContent())
            {
                User user;
                //var FormData = await Request.Content.ReadAsFormDataAsync();
                try
                {
                    int id = int.Parse(HttpContext.Current.Request.Form["UserId"]);
                    user = db.Users.Find(id);
                }
                catch {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var streamProvider = new MultipartMemoryStreamProvider();
                streamProvider = await Request.Content.ReadAsMultipartAsync(streamProvider);

                foreach (var item in streamProvider.Contents.Where(c => !string.IsNullOrEmpty(c.Headers.ContentDisposition.FileName)))
                {   
                    Stream stPictureSource = new MemoryStream(await item.ReadAsByteArrayAsync());
                    Stream stThumbSource = new MemoryStream(await item.ReadAsByteArrayAsync());

                    // Resize for Picture
                    MemoryStream stPictureDest = new MemoryStream();
                    var pictureSettings = new ResizeSettings
                    {
                        MaxWidth = Constants.USER_PICTURE_MAXWIDTH,
                        MaxHeight = Constants.USER_PICTURE_MAXHEIGHT,
                        Format = Constants.USER_PICTURE_FORMAT,
                        Mode = FitMode.Crop
                    };
                    ImageBuilder.Current.Build(stPictureSource, stPictureDest, pictureSettings);

                    // Resize for ThumbNail
                    MemoryStream stThumbDest = new MemoryStream();
                    var thumbSettings = new ResizeSettings
                    {
                        MaxWidth = Constants.USER_THUMB_MAXWIDTH,
                        MaxHeight = Constants.USER_THUMB_MAXHEIGHT,
                        Format = Constants.USER_THUMB_FORMAT,
                        Mode = FitMode.Crop
                    };
                    ImageBuilder.Current.Build(stThumbSource, stThumbDest, thumbSettings);

                    user.Picture = "data:image/gif;base64," + Convert.ToBase64String(stPictureDest.ToArray());
                    user.Thumb = "data:image/gif;base64," + Convert.ToBase64String(stThumbDest.ToArray());
                }
                await db.SaveChangesAsync();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else{
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

        }

        private IHttpActionResult BadDisciturRequest(string errorCode)
        {
            AddModelError(errorCode);
            return BadRequest(ModelState);
        }

        private void AddModelError(string errorCode)
        {
            ModelState.AddModelError(Constants.DISCITUR_ERRORS, errorCode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}