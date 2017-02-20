using DayCareWebAPI.Repository;
using DayCareWebAPI.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using DayCareWebAPI.Models;

namespace DayCareWebAPI.Controllers
{
    public class UploadController : ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            //get day care id
            IEnumerable<string> headerValues;
            var dayCareId = string.Empty;
            if (Request.Headers.TryGetValues("DayCareId", out headerValues))
            {
                dayCareId = headerValues.FirstOrDefault();
            }

            //check if the daycareid folder exists
            var dayCareFolder = HttpContext.Current.Server.MapPath("~/ftp/snaps/" + dayCareId);
            var docuServ = new DocumentService();
            var path = string.Format(Constants.FtpSnapsPath, dayCareId);
            if (!docuServ.CheckDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd))
            {
                docuServ.CreateDocuDirectory(path, Constants.FtpUser, Constants.FtpPswd);
            }

            //check file limit
            if (docuServ.CheckFileCountLimit(path, Constants.FtpUser, Constants.FtpPswd) > 4)
            {
                var resp = new DayCareSnap() { Error = "Sorry! Maximum of five snaps allowed." };
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }

            // Check whether the POST operation is MultiPart?
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // Prepare CustomMultipartFormDataStreamProvider in which our multipart form
            // data will be loaded.            
            CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(dayCareFolder);
            List<string> files = new List<string>();

            try
            {
                // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (MultipartFileData file in provider.FileData)
                {
                    files.Add(Path.GetFileName(file.LocalFileName));
                }
                var _repo = new DayCareRepository();
                var response = _repo.InsertDayCareSnap(new Guid(dayCareId), files[0]);
                //change file name and extension on server
                var filePath = dayCareFolder + "/" + files[0];
                Path.ChangeExtension(filePath, ".jpeg");
                // Send OK Response along with saved file names to the client.
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                var fileName = headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                fileName = fileName.Contains(".") ?
                    fileName.Replace("%3A", "") : (fileName.Replace("%3A", "") + ".jpeg");
                return fileName;
            }
        }
    }
}
