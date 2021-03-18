using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("DescriptionKeyType", Schema = "Det")]
    public partial class DescriptionKeyType
    {
        public DescriptionKeyType()
        {
            DescriptionKey = new HashSet<DescriptionKey>();
            TaxonDescription = new HashSet<TaxonDescription>();
        }

        public int DescriptionKeyTypeId { get; set; }
        [StringLength(30)]
        public string DescriptionKeyTypeName { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("DescriptionKeyTypeNavigation")]
        public virtual ICollection<DescriptionKey> DescriptionKey { get; set; }
        [InverseProperty("DescriptionKeyType")]
        public virtual ICollection<TaxonDescription> TaxonDescription { get; set; }
    }
}
