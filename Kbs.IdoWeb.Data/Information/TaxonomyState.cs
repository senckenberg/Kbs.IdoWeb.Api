using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("TaxonomyState", Schema = "Inf")]
    public partial class TaxonomyState
    {
        public TaxonomyState()
        {
            Taxon = new HashSet<Taxon>();
        }

        public int StateId { get; set; }
        public int StateLevel { get; set; }
        [Required]
        [StringLength(50)]
        public string StateName { get; set; }
        [StringLength(100)]
        public string StateDescription { get; set; }
        [Required]
        [Column(TypeName = "bit(1)")]
        public BitArray IsTreeNode { get; set; }
        [Required]
        [Column(TypeName = "bit(1)")]
        public BitArray IsMainGroup { get; set; }
        [StringLength(100)]
        public string StateListName { get; set; }
        public int? HierarchyLevel { get; set; }

        [InverseProperty("TaxonomyState")]
        public virtual ICollection<Taxon> Taxon { get; set; }
    }
}
