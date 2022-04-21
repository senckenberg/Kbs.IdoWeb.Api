using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Data.Authentication
{
	public class ApplicationSettings
	{
		public string JWT_Secret { get; set; }
		public string CNC_Secret { get; set; }

	}
}
