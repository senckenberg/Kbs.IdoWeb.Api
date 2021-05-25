using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("Image", Schema = "Obs")]
    public partial class Image
    {
        public int ImageId { get; set; }
        public int? TaxonId { get; set; }
        [StringLength(500)]
        public string Author { get; set; }
        [StringLength(500)]
        public string CopyrightText { get; set; }
        public int? LicenseId { get; set; }
        public string Description { get; set; }
        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; }
        public string UserId { get; set; }
        public bool IsApproved { get; set; }
        public int? ObservationId { get; set; }
        public int? CmsId { get; set; }
        [StringLength(100)]
        public string TaxonName { get; set; }
        public int? ImagePriority { get; set; }

        [ForeignKey("LicenseId")]
        [InverseProperty("Image")]
        public virtual ImageLicense License { get; set; }
        [ForeignKey("ObservationId")]
        [InverseProperty("Image")]
        public virtual Observation Observation { get; set; }
    }
}
