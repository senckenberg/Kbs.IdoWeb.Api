using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("Observation", Schema = "Obs")]
    public partial class Observation
    {
        public Observation()
        {
            Image = new HashSet<Image>();
        }

        public int ObservationId { get; set; }
        public int TaxonId { get; set; }
        public int EventId { get; set; }
        [Column(TypeName = "date")]
        public DateTime HabitatDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime? HabitatDateTo { get; set; }
        public int? AdviceCount { get; set; }
        public int? MaleCount { get; set; }
        public int? FemaleCount { get; set; }
        public int? SizeGroupId { get; set; }
        public int? DiagnosisTypeId { get; set; }
        public int? LocalityTypeId { get; set; }
        public string ObservationComment { get; set; }
        public int? JuvenileCount { get; set; }
        public string UserId { get; set; }
        public int ApprovalStateId { get; set; }
        [StringLength(100)]
        public string TaxonName { get; set; }
        [StringLength(100)]
        public string AuthorName { get; set; }
        public string EditorComment { get; set; }
        [Column(TypeName = "date")]
        public DateTime? LastEditDate { get; set; }

        [ForeignKey("ApprovalStateId")]
        [InverseProperty("Observation")]
        public virtual ApprovalState ApprovalState { get; set; }
        [ForeignKey("DiagnosisTypeId")]
        [InverseProperty("Observation")]
        public virtual DiagnosisType DiagnosisType { get; set; }
        [ForeignKey("EventId")]
        [InverseProperty("Observation")]
        public virtual Event Event { get; set; }
        [ForeignKey("LocalityTypeId")]
        [InverseProperty("Observation")]
        public virtual LocalityType LocalityType { get; set; }
        [ForeignKey("SizeGroupId")]
        [InverseProperty("Observation")]
        public virtual SizeGroup SizeGroup { get; set; }
        [InverseProperty("Observation")]
        public virtual ICollection<Image> Image { get; set; }
    }
}
