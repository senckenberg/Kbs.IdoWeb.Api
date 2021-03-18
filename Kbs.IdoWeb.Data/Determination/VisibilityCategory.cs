using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("VisibilityCategory", Schema = "Det")]
    public partial class VisibilityCategory
    {
        public VisibilityCategory()
        {
            DescriptionKeyGroup = new HashSet<DescriptionKeyGroup>();
        }

        public int VisibilityCategoryId { get; set; }
        public string VisibilityCategoryName { get; set; }
        public string DisplayName { get; set; }

        [InverseProperty("VisibilityCategory")]
        public virtual ICollection<DescriptionKeyGroup> DescriptionKeyGroup { get; set; }
    }
}
