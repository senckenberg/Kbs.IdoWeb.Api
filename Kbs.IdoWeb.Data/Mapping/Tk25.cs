using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Mapping
{
    [Table("Tk25", Schema = "Map")]
    public partial class Tk25
    {
        public int Tk25Id { get; set; }
        public int TkNr { get; set; }
        public int? TkQuadrant { get; set; }
        [StringLength(200)]
        public string Title { get; set; }
        [Column(TypeName = "numeric(18,15)")]
        public decimal? Wgs84CenterLat { get; set; }
        [Column(TypeName = "numeric(18,15)")]
        public decimal? Wgs84CenterLong { get; set; }
        [Column(TypeName = "numeric(18,15)")]
        public decimal? GkCenterLat { get; set; }
        [Column(TypeName = "numeric(18,15)")]
        public decimal? GkCenterLong { get; set; }
        public int? GkEpsg { get; set; }
        public int? Tk25IdV2 { get; set; }
    }
}
