using DayCareWebAPI.Services;
using System;
using System.Net;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using System.Collections.Generic;

namespace DayCareWebAPI.Controllers
{
    public class ScheduleController : ApiController
    {
        private readonly DayCareService _serv;

        public ScheduleController()
        {
            _serv = new DayCareService();
        }

        [HttpGet]
        [Authorize]
        [Route("api/schedule/GetSchedules/{id}")]
        public IHttpActionResult GetSchedules(Guid id)
        {
            List<Schedule> schedules = null;
            if (User.IsInRole(Constants.DayCareRole))
                schedules = _serv.GetSchedules(id);
            else if (User.IsInRole(Constants.ParentRole))
            {
                var dayCareId = _serv.GetParentsDayCare(id);
                if (dayCareId.HasValue)
                    schedules = _serv.GetSchedules(dayCareId.Value);
            }
            return Ok(schedules);
        }

        [HttpGet]
        [Authorize]
        [Route("api/schedule/GetSchedule/{id}/{scheduleId}")]
        public IHttpActionResult GetSchedule(Guid id, int scheduleId)
        {
            Schedule schedule = null;
            if (User.IsInRole(Constants.DayCareRole))
                schedule = _serv.GetSchedule(id, scheduleId);
            else if (User.IsInRole(Constants.ParentRole))
            {
                var dayCareId = _serv.GetParentsDayCare(id);
                if (dayCareId.HasValue)
                {
                    schedule = _serv.GetSchedule(id, scheduleId);
                }
            }
            return Ok(schedule);
        }

        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/InsertSchedule")]
        public IHttpActionResult InsertSchedule(Schedule schedule)
        {
            var response = _serv.InsertSchedule(schedule);
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
            var response = _serv.SaveScheduleMessage(message, new Guid(User.Identity.Name));
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                              Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        //remove document
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Put(int id)
        {
            var response = _serv.RemoveSchedule(new Guid(User.Identity.Name), id);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/RemoveScheduleMessage/{dayCareId}/{scheduleId}/{messageId}")]
        public HttpResponseMessage RemoveScheduleMessage(Guid dayCareId, int scheduleId, int messageId)
        {
            var response = _serv.RemoveScheduleMessage(dayCareId, scheduleId, messageId);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        //send email
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/schedule/SendEmail")]
        public HttpResponseMessage SendEmail(Schedule data)
        {
            var response = _serv.SendScheduleEmail(data);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
