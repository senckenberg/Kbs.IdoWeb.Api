using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("TaxonDescription", Schema = "Det")]
    public partial class TaxonDescription
    {
        public int TaxonDescriptionId { get; set; }
        public int TaxonId { get; set; }
        public int DescriptionKeyId { get; set; }
        [StringLength(100)]
        public string KeyValue { get; set; }
        public int? DescriptionKeyTypeId { get; set; }
        [Column(TypeName = "numeric(7,2)")]
        public decimal? MinValue { get; set; }
        [Column(TypeName = "numeric(7,2)")]
        public decimal? MaxValue { get; set; }

        [ForeignKey("DescriptionKeyId")]
        [InverseProperty("TaxonDescription")]
        public virtual DescriptionKey DescriptionKey { get; set; }
        [ForeignKey("DescriptionKeyTypeId")]
        [InverseProperty("TaxonDescription")]
        public virtual DescriptionKeyType DescriptionKeyType { get; set; }
    }
}
