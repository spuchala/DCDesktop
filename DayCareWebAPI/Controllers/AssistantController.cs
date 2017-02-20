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
    public class AssistantController : ApiController
    {
        private readonly AssistantService _rep;

        public AssistantController()
        {
            _rep = new AssistantService();
        }

        // POST: analyze the request
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Post(Assistant assistant)
        {
            if (assistant.Message.TrimEnd().Substring(0, assistant.Message.Length - 1) != ".")
            {
                assistant.Message = assistant.Message + ".";
            }
            var output = _rep.Parser(assistant);
            if (string.IsNullOrEmpty(output.Error))
                return Ok(output);
            else
                return Content(HttpStatusCode.InternalServerError, Constants.Error);
        }

        // process the request
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/assistant/ProcessText")]
        public HttpResponseMessage ProcessText(Assistant assistant)
        {
            var response = _rep.Process(assistant);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
