using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using DayCareWebAPI.Models;
using DayCareWebAPI.Services;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;

namespace DayCareWebAPI.Controllers
{
    public class LoginController : ApiController
    {
        private readonly DayCareService _rep;

        public LoginController()
        {
            _rep = new DayCareService();
        }

        [HttpPost]        
        [AllowAnonymous]
        [Route("api/login/LoginUser")]
        public IHttpActionResult LoginUser(User login)
        {
            var user = _rep.LoginUser(login.Email, login.Password);
            if (user != null)
            {
                var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                var tokenExpiration = TimeSpan.FromDays(100);
                var props = new AuthenticationProperties()
                {
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
                };

                var ticket = new AuthenticationTicket(identity, props);
                var tokenResponse = new JObject(
                    new JProperty("id", user.Id),
                    new JProperty("userName", user.LName),
                    new JProperty("userEmail", user.Email),
                    new JProperty("userRole", user.Role),
                    new JProperty("userPhone", user.Phone),
                    new JProperty("access_token", AuthStartUpService.OAuthBearerOptions.AccessTokenFormat.Protect(ticket))
                   );
                return Ok(tokenResponse);
            }
            return BadRequest("The user name or password is incorrect.");
        }
    }
}
