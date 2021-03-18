using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Authentication
{
	public class ApplicationUser : IdentityUser
	{
		[Column(TypeName ="VARCHAR(150)")]
		public string FirstName { get; set; }
		[Column(TypeName = "VARCHAR(150)")]
		public string LastName { get; set; }
		[Column(TypeName = "TEXT")]
		public string Comment { get; set; }
		[Column(TypeName = "INT")]
		public int DataRestrictionId { get; set; }

		//add ClientId
	}

}
