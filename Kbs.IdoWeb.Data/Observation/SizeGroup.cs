using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("SizeGroup", Schema = "Obs")]
    public partial class SizeGroup
    {
        public SizeGroup()
        {
            Observation = new HashSet<Observation>();
        }

        public int SizeGroupId { get; set; }
        [Required]
        [StringLength(15)]
        public string SizeGroupName { get; set; }

        [InverseProperty("SizeGroup")]
        public virtual ICollection<Observation> Observation { get; set; }
    }
}
