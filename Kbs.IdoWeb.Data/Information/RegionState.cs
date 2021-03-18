using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("RegionState", Schema = "Inf")]
    public partial class RegionState
    {
        public int RegionStateId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }
    }
}
