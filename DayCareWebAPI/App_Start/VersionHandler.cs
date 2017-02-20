using DayCareWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DayCareWebAPI.App_Start
{
    public class VersionHandler : DelegatingHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)

        {
            //check version here
            var headers = request.Headers;
            if (headers.Contains("CodeVersion"))
            {
                var clientCodeversion = headers.GetValues("CodeVersion").First();
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CodeVersion"]))
                {
                    if (clientCodeversion != ConfigurationManager.AppSettings["CodeVersion"].ToString())
                    {
                        //return 205 error to client and end it and cold refresh the page
                        return request.CreateResponse(HttpStatusCode.ExpectationFailed, Constants.CodeChange);
                    }
                }
            }
            // Call the inner handler.
            return await base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {
                return task.Result;
            }, cancellationToken);
        }
    }
}
