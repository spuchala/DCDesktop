using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using AttributeRouting;
using AttributeRouting.Web.Http;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;

namespace DayCareWebAPI.Controllers
{
    public class ParentController : ApiController
    {
        private readonly DayCareService _rep;

        public ParentController()
        {
            _rep = new DayCareService();
        }

        // GET: api/Parent
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Parent/5
        [Authorize(Roles = Constants.ParentRole)]
        public IHttpActionResult Get(Guid id)
        {
            var parent = _rep.GetParent(id, null);
            return parent != null ? (IHttpActionResult)Ok(parent) : NotFound();
        }

        // GET kid log for today
        [AllowAnonymous]
        [HttpPost]
        [Route("api/parent/CheckParentByEmail")]
        public IHttpActionResult CheckParentByEmail(Parent parent)
        {
            var outParent = _rep.CheckParentByEmail(parent.Email);
            if (outParent == null) return NotFound();
            if (outParent.HasKidInSystem && !outParent.IsRegistered)
                return Ok(outParent);
            if (outParent.HasKidInSystem && outParent.IsRegistered)
                return Content(HttpStatusCode.InternalServerError, "You are already registered. Please signin.");
            if (!outParent.HasKidInSystem && !outParent.IsRegistered)
                return Content(HttpStatusCode.InternalServerError, "Your day care hasn't registered you yet. Please contact your day care.");
            return NotFound();
        }

        // POST: api/Parent
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Post(Parent parent)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var parentData = _rep.InsertParent(parent);
            if (string.IsNullOrEmpty(parentData.Error))
            {
                //var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
                //identity.AddClaim(new Claim(ClaimTypes.Name, parentData.ParentId.ToString()));
                //identity.AddClaim(new Claim(ClaimTypes.Role, Constants.ParentRole));
                //var tokenExpiration = TimeSpan.FromDays(100);
                //var props = new AuthenticationProperties()
                //{
                //    IssuedUtc = DateTime.UtcNow,
                //    ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
                //};

                //var ticket = new AuthenticationTicket(identity, props);
                //var tokenResponse = new JObject(
                //    new JProperty("id", parentData.ParentId.ToString()),
                //    new JProperty("userName", parentData.FName + " " + parentData.LName),
                //    new JProperty("userEmail", parentData.Email),
                //    new JProperty("userRole", Constants.ParentRole),
                //    new JProperty("userPhone", parentData.Phone),
                //    new JProperty("userToken", AuthStartUpService.OAuthBearerOptions.AccessTokenFormat.Protect(ticket))
                //   );
                //return Request.CreateResponse(HttpStatusCode.Created, tokenResponse);
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.Conflict, parentData.Error);
        }

        // PUT: api/Parent/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Parent/5
        public void Delete(int id)
        {
        }
    }
}
