using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("ImageLicense", Schema = "Obs")]
    public partial class ImageLicense
    {
        public ImageLicense()
        {
            Image = new HashSet<Image>();
        }

        public int LicenseId { get; set; }
        [Required]
        [StringLength(30)]
        public string LicenseName { get; set; }
        [StringLength(100)]
        public string LicenseLink { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("License")]
        public virtual ICollection<Image> Image { get; set; }
    }
}
