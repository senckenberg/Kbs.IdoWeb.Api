using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("ApprovalState", Schema = "Obs")]
    public partial class ApprovalState
    {
        public ApprovalState()
        {
            Event = new HashSet<Event>();
            Observation = new HashSet<Observation>();
        }

        public int ApprovalStateId { get; set; }
        [StringLength(20)]
        public string ApprovalStateName { get; set; }
        [StringLength(75)]
        public string ApprovalStateDisplayName { get; set; }

        [InverseProperty("ApprovalState")]
        public virtual ICollection<Event> Event { get; set; }
        [InverseProperty("ApprovalState")]
        public virtual ICollection<Observation> Observation { get; set; }
    }
}
