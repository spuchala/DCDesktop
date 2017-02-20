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
    public class SettingsController : ApiController
    {
        private readonly DayCareService _rep;

        public SettingsController()
        {
            _rep = new DayCareService();
        }


        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            try
            {
                var settings = _rep.GetSettings(id);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _rep.InsertError(ex.InnerException.Message, "while getting settings for daycare", id.ToString(), ex.StackTrace);
                return Ok(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/settings/CheckCustomReportExists/{dayCareId}")]
        public IHttpActionResult CheckCustomReportExists(Guid dayCareId)
        {
            return Ok(_rep.CheckCustomReportExists(dayCareId));
        }

        // GET app version
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/settings/CheckAppVersion")]
        public IHttpActionResult CheckAppVersion(App app)
        {
            return Ok(_rep.CheckAppVersion(app.OsType, app.Version));
        }


        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Post(Settings settings)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            var resp = _rep.SaveSettings(settings);
            var response = string.IsNullOrEmpty(resp) ? Request.CreateResponse(HttpStatusCode.Created, settings) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, resp);
            return response;
        }
    }
}
