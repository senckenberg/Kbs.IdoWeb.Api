using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("Event", Schema = "Obs")]
    public partial class Event
    {
        public Event()
        {
            Observation = new HashSet<Observation>();
        }

        public int EventId { get; set; }
        [StringLength(100)]
        public string LocalityName { get; set; }
        public string HabitatDescription { get; set; }
        public string Geom { get; set; }
        public string GeomPoly { get; set; }
        public string GeomTransect { get; set; }
        public int? AccuracyId { get; set; }
        public int? ProjectId { get; set; }
        [StringLength(100)]
        public string AuthorName { get; set; }
        public int? TkNr { get; set; }
        [Column(TypeName = "numeric(12,7)")]
        public decimal? LongitudeDecimal { get; set; }
        [Column(TypeName = "numeric(12,7)")]
        public decimal? LatitudeDecimal { get; set; }
        public string UserId { get; set; }
        public int? ApprovalStateId { get; set; }
        public int? HabitatTypeId { get; set; }
        public int? RegionId { get; set; }
        public int? CountryId { get; set; }
        public string EditorComment { get; set; }

        [ForeignKey("AccuracyId")]
        [InverseProperty("Event")]
        public virtual AccuracyType Accuracy { get; set; }
        [ForeignKey("ApprovalStateId")]
        [InverseProperty("Event")]
        public virtual ApprovalState ApprovalState { get; set; }
        [ForeignKey("HabitatTypeId")]
        [InverseProperty("Event")]
        public virtual HabitatType HabitatType { get; set; }
        [JsonIgnore]
        [InverseProperty("Event")]
        public virtual ICollection<Observation> Observation { get; set; }
    }
}
