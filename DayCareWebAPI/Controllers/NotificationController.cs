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
    public class NotificationController : ApiController
    {
        private readonly DayCareService _rep;

        public NotificationController()
        {
            _rep = new DayCareService();
        }

        // GET number of notifications
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/notification/GetCount/{dayCareId}")]
        public IHttpActionResult GetCount(Guid dayCareId)
        {
            var nots = _rep.GetNotifications(dayCareId);
            return Ok(nots != null && nots.Any() ? nots.Count() : 0);
        }

        // GET all notifications
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            var nots = _rep.GetNotifications(id);
            return Ok(nots);
        }

        // remove kid
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/notification/RemoveNotification/{dayCareId}/{id}")]
        public HttpResponseMessage RemoveNotification(Guid dayCareId, int id)
        {
            var response = _rep.RemoveNotification(dayCareId, id);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
