using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;

namespace DayCareWebAPI.Controllers
{
    public class KidController : ApiController
    {

        private readonly DayCareService _rep;

        public KidController()
        {
            _rep = new DayCareService();
        }

        // GET All Kids in a day care: api/Kid/id    
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            var kids = _rep.GetKidsFromDayCare(id);
            return Ok(kids);
        }

        // GET kids for parent
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/GetKidsFromDayCare/{dayCareId}")]
        public IHttpActionResult GetKidsFromDayCare(Guid dayCareId)
        {
            var kids = _rep.GetKidsFromDayCare(dayCareId);
            return Ok(kids);
        }

        // GET kids for parent
        [HttpGet]
        [Authorize(Roles = Constants.ParentRole)]
        [Route("api/kid/GetKidsFromParent/{parentId}")]
        public IHttpActionResult GetKidsFromParent(Guid parentId)
        {
            var kids = _rep.GetKidsFromParent(parentId);
            return Ok(kids);
        }

        // GET kids in class
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/GetKidsInClass/{classId}")]
        public IHttpActionResult GetKidsInClass(Guid classId)
        {
            var kids = _rep.GetKidsInClass(classId);
            return Ok(kids);
        }

        // GET attendance for kids
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/GetKidsAttendance/{dayCareId}")]
        public IHttpActionResult GetKidsAttendance(Guid dayCareId)
        {
            var kids = _rep.GetAttendance(dayCareId);
            return Ok(kids);
        }

        // GET a kid by day care id and kid id
        [HttpGet]
        [Route("api/kid/GetKidInDayCare/{kidId}/{dayCareId}")]
        public IHttpActionResult GetKidInDayCare(int kidId, Guid dayCareId)
        {
            var kids = _rep.GetKid(kidId, dayCareId);
            return Ok(kids);
        }

        // GET kids with no class
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/GetKidsWithNoClass/{dayCareId}")]
        public IHttpActionResult GetKidsWithNoClass(Guid dayCareId)
        {
            var kids = _rep.GetKidsWithNoClass(dayCareId);
            return Ok(kids);
        }

        // log attendance
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/LogKidAttendance")]
        public HttpResponseMessage LogKidAttendance(List<Attendance> kids)
        {
            var response = _rep.InsertAttendance(kids);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/EditKid")]
        public HttpResponseMessage EditKid(Kid kid)
        {
            var response = _rep.UpdateKid(kid);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // remove kid
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/RemoveKid/{kidId}/{dayCareId}/{reason}")]
        public HttpResponseMessage RemoveKid(int kidId, Guid dayCareId, string reason)
        {
            var response = _rep.RemoveKid(kidId, dayCareId, reason);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/RemoveKid/{kidId}/{dayCareId}")]
        public HttpResponseMessage RemoveKid(int kidId, Guid dayCareId)
        {
            var response = _rep.RemoveKid(kidId, dayCareId, null);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // POST: api/Kid
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Post(Kid kid)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var kidData = _rep.InsertKid(kid);
            var response = string.IsNullOrEmpty(kidData.Error) ? Request.CreateResponse(HttpStatusCode.Created, kid) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, kidData.Error);
            return response;
        }

        // POST: api/Kid
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/kid/InsertKidShort")]
        public HttpResponseMessage InsertKidShort(Kid kid)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var kidData = _rep.InsertKidShort(kid);
            var response = string.IsNullOrEmpty(kidData.Error) ? Request.CreateResponse(HttpStatusCode.Created, kid) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, kidData.Error);
            return response;
        }

        // PUT: api/Kid/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Kid/5
        public void Delete(int id)
        {
        }
    }
}
