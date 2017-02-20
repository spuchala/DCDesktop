using System.Net.Http;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using System.Net;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;

namespace DayCareWebAPI.Controllers
{
    public class DocumentController : ApiController
    {
        private readonly DocumentService _rep;

        public DocumentController()
        {
            _rep = new DocumentService();
        }

        // GET all documents
        [Authorize(Roles = Constants.DayCareRole)]
        public IHttpActionResult Get(Guid id)
        {
            var docs = _rep.GetDocuments(id);
            return Ok(docs);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/document/DownloadDoc/{id}")]
        public HttpResponseMessage DownloadDoc(int id)
        {
            return _rep.DownloadDocument(id);
        }

        //remove document
        [HttpPut]
        [Authorize(Roles = Constants.DayCareRole)]
        public HttpResponseMessage Put(int id)
        {
            var response = _rep.DeleteDocument(id);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        //send email
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/document/SendEmail")]
        public HttpResponseMessage SendEmail(Models.Document data)
        {
            var _dServ = new DayCareService();
            var response = _dServ.SendDocumentEmail(data);
            return string.IsNullOrEmpty(response) ? Request.CreateResponse(HttpStatusCode.Created) :
                                               Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        // upload the document
        [HttpPost]
        [Authorize(Roles = Constants.DayCareRole)]
        [Route("api/document/UploadDocument")]
        public async Task<IHttpActionResult> UploadDocument()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Content(HttpStatusCode.InternalServerError, "Not a valid document.");
            }
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                var file = provider.Contents[0];
                var type = file.Headers.ContentType.MediaType;
                var fileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                var title = provider.Contents[1].ReadAsStringAsync().Result;
                var dayCareId = provider.Contents[2].ReadAsStringAsync().Result;
                Stream docData = await file.ReadAsStreamAsync();
                var response = _rep.SaveDocument(docData, title, fileName, type, new Guid(dayCareId), file.Headers.ContentLength);
                if (response.Error == string.Empty)
                    return Ok(response);
                else
                    return Content(HttpStatusCode.InternalServerError, response.Error);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, Constants.Error);
            }
        }
    }
}
