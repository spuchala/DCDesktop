using DayCareWebAPI.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;

namespace DayCareWebAPI.Controllers
{
    public class PublicController : ApiController
    {
        private readonly PublicService _rep;

        public PublicController()
        {
            _rep = new PublicService();
        }

        // send contact email
        [HttpPost]
        [AllowAnonymous]
        [Route("api/public/SendContactMessage")]
        public HttpResponseMessage SendContactMessage(PublicContact contact)
        {
            var response = _rep.SendContactEmail(contact);
            return response == string.Empty ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }
    }
}
