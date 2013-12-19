using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SPAuth.Models {
	// Models used as parameters to AccountController actions.
	#region AccountBindingModels
	public class AddExternalLoginBindingModel {
		[Required]
		[Display(Name = "External access token")]
		public string ExternalAccessToken { get; set; }
	}

	public class ChangePasswordBindingModel {
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class RegisterBindingModel {
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class RegisterExternalBindingModel {
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }
	}

	public class RemoveLoginBindingModel {
		[Required]
		[Display(Name = "Login provider")]
		public string LoginProvider { get; set; }

		[Required]
		[Display(Name = "Provider key")]
		public string ProviderKey { get; set; }
	}

	public class SetPasswordBindingModel {
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}
	#endregion

	// Models returned by AccountController actions.
	#region AccountViewModels
	public class ExternalLoginViewModel {
		public string Name { get; set; }

		public string Url { get; set; }

		public string State { get; set; }
	}

	public class ManageInfoViewModel {
		public string LocalLoginProvider { get; set; }

		public string UserName { get; set; }

		public IEnumerable<UserLoginInfoViewModel> Logins { get; set; }

		public IEnumerable<ExternalLoginViewModel> ExternalLoginProviders { get; set; }
	}

	public class UserInfoViewModel {
		public string UserName { get; set; }

		public bool HasRegistered { get; set; }

		public string LoginProvider { get; set; }
	}

	public class UserLoginInfoViewModel {
		public string LoginProvider { get; set; }

		public string ProviderKey { get; set; }
	}
	#endregion
}