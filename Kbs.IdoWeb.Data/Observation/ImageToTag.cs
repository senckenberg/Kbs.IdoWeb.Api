using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Observation
{
    [Table("ImageToTag", Schema = "Obs")]
    public partial class ImageToTag
    {
        public int ImageToTagId { get; set; }
        public int ImageId { get; set; }
        public int TagId { get; set; }
    }
}
