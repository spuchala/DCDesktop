using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using AttributeRouting;
using AttributeRouting.Web.Http;


namespace DayCareWebAPI.Controllers
{
    public class LogController : ApiController
    {
        private readonly DayCareService _rep;

        public LogController()
        {
            _rep = new DayCareService();
        }

        // GET: api/Log/5
        public IHttpActionResult Get(Guid id)
        {
            var log = _rep.GetKidLogByLogId(id);
            return Ok(log);
        }

        // GET kid log for today
        [HttpGet]
        [Authorize]
        [Route("api/log/GetKidLogToday/{kidId}/{dayCareId}")]
        public IHttpActionResult GetKidLogToday(int kidId, Guid dayCareId)
        {
            var log = _rep.GetKidLogOnADay(kidId, dayCareId, DateTime.Today.ToString("yyyy-MM-dd"), false);
            return log != null ? (IHttpActionResult)Ok(log) : NotFound();
        }

        // GET kid log on a day
        [HttpGet]
        [Authorize]
        [Route("api/log/GetKidLogOnADay/{kidId}/{id}/{day}")]
        public IHttpActionResult GetKidLogOnADay(int kidId, Guid id, string day)
        {
            var log = _rep.GetKidLogOnADay(kidId, id, day, true);
            return Ok(log);
        }

        // GET kid log from start and end day
        [HttpGet]
        [Route("api/log/GetKidLogsInDayRange/{kidId}/{dayCareId}/{startDay}/{endDay}")]
        public IHttpActionResult GetKidLogsInDayRange(int kidId, Guid dayCareId, string startDay, string endDay)
        {
            var log = _rep.GetKidLogsInDayRange(kidId, dayCareId, Convert.ToDateTime(startDay), Convert.ToDateTime(endDay));
            return Ok(log);
        }

        // POST: api/Log
        public HttpResponseMessage Post(KidLog log)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var logData = _rep.InsertKidLog(log);
            var response = string.IsNullOrEmpty(logData.Error) ? Request.CreateResponse(HttpStatusCode.Created, logData) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, logData.Error);
            return response;
        }

        // PUT: api/Log/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Log/5
        public void Delete(int id)
        {
        }
    }
}
