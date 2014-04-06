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

		protected override void OnModelCreating(DbModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
		}
	}
}