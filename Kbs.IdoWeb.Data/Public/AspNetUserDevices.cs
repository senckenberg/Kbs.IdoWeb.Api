using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Public
{
    public partial class AspNetUserDevices
    {
        [Key]
        public Guid DeviceGuid { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceHash { get; set; }
        [Column(TypeName = "date")]
        public DateTime? LastAccess { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("AspNetUserDevices")]
        public virtual AspNetUsers User { get; set; }
    }
}
