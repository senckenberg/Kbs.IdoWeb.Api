using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("DiagnosisType", Schema = "Obs")]
    public partial class DiagnosisType
    {
        public DiagnosisType()
        {
            Observation = new HashSet<Observation>();
        }

        public int DiagnosisTypeId { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }

        [InverseProperty("DiagnosisType")]
        public virtual ICollection<Observation> Observation { get; set; }
    }
}
