using DayCareWebAPI.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;

namespace DayCareWebAPI.Controllers
{
    public class WhatsNewController : ApiController
    {
        private readonly DayCareService _rep;

        public WhatsNewController()
        {
            _rep = new DayCareService();
        }

        // GET number of notifications
        [HttpGet]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/whatsNew/GetCount/{dayCareId}")]
        public IHttpActionResult GetCount(Guid dayCareId)
        {
            var news = _rep.GetWhatsNew(dayCareId);
            return Ok(news != null && news.Any() ? news.Count() : 0);
        }

        // GET all new notifications for home page
        [HttpGet]
        [AllowAnonymous]
        [Route("api/whatsNew/GetNew/")]
        public IHttpActionResult GetNew()
        {
            var news = _rep.GetAllWhatsNew();
            return Ok(news);
        }

        // GET all notifications
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            var news = _rep.GetWhatsNew(id);
            return Ok(news);
        }

        // remove kid
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/whatsNew/RemoveWhatsNew/{dayCareId}/{id}")]
        public HttpResponseMessage RemoveWhatsNew(Guid dayCareId, int id)
        {
            var response = _rep.RemoveWhatsNew(dayCareId, id);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
