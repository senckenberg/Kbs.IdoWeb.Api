using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("LastSyncVersion", Schema = "Obs")]
    public partial class LastSyncVersion
    {
        [Column(TypeName = "character varying")]
        public string SyncTypeName { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        [Column(TypeName = "date")]
        public DateTime? VersionDate { get; set; }
    }
}
