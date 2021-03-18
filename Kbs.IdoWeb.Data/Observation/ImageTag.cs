using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("ImageTag", Schema = "Obs")]
    public partial class ImageTag
    {
        public int ImageTagId { get; set; }
        public int ImageTagGroupId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [ForeignKey("ImageTagGroupId")]
        [InverseProperty("ImageTag")]
        public virtual ImageTagGroup ImageTagGroup { get; set; }
    }
}
