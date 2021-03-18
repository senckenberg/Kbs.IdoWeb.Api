using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kbs.IdoWeb.Data.Location
{
	public partial class LocationContext : DbContext
	{
		public LocationContext()
		{

		}
		public LocationContext(DbContextOptions<LocationContext> options) : base(options)
		{
		}
		public virtual DbSet<LocationItem> LocationItem { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseNpgsql("Name=LocationConnection");
			}
		}
	}
}