using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("AccuracyType", Schema = "Obs")]
    public partial class AccuracyType
    {
        public AccuracyType()
        {
            Event = new HashSet<Event>();
        }

        public int AccuracyTypeId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("Accuracy")]
        public virtual ICollection<Event> Event { get; set; }
    }
}
