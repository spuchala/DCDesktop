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
    public class InstantLogController : ApiController
    {
        private readonly DayCareService _rep;

        public InstantLogController()
        {
            _rep = new DayCareService();
        }


        [HttpGet]
        [Authorize]
        [Route("api/instantLog/GetInstantLog/{kidId}/{day}")]
        public IHttpActionResult GetInstantLog(int kidId, string day)
        {
            var instantLog = _rep.GetInstantLog(kidId, day);
            return Ok(instantLog);
        }


        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/instantLog/LogInstantMessage")]
        public IHttpActionResult LogInstantMessage(Message message)
        {
            Message respMessage = _rep.InsertInstantLogMessage(message, message.KidId, message.LogId);
            if (respMessage == null) return NotFound();
            else if (respMessage.Error != string.Empty)
                return Ok(respMessage);
            return Content(HttpStatusCode.InternalServerError, respMessage.Error);
        }
    }
}
