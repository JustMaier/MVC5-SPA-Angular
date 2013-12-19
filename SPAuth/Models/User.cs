using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPAuth.Models {
	public class User : IdentityUser {
		public string email { get; set; }
	}
}