using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SPAuth.Models {
	public class AppContext : IdentityDbContext<User> {
		public AppContext()
			: base("DefaultConnection") {
		}

		//Db Sets
		//public virtual DbSet<Partner> Partners { get; set; }

		static AppContext() {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
			Database.SetInitializer<AppContext>(new ApplicationDbInitializer());
        }

		public static AppContext Create() {
			return new AppContext();
        }
	}
}