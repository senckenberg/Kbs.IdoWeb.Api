using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Mapping
{
    [Table("osm_new_places", Schema = "Map")]
    public partial class OsmNewPlaces
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("osm_id")]
        public long? OsmId { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; }
        [Column("type")]
        [StringLength(255)]
        public string Type { get; set; }
        [Column("z_order")]
        public short? ZOrder { get; set; }
        [Column("population")]
        public int? Population { get; set; }
    }
}
