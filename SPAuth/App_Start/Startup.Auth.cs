using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SPAuth.Providers;
using SPAuth.Models;

namespace SPAuth {
	public partial class Startup {
		public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

		public static string PublicClientId { get; private set; }

		// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
		public void ConfigureAuth(IAppBuilder app) {
			// Configure the db context, user manager and role manager to use a single instance per request
			app.CreatePerOwinContext(AppContext.Create);
			app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
			app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

			// Enable the application to use a cookie to store information for the signed in user
			// and to use a cookie to temporarily store information about a user logging in with a third party login provider
			app.UseCookieAuthentication(new CookieAuthenticationOptions());
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			// Enable the application to use bearer tokens to authenticate users
			PublicClientId = "self";
			OAuthOptions = new OAuthAuthorizationServerOptions {
				TokenEndpointPath = new PathString("/Token"),
				Provider = new ApplicationOAuthProvider(PublicClientId),
				AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
				AllowInsecureHttp = true
			};
			app.UseOAuthBearerTokens(OAuthOptions);

			// Uncomment the following lines to enable logging in with third party login providers
			//app.UseMicrosoftAccountAuthentication(
			//	clientId: "",
			//	clientSecret: "");

			//app.UseTwitterAuthentication(
			//	consumerKey: "",
			//	consumerSecret: "");

			//app.UseFacebookAuthentication(
			//	appId: "",
			//	appSecret: "");

			app.UseGoogleAuthentication();
		}
	}
}
