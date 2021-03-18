using System;
using System.Collections.Generic;
using System.Text;

namespace Kbs.IdoWeb.Data.Authentication
{
	public class UpdateUserModel: ApplicationUser
	{
		public string UserRoles { get; set; }
	}
}
