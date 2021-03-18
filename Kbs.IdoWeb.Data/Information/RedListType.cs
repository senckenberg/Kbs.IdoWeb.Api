using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("RedListType", Schema = "Inf")]
    public partial class RedListType
    {
        public RedListType()
        {
            Taxon = new HashSet<Taxon>();
        }

        public int RedListTypeId { get; set; }
        public string RedListTypeName { get; set; }

        [InverseProperty("RedListType")]
        public virtual ICollection<Taxon> Taxon { get; set; }
    }
}
