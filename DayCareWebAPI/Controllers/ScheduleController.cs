using DayCareWebAPI.Services;
using System;
using System.Net;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;

namespace DayCareWebAPI.Controllers
{
    public class ScheduleController : ApiController
    {
        private readonly DayCareService _rep;

        public ScheduleController()
        {
            _rep = new DayCareService();
        }

        [HttpGet]
        [Authorize]
        [Route("api/schedule/GetSchedules/{dayCareId}")]
        public IHttpActionResult GetSchedules(Guid dayCareId)
        {
            var schedules = _rep.GetSchedules(dayCareId);
            return Ok(schedules);
        }

        [HttpGet]
        [Authorize]
        [Route("api/schedule/GetSchedule/{dayCareId}/{scheduleId}")]
        public IHttpActionResult GetSchedule(Guid dayCareId, int scheduleId)
        {
            var schedule = _rep.GetSchedule(dayCareId, scheduleId);
            return Ok(schedule);
        }

        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/InsertSchedule")]
        public IHttpActionResult InsertSchedule(Schedule schedule)
        {
            var response = _rep.InsertSchedule(schedule);
            if (response == null) return NotFound();
            else if (string.IsNullOrEmpty(response.Error))
                return Ok(response);
            return Content(HttpStatusCode.InternalServerError, response.Error);
        }

        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/SaveScheduleMessage")]
        public HttpResponseMessage SaveScheduleMessage(ScheduleMessage message)
        {
            var response = _rep.SaveScheduleMessage(message, new Guid(User.Identity.Name));
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                              Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        //remove document
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Put(int id)
        {
            var response = _rep.RemoveSchedule(new Guid(User.Identity.Name), id);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/RemoveScheduleMessage/{dayCareId}/{scheduleId}/{messageId}")]
        public HttpResponseMessage RemoveScheduleMessage(Guid dayCareId, int scheduleId, int messageId)
        {
            var response = _rep.RemoveScheduleMessage(dayCareId, scheduleId, messageId);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        //send email
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/SendEmail")]
        public HttpResponseMessage SendEmail(Schedule data)
        {
            var response = _rep.SendScheduleEmail(data);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
