using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Public
{
    [Table("abcdmetadata")]
    public partial class Abcdmetadata
    {
        [Column("metadataid")]
        public int Metadataid { get; set; }
        [Column("datasetguid")]
        [StringLength(50)]
        public string Datasetguid { get; set; }
        [Column("technicalcontactname")]
        [StringLength(50)]
        public string Technicalcontactname { get; set; }
        [Column("technicalcontactemail")]
        [StringLength(50)]
        public string Technicalcontactemail { get; set; }
        [Column("technicalcontactphone")]
        [StringLength(50)]
        public string Technicalcontactphone { get; set; }
        [Column("technicalcontactaddress")]
        [StringLength(255)]
        public string Technicalcontactaddress { get; set; }
        [Column("contentcontactname")]
        [StringLength(50)]
        public string Contentcontactname { get; set; }
        [Column("contentcontactemail")]
        [StringLength(50)]
        public string Contentcontactemail { get; set; }
        [Column("contentcontactphone")]
        [StringLength(50)]
        public string Contentcontactphone { get; set; }
        [Column("contentcontactaddress")]
        [StringLength(255)]
        public string Contentcontactaddress { get; set; }
        [Column("otherprovideruddi")]
        [StringLength(50)]
        public string Otherprovideruddi { get; set; }
        [Column("datasettitle")]
        [StringLength(400)]
        public string Datasettitle { get; set; }
        [Column("datasetdetails")]
        [StringLength(1000)]
        public string Datasetdetails { get; set; }
        [Column("datasetcoverage")]
        [StringLength(500)]
        public string Datasetcoverage { get; set; }
        [Column("dataseturi")]
        [StringLength(50)]
        public string Dataseturi { get; set; }
        [Column("dataseticonuri")]
        [StringLength(50)]
        public string Dataseticonuri { get; set; }
        [Column("datasetversionmajor")]
        [StringLength(50)]
        public string Datasetversionmajor { get; set; }
        [Column("datasetcreators")]
        [StringLength(200)]
        public string Datasetcreators { get; set; }
        [Column("datasetcontributors")]
        [StringLength(200)]
        public string Datasetcontributors { get; set; }
        [Column("datecreated", TypeName = "date")]
        public DateTime? Datecreated { get; set; }
        [Column("datemodified", TypeName = "date")]
        public DateTime? Datemodified { get; set; }
        [Column("ownerorganizationname")]
        [StringLength(255)]
        public string Ownerorganizationname { get; set; }
        [Column("ownerorganizationabbrev")]
        [StringLength(50)]
        public string Ownerorganizationabbrev { get; set; }
        [Column("ownercontactperson")]
        [StringLength(255)]
        public string Ownercontactperson { get; set; }
        [Column("ownercontactrole")]
        [StringLength(50)]
        public string Ownercontactrole { get; set; }
        [Column("owneraddress")]
        [StringLength(255)]
        public string Owneraddress { get; set; }
        [Column("ownertelephone")]
        [StringLength(50)]
        public string Ownertelephone { get; set; }
        [Column("owneremail")]
        [StringLength(50)]
        public string Owneremail { get; set; }
        [Column("owneruri")]
        [StringLength(50)]
        public string Owneruri { get; set; }
        [Column("ownerlogouri")]
        [StringLength(255)]
        public string Ownerlogouri { get; set; }
        [Column("iprtext")]
        [StringLength(255)]
        public string Iprtext { get; set; }
        [Column("iprdetails")]
        [StringLength(1000)]
        public string Iprdetails { get; set; }
        [Column("ipruri")]
        [StringLength(255)]
        public string Ipruri { get; set; }
        [Column("copyrighttext")]
        [StringLength(400)]
        public string Copyrighttext { get; set; }
        [Column("copyrightdetails")]
        [StringLength(1000)]
        public string Copyrightdetails { get; set; }
        [Column("copyrighturi")]
        [StringLength(255)]
        public string Copyrighturi { get; set; }
        [Column("termsofusetext")]
        [StringLength(255)]
        public string Termsofusetext { get; set; }
        [Column("termsofusedetails")]
        [StringLength(1000)]
        public string Termsofusedetails { get; set; }
        [Column("termsofuseuri")]
        [StringLength(255)]
        public string Termsofuseuri { get; set; }
        [Column("disclaimerstext")]
        [StringLength(255)]
        public string Disclaimerstext { get; set; }
        [Column("disclaimersdetails")]
        [StringLength(500)]
        public string Disclaimersdetails { get; set; }
        [Column("disclaimersuri")]
        [StringLength(255)]
        public string Disclaimersuri { get; set; }
        [Column("licensetext")]
        [StringLength(255)]
        public string Licensetext { get; set; }
        [Column("licensesdetails")]
        [StringLength(500)]
        public string Licensesdetails { get; set; }
        [Column("licenseuri")]
        [StringLength(255)]
        public string Licenseuri { get; set; }
        [Column("acknowledgementstext")]
        [StringLength(255)]
        public string Acknowledgementstext { get; set; }
        [Column("acknowledgementsdetails")]
        [StringLength(500)]
        public string Acknowledgementsdetails { get; set; }
        [Column("acknowledgementsuri")]
        [StringLength(255)]
        public string Acknowledgementsuri { get; set; }
        [Column("citationstext")]
        [StringLength(255)]
        public string Citationstext { get; set; }
        [Column("citationsdetails")]
        [StringLength(1000)]
        public string Citationsdetails { get; set; }
        [Column("citationsuri")]
        [StringLength(255)]
        public string Citationsuri { get; set; }
        [Column("recordbasis")]
        [StringLength(50)]
        public string Recordbasis { get; set; }
    }
}
