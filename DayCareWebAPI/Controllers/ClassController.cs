using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DayCareWebAPI.Controllers
{
    public class ClassController : ApiController
    {
        private readonly DayCareService _rep;

        public ClassController()
        {
            _rep = new DayCareService();
        }

        // GET All classes for daycare    
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            var classes = _rep.GetClassesForDayCare(id);
            return Ok(classes);
        }

        // POST: api/class
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Post(Class item)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var classData = _rep.InsertClass(item);
            var response = string.IsNullOrEmpty(classData.Error) ? Request.CreateResponse(HttpStatusCode.Created, item) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, classData.Error);
            return response;
        }

        // log attendance
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/class/AssignClassToKid")]
        public HttpResponseMessage AssignClassToKid(Class item)
        {
            var response = _rep.AssignClassToKid(item);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
