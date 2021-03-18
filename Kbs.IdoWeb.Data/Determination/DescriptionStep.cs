using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Determination
{
    [Table("DescriptionStep", Schema = "Det")]
    public partial class DescriptionStep
    {
        public int DescriptionStepId { get; set; }
        public int BaseTaxonId { get; set; }
        public int DescriptionKeyId { get; set; }
        public int StepOrder { get; set; }

        [ForeignKey("DescriptionKeyId")]
        [InverseProperty("DescriptionStep")]
        public virtual DescriptionKey DescriptionKey { get; set; }
    }
}
