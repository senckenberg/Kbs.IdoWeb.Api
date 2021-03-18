using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Kbs.IdoWeb.Data.Location
{
	public class LocationItem
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public int Order { get; set; }
		[DataMember]
		public double Lat { get; set; }
		[DataMember]
		public double Lon { get; set; }
	}
}
