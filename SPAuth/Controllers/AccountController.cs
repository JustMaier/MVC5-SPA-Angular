using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using SPAuth.Models;
using SPAuth.Providers;
using SPAuth.Web;
using System.Threading;
using System.Net;

namespace SPAuth.Controllers {
	[Authorize]
	[RoutePrefix("api/Account")]
	public class AccountController : ApiController {

		#region Base
		private const string LocalLoginProvider = "Local";
		public AccountController() { }
		public AccountController(ApplicationUserManager userManager) {
			UserManager = userManager;
		}

		private ApplicationUserManager _userManager;
		public ApplicationUserManager UserManager {
			get {
				return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set {
				_userManager = value;
			}
		}

		private SignInHelper _helper;
		private SignInHelper SignInHelper {
			get {
				if (_helper == null) {
					_helper = new SignInHelper(UserManager, Authentication);
				}
				return _helper;
			}
		}
		#endregion

		#region Login/Logout
		// GET api/Account/UserInfo
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("UserInfo")]
		public async Task<UserInfoViewModel> GetUserInfo() {
			User user = null;
			var loginInfo = await Authentication.GetExternalLoginInfoAsync();
			
			if (loginInfo == null) {
				user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			}

			return new UserInfoViewModel {
				UserName = user == null ? loginInfo.Email : user.UserName,
				HasRegistered = loginInfo == null,
				LoginProvider = loginInfo != null ? loginInfo.Login.LoginProvider : null
			};
		}

		// POST api/Account/Logout
		[Route("Logout")]
		public IHttpActionResult Logout() {
			Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
			return Ok();
		}
		#endregion

		#region Reset Password
		// POST api/Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[Route("ForgotPassword")]
		public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model) {
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var user = await UserManager.FindByNameAsync(model.Email);
			if (user == null) {
				// Don't reveal that the user does not exist or is not confirmed
				ModelState.AddModelError("", "That user does not exist");
				return BadRequest(ModelState);
			}

			var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
			var callbackUrl = Utility.AbsoluteUrl("/ResetPassword?code="+HttpUtility.UrlEncode(code));
			await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
			return Ok(new { message = "We've emailed you a link to reset your password!" });
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[Route("ResetPassword")]
		public async Task<IHttpActionResult> ResetPassword(ResetPasswordViewModel model) {
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var user = await UserManager.FindByNameAsync(model.Email);

			if (user != null) { 
				var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
				IHttpActionResult errorResult = GetErrorResult(result);
				if (errorResult != null) return errorResult;
			}
			
			return Ok(new { message = "Your Password has been reset." });
		}
		#endregion

		#region Unused
		// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
		[Route("ManageInfo")]
		public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false) {
			User user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null) {
				return null;
			}

			List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

			foreach (IdentityUserLogin linkedAccount in user.Logins) {
				logins.Add(new UserLoginInfoViewModel {
					LoginProvider = linkedAccount.LoginProvider,
					ProviderKey = linkedAccount.ProviderKey
				});
			}

			if (user.PasswordHash != null) {
				logins.Add(new UserLoginInfoViewModel {
					LoginProvider = LocalLoginProvider,
					ProviderKey = user.UserName,
				});
			}

