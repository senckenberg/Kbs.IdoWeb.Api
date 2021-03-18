using System.ComponentModel.DataAnnotations;

namespace Kbs.IdoWeb.Data.Authentication
{
	public class RoleModel
	{
		public string Email { get; set; }
		public string Roles { get; set; }
		public RoleModel(string email, string roles)
		{
			Email = email;
			Roles = roles;
		}
	}
}
