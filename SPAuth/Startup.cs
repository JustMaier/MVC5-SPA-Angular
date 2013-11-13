using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SPAuth.Startup))]

namespace SPAuth {
	public partial class Startup {
		public void Configuration(IAppBuilder app) {
			ConfigureAuth(app);
		}
	}
}