			return new ManageInfoViewModel {
				LocalLoginProvider = LocalLoginProvider,
				UserName = user.UserName,
				Logins = logins,
				ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
			};
		}

		// POST api/Account/SetPassword
		[Route("SetPassword")]
		public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
			IHttpActionResult errorResult = GetErrorResult(result);

			if (errorResult != null) {
				return errorResult;
			}

			return Ok();
		}

		// POST api/Account/AddExternalLogin
		[Route("AddExternalLogin")]
		public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

			AuthenticationTicket ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(model.ExternalAccessToken);

			if (ticket == null || ticket.Identity == null || (ticket.Properties != null
				&& ticket.Properties.ExpiresUtc.HasValue
				&& ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow)) {
				return BadRequest("External login failure.");
			}

			ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

			if (externalData == null) {
				return BadRequest("The external login is already associated with an account.");
			}

			IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
				new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

			IHttpActionResult errorResult = GetErrorResult(result);

			if (errorResult != null) {
				return errorResult;
			}

			return Ok();
		}

		// POST api/Account/RemoveLogin
		[Route("RemoveLogin")]
		public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			IdentityResult result;

			if (model.LoginProvider == LocalLoginProvider) {
				result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
			} else {
				result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
					new UserLoginInfo(model.LoginProvider, model.ProviderKey));
			}

			IHttpActionResult errorResult = GetErrorResult(result);

			if (errorResult != null) {
				return errorResult;
			}

			return Ok();
		}
		#endregion

		#region Manage Account
		// POST api/Account/ChangePassword
		[Route("ChangePassword")]
		public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
				model.NewPassword);
			IHttpActionResult errorResult = GetErrorResult(result);

			if (errorResult != null) {
				return errorResult;
			}

			return Ok();
		}
		#endregion

		#region Third Party Auth
		//
		// GET api/Account/ExternalLogin
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
		[AllowAnonymous]
		[Route("ExternalLogin", Name = "ExternalLogin")]
		public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null) {
			if (error != null) {
				return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
			}

			if (!User.Identity.IsAuthenticated) {
				return new ChallengeResult(provider, this);
			}

			var loginInfo = await Authentication.GetExternalLoginInfoAsync();
			if (loginInfo == null) {
				return InternalServerError();
			}

			if (loginInfo.Login.LoginProvider != provider) {
				Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
				return new ChallengeResult(provider, this);
			}

			// Sign in the user with this external login provider if the user already has a login
			var result = await SignInHelper.ExternalSignIn(loginInfo, isPersistent: false);
			switch (result) {
				case SignInStatus.Success:
					return Ok();
				case SignInStatus.LockedOut:
					return BadRequest("Your account has been locked");
				case SignInStatus.RequiresTwoFactorAuthentication:
					return Ok(new { twoFactor = true });
				case SignInStatus.Failure:
				default:
					// If the user does not have an account, then prompt the user to create an account
					ClaimsIdentity identity = new ClaimsIdentity(loginInfo.ExternalIdentity.Claims, OAuthDefaults.AuthenticationType);
					Authentication.SignIn(identity);
					return Ok(new { newLogin = true });
			}
		}

		// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
		[AllowAnonymous]
		[Route("ExternalLogins")]
		public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false) {
			IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
			List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

			string state;

			if (generateState) {
				const int strengthInBits = 256;
				state = RandomOAuthStateGenerator.Generate(strengthInBits);
			} else {
				state = null;
			}

			foreach (AuthenticationDescription description in descriptions) {
				ExternalLoginViewModel login = new ExternalLoginViewModel {
					Name = description.Caption,
					Url = Url.Route("ExternalLogin", new {
						provider = description.AuthenticationType,
						response_type = "token",
						client_id = Startup.PublicClientId,
						redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
						state = state
					}),
					State = state
				};
				logins.Add(login);
			}

			return logins;
		}

		// POST api/Account/RegisterExternal
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("RegisterExternal")]
		public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model) {
			if (!ModelState.IsValid) {
				return BadRequest(ModelState);
			}

			var info = await Authentication.GetExternalLoginInfoAsync();
			if (info == null) {
				return BadRequest("No third party auth token found");
			}
			var user = new User {
				UserName = model.UserName,
				Email = model.UserName
			};
			var result = await UserManager.CreateAsync(user);

			IHttpActionResult errorResult = GetErrorResult(result);
			if (errorResult != null) return errorResult;

			if (result.Succeeded) {
				result = await UserManager.AddLoginAsync(user.Id, info.Login);

				errorResult = GetErrorResult(result);
				if (errorResult != null) return errorResult;
			}

			return Ok();
		}
		#endregion

		#region Register
		// POST api/Account/Register
		[AllowAnonymous]
		[Route("Register")]
		public async Task<IHttpActionResult> Register(RegisterBindingModel model) {
			if (!ModelState.IsValid)  return BadRequest(ModelState);

			User user = new User {
				UserName = model.UserName,
				Email = model.UserName
			};

			IdentityResult result = await UserManager.CreateAsync(user, model.Password);

			IHttpActionResult errorResult = GetErrorResult(result);
			if (errorResult != null) return errorResult;
			
			if (result.Succeeded) {
				var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
				var callbackUrl = Utility.AbsoluteUrl(string.Format("/ConfirmEmail?code={0}&userId={1}",HttpUtility.UrlEncode(code),HttpUtility.UrlEncode(user.Id)));
				await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
			}

			return Ok();
		}

		//
		// GET api/Account/ConfirmEmail
		[AllowAnonymous]
		[Route("ConfirmEmail")]
		[HttpGet]
		public async Task<IHttpActionResult> ConfirmEmail(string userId, string code) {
			if (userId == null || code == null)  return BadRequest("Missing email confirmation token");

			var result = await UserManager.ConfirmEmailAsync(userId, code);
			IHttpActionResult errorResult = GetErrorResult(result);
			if (errorResult != null) return errorResult;

			return Ok(new { message = "Your email has been confirmed" });
		}

		#endregion

		#region Helpers
		private IAuthenticationManager Authentication {
			get { return Request.GetOwinContext().Authentication; }
		}

		private IHttpActionResult GetErrorResult(IdentityResult result) {
			if (result == null) {
				return InternalServerError();
			}

			if (!result.Succeeded) {
				if (result.Errors != null) {
					foreach (string error in result.Errors) {
						ModelState.AddModelError("", error);
					}
				}

				if (ModelState.IsValid) {
					// No ModelState errors are available to send, so just return an empty BadRequest.
					return BadRequest();
				}

				return BadRequest(ModelState);
			}

			return null;
		}

		public class ChallengeResult : IHttpActionResult {
			public ChallengeResult(string loginProvider, ApiController controller) {
				LoginProvider = loginProvider;
				Request = controller.Request;
			}

			public string LoginProvider { get; set; }
			public HttpRequestMessage Request { get; set; }

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) {
				Request.GetOwinContext().Authentication.Challenge(LoginProvider);

				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
				response.RequestMessage = Request;
				return Task.FromResult(response);
			}
		}

		private class ExternalLoginData {
			public string LoginProvider { get; set; }
			public string ProviderKey { get; set; }
			public string UserName { get; set; }

			public IList<Claim> GetClaims() {
				IList<Claim> claims = new List<Claim>();
				claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

				if (UserName != null) {
					claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
				}

				return claims;
			}

			public static ExternalLoginData FromIdentity(ClaimsIdentity identity) {
				if (identity == null) {
					return null;
				}

				Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

				if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
					|| String.IsNullOrEmpty(providerKeyClaim.Value)) {
					return null;
				}

				if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer) {
					return null;
				}

				return new ExternalLoginData {
					LoginProvider = providerKeyClaim.Issuer,
					ProviderKey = providerKeyClaim.Value,
					UserName = identity.FindFirstValue(ClaimTypes.Name)
				};
			}
		}

		private static class RandomOAuthStateGenerator {
			private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

			public static string Generate(int strengthInBits) {
				const int bitsPerByte = 8;

				if (strengthInBits % bitsPerByte != 0) {
					throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
				}

				int strengthInBytes = strengthInBits / bitsPerByte;

				byte[] data = new byte[strengthInBytes];
				_random.GetBytes(data);
				return HttpServerUtility.UrlTokenEncode(data);
			}
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				UserManager.Dispose();
			}

			base.Dispose(disposing);
		}
		#endregion
	}
}
