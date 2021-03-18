using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("DescriptionKey", Schema = "Det")]
    public partial class DescriptionKey
    {
        public DescriptionKey()
        {
            DescriptionStep = new HashSet<DescriptionStep>();
            InverseParentDescriptionKey = new HashSet<DescriptionKey>();
            TaxonDescription = new HashSet<TaxonDescription>();
        }

        public int DescriptionKeyId { get; set; }
        public int DescriptionKeyGroupId { get; set; }
        public string KeyName { get; set; }
        public string KeyDescription { get; set; }
        public int? ParentDescriptionKeyId { get; set; }
        [Column(TypeName = "jsonb")]
        public string ListSourceJson { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }
        public int? DescriptionKeyType { get; set; }

        [ForeignKey("DescriptionKeyGroupId")]
        [InverseProperty("DescriptionKey")]
        [JsonIgnore]
        public virtual DescriptionKeyGroup DescriptionKeyGroup { get; set; }
        [ForeignKey("DescriptionKeyType")]
        [InverseProperty("DescriptionKey")]
        public virtual DescriptionKeyType DescriptionKeyTypeNavigation { get; set; }
        [ForeignKey("ParentDescriptionKeyId")]
        [InverseProperty("InverseParentDescriptionKey")]
        public virtual DescriptionKey ParentDescriptionKey { get; set; }
        [InverseProperty("DescriptionKey")]
        public virtual ICollection<DescriptionStep> DescriptionStep { get; set; }
        [InverseProperty("ParentDescriptionKey")]
        public virtual ICollection<DescriptionKey> InverseParentDescriptionKey { get; set; }
        [InverseProperty("DescriptionKey")]
        public virtual ICollection<TaxonDescription> TaxonDescription { get; set; }
    }
}
