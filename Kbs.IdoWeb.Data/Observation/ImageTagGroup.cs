using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("ImageTagGroup", Schema = "Obs")]
    public partial class ImageTagGroup
    {
        public ImageTagGroup()
        {
            ImageTag = new HashSet<ImageTag>();
        }

        public int ImageTagGroupId { get; set; }
        [StringLength(100)]
        public string ImageTagLocal { get; set; }
        [StringLength(100)]
        public string ImageTagEn { get; set; }

        [InverseProperty("ImageTagGroup")]
        public virtual ICollection<ImageTag> ImageTag { get; set; }
    }
}
