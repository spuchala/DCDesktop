using DayCareWebAPI.Repository;
using DayCareWebAPI.Services;
using System;
using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using System.Net;

namespace DayCareWebAPI.Controllers
{
    public class AlexaController : ApiController
    {
        ////development
        //[AllowAnonymous]
        //[HttpPost]
        //public HttpResponseMessage Post()
        //{
        //    try
        //    {
        //        var speechlet = new AlexaService();                
        //        speechlet.Id = new Guid("53ca637a-95e7-e511-9af4-782bcb6d9a11");
        //        speechlet.Role = Constants.DayCareRole;
        //        speechlet.IsDev = true;
        //        return speechlet.GetResponse(Request);
        //    }
        //    catch (Exception ex)
        //    {
        //        var repo = new DayCareRepository();
        //        repo.LogError(ex.Message, "alexa process failed", "test", ex.StackTrace);
        //        return null;
        //    }
        //}

        //production
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Post()
        {           
            try
            {
                var speechlet = new AlexaService();
                speechlet.IsDev = false;                
                return speechlet.GetResponse(Request);
            }
            catch (Exception ex)
            {
                var repo = new DayCareRepository();
                repo.LogError(ex.Message, "alexa process failed", User.Identity.Name, ex.StackTrace);
                return null;
            }
        }

        // log alexa token
        [HttpPost]
        [Authorize]
        [Route("api/alexa/LogToken")]
        public HttpResponseMessage LogToken(Token token)
        {
            var serv = new DayCareService();
            serv.InsertAlexaToken(token);
            return Request.CreateResponse(HttpStatusCode.Created);
        }
    }
}
