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
    public class ReportsController : ApiController
    {
        private readonly DayCareService _rep;
        public ReportsController()
        {
            _rep = new DayCareService();
        }

        // GET: api/Reports
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Reports/5
        [Authorize]
        public IHttpActionResult Get(Report id)
        {
            List<Kid> kids;
            if (id.UserRole == Constants.ParentRole)
            {
                kids = _rep.GetKidsFromDayCare(id.Id);
                return Ok(kids);
            }
            kids = _rep.GetKidsFromParent(id.Id);
            return Ok(kids);
        }

        // POST: api/Reports
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Reports/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Reports/5
        public void Delete(int id)
        {
        }
    }
}
