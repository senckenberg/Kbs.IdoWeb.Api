/* cc by-sa 4.0
 * stackoverflow.com
 * (c) Darxtar, lolol
 */

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Middleware
{
	public class JwtInHeaderMiddleware
	{
		private readonly RequestDelegate _next;
		public JwtInHeaderMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task Invoke(HttpContext context)
		{
			var name = "JWTToken";
			var cookie = context.Request.Cookies[name];
			if (cookie != null)
			{
				if (!context.Request.Headers.ContainsKey("Authorization"))
				{
					context.Request.Headers.Append("Authorization", "Bearer " + cookie);
				}
			}
			await _next.Invoke(context);
		}
	}
}
