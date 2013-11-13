using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace SPAuth {
	public class BundleConfig {
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles) {
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
				"~/Scripts/jquery-{version}.js"));

			//Angular
			bundles.Add(new ScriptBundle("~/bundles/angular").Include(
					   "~/Scripts/angular.js",
					   "~/Scripts/angular-sanitize.js",
					   "~/Scripts/angular-resource.js",
					   "~/Scripts/angular-route.js"
					   ));

			//Angular Bootstrap
			bundles.Add(new ScriptBundle("~/bundles/angularBootstrap").Include(
					   "~/Scripts/ui-bootstrap-tpls-{version}.js",
					   "~/Scripts/angular-spa-security.js",
					   "~/Scripts/autoFields-bootstrap.js"
					   ));

			//App
			bundles.Add(new ScriptBundle("~/bundles/app")
				.IncludeDirectory("~/Scripts/App/", "*.js")
				.IncludeDirectory("~/Scripts/App/Controllers", "*.js")
				.IncludeDirectory("~/Scripts/App/Directives", "*.js")
				.IncludeDirectory("~/Scripts/App/Filters", "*.js")
				.IncludeDirectory("~/Scripts/App/Services", "*.js")
				);

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
				"~/Scripts/modernizr-*"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
				 "~/Content/css/bootstrap.css",
				 "~/Content/css/Site.css"));
		}
	}
}
