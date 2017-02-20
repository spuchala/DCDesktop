using DayCareWebAPI.Services;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace DayCareWebAPI.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //use below line for production
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            var rep = new DayCareService();
            //check user here
            var password = context.UserName.Split(';')[1];
            var userName = context.UserName.Split(';')[0];
            var user = rep.LoginUser(userName, password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }
            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    {
                        "id", user.Id.ToString()
                    },
                    {
                        "userName", user.FName+" "+user.LName
                    },
                    {
                        "userPhone", user.Phone
                    },
                    {
                        "userRole", user.Role
                    }
                });
            props.IssuedUtc = DateTime.UtcNow;
            props.ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromDays(365));
            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.FromResult<object>(null);
        }

        public override Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            if (context.IsTokenEndpoint && (context.Request.Method == "OPTIONS"))
            {
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "authorization", "codeVersion" });
                context.RequestCompleted();
                return Task.FromResult(0);
            }
            //else if (!context.IsTokenEndpoint && (context.Request.Method == "GET"))
            //{
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
            //    HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
            //    HttpContext.Current.Response.End();
            //}
            return base.MatchEndpoint(context);
        }
    }
}