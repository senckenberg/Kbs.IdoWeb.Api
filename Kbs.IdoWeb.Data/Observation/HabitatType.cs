using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("HabitatType", Schema = "Obs")]
    public partial class HabitatType
    {
        public HabitatType()
        {
            Event = new HashSet<Event>();
        }

        public int HabitatTypeId { get; set; }
        public int Code { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("HabitatType")]
        public virtual ICollection<Event> Event { get; set; }
    }
}
