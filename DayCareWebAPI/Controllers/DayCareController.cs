using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using DayCareWebAPI.Repository;
using System.Web;
//using System.Web.Http.Cors;

namespace DayCareWebAPI.Controllers
{
    //[EnableCors("*", "*", "*")]
    public class DayCareController : ApiController
    {
        private readonly DayCareService _rep;

        public DayCareController()
        {
            _rep = new DayCareService();
        }

        // GET: api/DayCare
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/DayCare/5
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            try
            {
                var dayCareData = _rep.GetDayCareData(id);
                return Ok(dayCareData);
            }
            catch (Exception ex)
            {
                _rep.InsertError(ex.InnerException.Message, "while getting role for daycare", id.ToString(), ex.StackTrace);
                return Ok(ex.Message);
            }
        }

        // POST: api/DayCare
        public HttpResponseMessage Post(DayCare dayCare)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var dayCareData = _rep.InsertDayCare(dayCare);
            if (string.IsNullOrEmpty(dayCare.Error))
            {
                //var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
                //identity.AddClaim(new Claim(ClaimTypes.Name, dayCare.DayCareId.ToString()));
                //identity.AddClaim(new Claim(ClaimTypes.Role, Constants.DayCareRole));
                //var tokenExpiration = TimeSpan.FromDays(100);
                //var props = new AuthenticationProperties()
                //{
                //    IssuedUtc = DateTime.UtcNow,
                //    ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
                //};

                //var ticket = new AuthenticationTicket(identity, props);
                //var tokenResponse = new JObject(
                //    new JProperty("id", dayCare.DayCareId.ToString()),
                //    new JProperty("userName", dayCare.FName + " " + dayCare.LName),
                //    new JProperty("userEmail", dayCare.Email),
                //    new JProperty("userRole", Constants.DayCareRole),
                //    new JProperty("userPhone", dayCare.Phone),
                //    new JProperty("userToken", AuthStartUpService.OAuthBearerOptions.AccessTokenFormat.Protect(ticket))
                //   );
                //return Request.CreateResponse(HttpStatusCode.Created, tokenResponse);
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.Conflict, dayCareData.Error);
        }

        // send todays report to parents
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/SendEmail")]
        public HttpResponseMessage SendEmail(Email emailData)
        {
            var response = _rep.SendEmailToParents(emailData.From, emailData.To);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // GET day care info
        [HttpGet]
        [AllowAnonymous]
        [Route("api/dayCare/GetDayCareInfo/{dayCareId}")]
        public IHttpActionResult GetDayCareInfo(Guid dayCareId)
        {
            var response = _rep.GetDayCareInfo(dayCareId);
            return Ok(response);
        }

        // GET day care info
        [HttpGet]
        [AllowAnonymous]
        [Route("api/dayCare/GetDayCareInfoHome/{dayCareId}")]
        public IHttpActionResult GetDayCareInfoHome(Guid dayCareId)
        {
            var response = _rep.GetDayCareInfo(dayCareId);
            if (response != null && response.DescriptionHome != null)
                return Ok(response.DescriptionHome);
            else
                return Ok(string.Empty);
        }

        // GET day care info
        [HttpGet]
        [AllowAnonymous]
        [Route("api/dayCare/GetDayCareInfoAboutUs/{dayCareId}")]
        public IHttpActionResult GetDayCareInfoAboutUs(Guid dayCareId)
        {
            var response = _rep.GetDayCareInfo(dayCareId);
            if (response != null && response.DescriptionAboutUs != null)
                return Ok(response.DescriptionAboutUs);
            else
                return Ok(string.Empty);
        }

        // GET day care info
        [HttpGet]
        [AllowAnonymous]
        [Route("api/dayCare/GetDayCareInfoProgram/{dayCareId}")]
        public IHttpActionResult GetDayCareInfoProgram(Guid dayCareId)
        {
            var response = _rep.GetDayCareInfo(dayCareId);
            if (response != null && response.DescriptionProgram != null)
                return Ok(response.DescriptionProgram);
            else
                return Ok(string.Empty);
        }

        // GET day care info
        [HttpGet]
        [AllowAnonymous]
        [Route("api/dayCare/GetDayCareInfoImages/{dayCareId}")]
        public IHttpActionResult GetDayCareInfoImages(Guid dayCareId)
        {
            var response = _rep.GetDayCareInfo(dayCareId);
            if (response != null && response.DescriptionProgram != null)
                return Ok(response.Snaps);
            else
                return Ok(string.Empty);
        }

        // send todays custom report to parents
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/SendCustomEmail")]
        public HttpResponseMessage SendCustomEmail(Email emailData)
        {
            var response = _rep.SendCustomEmailsToParents(emailData.From, emailData.To);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // manage day care info
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/ManageDayCareInfo")]
        public HttpResponseMessage ManageDayCareInfo(DayCareInfo info)
        {
            var response = _rep.ManageDayCareInfo(info);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // manage day care info home
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/ManageDayCareInfoHome")]
        public HttpResponseMessage ManageDayCareInfoHome(DayCareInfo info)
        {
            var response = _rep.ManageDayCareInfoHome(info);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/dayCare/UploadImage")]
        public HttpResponseMessage UploadImage()
        {
            var repo1 = new DayCareRepository();
            repo1.LogError("entered upload image", "entered uploaded image", "test", "test");
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count < 1)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    var filePath = HttpContext.Current.Server.MapPath("~/ftp/snaps/" + User.Identity.Name + "/" + postedFile.FileName);
                    postedFile.SaveAs(filePath);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                var repo = new DayCareRepository();
                repo.LogError(ex.Message, "image upload failed", User.Identity.Name, ex.StackTrace);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // manage day care info about us
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/ManageDayCareInfoAbout")]
        public HttpResponseMessage ManageDayCareInfoAbout(DayCareInfo info)
        {
            var response = _rep.ManageDayCareInfoAbout(info);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // manage day care info program
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/ManageDayCareInfoProgram")]
        public HttpResponseMessage ManageDayCareInfoProgram(DayCareInfo info)
        {
            var response = _rep.ManageDayCareInfoProgram(info);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // upload custom report 
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/UploadSnap")]
        public async Task<IHttpActionResult> UploadSnap()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Content(HttpStatusCode.InternalServerError, "Not a Valid file!");
            }
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                Stream customRepData = await file.ReadAsStreamAsync();
                var type = file.Headers.ContentType.MediaType;
                var fileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                Stream docData = await file.ReadAsStreamAsync();
                var _docRepo = new DocumentService();
                var response = _docRepo.SaveSnap(docData, fileName, new Guid(User.Identity.Name), file.Headers.ContentLength);
                return Ok(response);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, Constants.Error);
            }
        }

        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/dayCare/UploadSnapBase64")]
        public IHttpActionResult UploadSnapBase64(dynamic data)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(data.Content);
                var _docRepo = new DocumentService();
                var response = _docRepo.SaveSnapBase64(imageBytes, data.FileName, new Guid(User.Identity.Name));
                return Ok(response);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, Constants.Error);
            }
        }

        // PUT: api/DayCare/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/DayCare/5
        public void Delete(int id)
        {
        }
    }
}
