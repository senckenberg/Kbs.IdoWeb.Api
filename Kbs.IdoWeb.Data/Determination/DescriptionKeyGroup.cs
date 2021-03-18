using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("DescriptionKeyGroup", Schema = "Det")]
    public partial class DescriptionKeyGroup
    {
        public DescriptionKeyGroup()
        {
            DescriptionKey = new HashSet<DescriptionKey>();
            InverseParentDescriptionKeyGroup = new HashSet<DescriptionKeyGroup>();
        }

        public int DescriptionKeyGroupId { get; set; }
        public string KeyGroupName { get; set; }
        [StringLength(10)]
        public string DescriptionKeyGroupDataType { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }
        public int? ParentDescriptionKeyGroupId { get; set; }
        [Column(TypeName = "jsonb")]
        public string DescriptionKeyGroupType { get; set; }
        public int? VisibilityCategoryId { get; set; }
        public int? OrderPriority { get; set; }

        [ForeignKey("ParentDescriptionKeyGroupId")]
        [InverseProperty("InverseParentDescriptionKeyGroup")]
        public virtual DescriptionKeyGroup ParentDescriptionKeyGroup { get; set; }
        [ForeignKey("VisibilityCategoryId")]
        [InverseProperty("DescriptionKeyGroup")]
        public virtual VisibilityCategory VisibilityCategory { get; set; }
        [InverseProperty("DescriptionKeyGroup")]
        public virtual ICollection<DescriptionKey> DescriptionKey { get; set; }
        [InverseProperty("ParentDescriptionKeyGroup")]
        public virtual ICollection<DescriptionKeyGroup> InverseParentDescriptionKeyGroup { get; set; }
    }
}
