using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("TaxonToRegion", Schema = "Inf")]
    public partial class TaxonToRegion
    {
        public int TaxonRegionId { get; set; }
        public int RegionId { get; set; }
        public int TaxonId { get; set; }
        public int? RegionStateId { get; set; }

        [ForeignKey("RegionId")]
        [InverseProperty("TaxonToRegion")]
        public virtual Region Region { get; set; }
        [ForeignKey("TaxonId")]
        [InverseProperty("TaxonToRegion")]
        public virtual Taxon Taxon { get; set; }
    }
}
