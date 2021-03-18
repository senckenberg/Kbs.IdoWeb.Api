using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Data.Authentication
{
	public class ApplicationUserModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Comment { get; set; }
		public int DataRestrictionId { get; set; }
		public string Role { get; set; }
	}
}
