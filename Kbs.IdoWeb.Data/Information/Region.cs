using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("Region", Schema = "Inf")]
    public partial class Region
    {
        public Region()
        {
            InverseSubRegionOf = new HashSet<Region>();
            TaxonToRegion = new HashSet<TaxonToRegion>();
        }

        public int RegionId { get; set; }
        public int? SubRegionOfId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [ForeignKey("SubRegionOfId")]
        [InverseProperty("InverseSubRegionOf")]
        public virtual Region SubRegionOf { get; set; }
        [InverseProperty("SubRegionOf")]
        public virtual ICollection<Region> InverseSubRegionOf { get; set; }
        [InverseProperty("Region")]
        public virtual ICollection<TaxonToRegion> TaxonToRegion { get; set; }
    }
}
