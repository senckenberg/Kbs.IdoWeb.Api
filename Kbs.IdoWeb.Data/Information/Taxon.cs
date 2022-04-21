using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kbs.IdoWeb.Data.Information
{
    [Table("Taxon", Schema = "Inf")]
    public partial class Taxon
    {
        public Taxon()
        {
            InverseClass = new HashSet<Taxon>();
            InverseFamily = new HashSet<Taxon>();
            InverseGenus = new HashSet<Taxon>();
            InverseKingdom = new HashSet<Taxon>();
            InverseOrder = new HashSet<Taxon>();
            InversePhylum = new HashSet<Taxon>();
            InverseSpecies = new HashSet<Taxon>();
            InverseSubclass = new HashSet<Taxon>();
            InverseSubfamily = new HashSet<Taxon>();
            InverseSuborder = new HashSet<Taxon>();
            InverseSubphylum = new HashSet<Taxon>();
            TaxonToRegion = new HashSet<TaxonToRegion>();
        }

        public int TaxonId { get; set; }
        public int? KingdomId { get; set; }
        public int? PhylumId { get; set; }
        public int? ClassId { get; set; }
        public int? OrderId { get; set; }
        public int? FamilyId { get; set; }
        public int? SubfamilyId { get; set; }
        public int? GenusId { get; set; }
        public int? SpeciesId { get; set; }
        [Required]
        [StringLength(100)]
        public string TaxonName { get; set; }
        public string TaxonDescription { get; set; }
        [StringLength(100)]
        public string DescriptionBy { get; set; }
        public int? DescriptionYear { get; set; }
        public int? TaxonomyStateId { get; set; }
        public string Diagnose { get; set; }
        public int? IdentificationLevelMale { get; set; }
        public int? IdentificationLevelFemale { get; set; }
        public string TaxonDistribution { get; set; }
        public string TaxonBiotopeAndLifestyle { get; set; }
        [Column(TypeName = "jsonb")]
        public string LocalisationJson { get; set; }
        public int? SubphylumId { get; set; }
        public int? SubclassId { get; set; }
        public int? SuborderId { get; set; }
        public bool? HasBracketDescription { get; set; }
        public bool? HasTaxDescChildren { get; set; }
        public string Group { get; set; }
        public int? EdaphobaseId { get; set; }
        [Column(TypeName = "jsonb")]
        public string Synonyms { get; set; }
        public int? RedListTypeId { get; set; }
        public string RedListSource { get; set; }
        public string LiteratureSource { get; set; }
        public string DistributionEurope { get; set; }
        public string Diagnosis { get; set; }
        public string AdditionalInfo { get; set; }
        public string DisplayLength { get; set; }
        [Column(TypeName = "jsonb")]
        public string SliderImages { get; set; }
        [Column(TypeName = "jsonb")]
        public string I18nNames { get; set; }

        [ForeignKey("ClassId")]
        [InverseProperty("InverseClass")]
        public virtual Taxon Class { get; set; }
        [ForeignKey("FamilyId")]
        [InverseProperty("InverseFamily")]
        public virtual Taxon Family { get; set; }
        [ForeignKey("GenusId")]
        [InverseProperty("InverseGenus")]
        public virtual Taxon Genus { get; set; }
        [ForeignKey("KingdomId")]
        [InverseProperty("InverseKingdom")]
        public virtual Taxon Kingdom { get; set; }
        [ForeignKey("OrderId")]
        [InverseProperty("InverseOrder")]
        public virtual Taxon Order { get; set; }
        [ForeignKey("PhylumId")]
        [InverseProperty("InversePhylum")]
        public virtual Taxon Phylum { get; set; }
        [ForeignKey("RedListTypeId")]
        [InverseProperty("Taxon")]
        public virtual RedListType RedListType { get; set; }
        [ForeignKey("SpeciesId")]
        [InverseProperty("InverseSpecies")]
        public virtual Taxon Species { get; set; }
        [ForeignKey("SubclassId")]
        [InverseProperty("InverseSubclass")]
        public virtual Taxon Subclass { get; set; }
        [ForeignKey("SubfamilyId")]
        [InverseProperty("InverseSubfamily")]
        public virtual Taxon Subfamily { get; set; }
        [ForeignKey("SuborderId")]
        [InverseProperty("InverseSuborder")]
        public virtual Taxon Suborder { get; set; }
        [ForeignKey("SubphylumId")]
        [InverseProperty("InverseSubphylum")]
        public virtual Taxon Subphylum { get; set; }
        [ForeignKey("TaxonomyStateId")]
        [InverseProperty("Taxon")]
        public virtual TaxonomyState TaxonomyState { get; set; }
        [InverseProperty("Class")]
        public virtual ICollection<Taxon> InverseClass { get; set; }
        [InverseProperty("Family")]
        public virtual ICollection<Taxon> InverseFamily { get; set; }
        [InverseProperty("Genus")]
        public virtual ICollection<Taxon> InverseGenus { get; set; }
        [InverseProperty("Kingdom")]
        public virtual ICollection<Taxon> InverseKingdom { get; set; }
        [InverseProperty("Order")]
        public virtual ICollection<Taxon> InverseOrder { get; set; }
        [InverseProperty("Phylum")]
        public virtual ICollection<Taxon> InversePhylum { get; set; }
        [InverseProperty("Species")]
        public virtual ICollection<Taxon> InverseSpecies { get; set; }
        [InverseProperty("Subclass")]
        public virtual ICollection<Taxon> InverseSubclass { get; set; }
        [InverseProperty("Subfamily")]
        public virtual ICollection<Taxon> InverseSubfamily { get; set; }
        [InverseProperty("Suborder")]
        public virtual ICollection<Taxon> InverseSuborder { get; set; }
        [InverseProperty("Subphylum")]
        public virtual ICollection<Taxon> InverseSubphylum { get; set; }
        [InverseProperty("Taxon")]
        public virtual ICollection<TaxonToRegion> TaxonToRegion { get; set; }
    }
}
