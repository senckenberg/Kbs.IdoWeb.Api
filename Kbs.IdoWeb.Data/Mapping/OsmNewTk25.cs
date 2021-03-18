using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Mapping
{
    [Table("osm_new_tk25", Schema = "Map")]
    public partial class OsmNewTk25
    {
        [Column("gid")]
        public int Gid { get; set; }
        [Column("area")]
        public double? Area { get; set; }
        [Column("perimeter")]
        public double? Perimeter { get; set; }
        [Column("poly_")]
        public int? Poly { get; set; }
        [Column("subclass")]
        [StringLength(13)]
        public string Subclass { get; set; }
        [Column("subclass_")]
        public int? Subclass1 { get; set; }
        [Column("dtknr")]
        [StringLength(6)]
        public string Dtknr { get; set; }
        [Column("dtknummer")]
        [StringLength(13)]
        public string Dtknummer { get; set; }
        [Column("dtkname")]
        [StringLength(50)]
        public string Dtkname { get; set; }
        [Column("lva")]
        [StringLength(2)]
        public string Lva { get; set; }
        [Column("bs", TypeName = "numeric")]
        public decimal? Bs { get; set; }
        [Column("id")]
        public int? Id { get; set; }
    }
}
