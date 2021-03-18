using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("LocalityType", Schema = "Obs")]
    public partial class LocalityType
    {
        public LocalityType()
        {
            Observation = new HashSet<Observation>();
        }

        public int LocalityTypeId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("LocalityType")]
        public virtual ICollection<Observation> Observation { get; set; }
    }
}
