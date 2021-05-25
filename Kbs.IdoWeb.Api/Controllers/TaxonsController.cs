using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxonsController : ControllerBase
    {
        private readonly DeterminationContext _detContext;
        private readonly InformationContext _infContext;
        private readonly ObservationContext _obsContext;
        private readonly MappingContext _mapContext;

        public TaxonsController(DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext)
        {
            _detContext = detContext;
            _infContext = infContext;
            _obsContext = obsContext;
            _mapContext = mapContext;
        }

        // GET: api/taxons
        [HttpGet]
        public ActionResult<IEnumerable<Taxon>> GetTaxonItems()
        {
            foreach (String key in HttpContext.Request.Query.Keys)
            {
                if (key.ToString().Contains("Class"))
                {
                    //get taxStateLevelId
                    if (Request.Query[key].Count > 0)
                    {
                        var paramKey_classFilter = Request.Query[key].ToString().Trim();
                        var classFilterId = _infContext.Taxon.Where(tx => tx.TaxonName == paramKey_classFilter).Select(tx => tx.TaxonId).FirstOrDefault();
                        var taxon_classFiltered = _infContext.Taxon
                            .Where(ts => ts.ClassId == classFilterId);

                        return Content(JsonConvert.SerializeObject(taxon_classFiltered.AsNoTracking()), "application/json");
                    }
                }
            }
            return Content(JsonConvert.SerializeObject(_infContext.Taxon.AsNoTracking()), "application/json");
        }


        // GET: api/taxons/species
        [HttpGet ("species")]
        public ActionResult<IEnumerable<Taxon>> GetTaxonItemsSpecies()
        {
            return Content(JsonConvert.SerializeObject(_infContext.Taxon.Where(t => t.TaxonomyStateId == 301).AsNoTracking()), "application/json");
        }


        // GET: api/taxons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Taxon>> GetTaxonItem(int? id)
        {
            var taxonItem = await _infContext.Taxon.FindAsync(id);

            if (taxonItem == null)
            {
                return NotFound();
            }

            return taxonItem;
        }

        // GET: api/taxons/byname/Chilopoda
        [HttpGet("byname/{taxName}")]
        public ActionResult<Taxon> GetTaxonItemByName(string taxName)
        {
            if (taxName.Length > 0)
            {
                var taxonItem = _infContext.Taxon.Where(tax => tax.TaxonName.ToLower() == taxName.ToLower()).FirstOrDefault();

                if (taxonItem == null)
                {
                    return NotFound();
                }

                return Content(JsonConvert.SerializeObject(taxonItem));
            }
            return NotFound();
        }


        [HttpGet("details/byname/{taxName}")]
        public ActionResult<Taxon> GetTaxonDetailsItemByName(string taxName)
        {
            if (taxName.Length > 0)
            {
                var taxonItem = _infContext.Taxon
                    .Where(tax => tax.TaxonName.ToLower() == taxName.ToLower())
                    .FirstOrDefault();
                if (taxonItem != null)
                {
                    var taxonDescItem = _detContext.TaxonDescription
                        .Include(tax => tax.DescriptionKey)
                            .ThenInclude(dk => dk.DescriptionKeyGroup)
                        .Where(tax => tax.TaxonId == taxonItem.TaxonId)
                        .Select(tax => new
                        {
                            descriptionKeyName = tax.DescriptionKey.KeyName,
                            descriptionKeyGroupName = tax.DescriptionKey.DescriptionKeyGroup.KeyGroupName,
                            minValue = tax.MinValue,
                            maxValue = tax.MaxValue
                        })
                        .AsNoTracking()
                        .ToList();

                    var taxonImage = _obsContext.Image
                        .Where(tax => tax.TaxonId == taxonItem.TaxonId)
                        .ToList();

                    return Content(JsonConvert.SerializeObject(new { taxon = taxonItem, taxonDescription = taxonDescItem, images = taxonImage }));

                }
            }
            return NotFound();
        }

        [HttpGet("details")]
        public ActionResult GetAllTaxonDetails()
        {
            var taxonItems = _infContext.Taxon
                .Include(tax => tax.TaxonomyState).AsNoTracking()
                .Include(tax => tax.Kingdom)
                .Include(tax => tax.Phylum)
                .Include(tax => tax.Subphylum)
                .Include(tax => tax.Class)
                .Include(tax => tax.Subclass)
                .Include(tax => tax.Order)
                .Include(tax => tax.Suborder)
                .Include(tax => tax.Family)
                .Include(tax => tax.Subfamily)
                .Include(tax => tax.Genus)
                .Include(tax => tax.RedListType)
                //.Where(tax => tax.GenusId != null && tax.SpeciesId == null)
                //for regular only species, for bodentiere all available
                .Where(tax => (tax.Group.Contains("Bodentiere")) || (tax.Group.Contains("regular") && tax.TaxonomyState.HierarchyLevel.Value >= 3000))
                .Distinct().AsNoTracking().ToList();

            if (taxonItems != null)
            {
                var taxonDescItems = _detContext.TaxonDescription
                    .Include(tax => tax.DescriptionKey)
                        .ThenInclude(dk => dk.DescriptionKeyGroup)
                    .Where(tax => taxonItems.Select(tItems => tItems.TaxonId).Contains(tax.TaxonId))
                    .Select(td => new { td.TaxonId, td.KeyValue, td.MaxValue, td.MinValue, td.DescriptionKey.KeyName, td.DescriptionKey.DescriptionKeyGroup.KeyGroupName })
                    .Distinct()
                    .AsNoTracking();

                var images = _obsContext.Image.Where(img => img.TaxonId.HasValue).OrderBy(i => i.ImagePriority).ToList();

                var taxonImages = images
                    .Where(img => taxonItems.Select(tItems => tItems.TaxonId).Contains(img.TaxonId.Value))
                    .ToList();

                /**
                var result = taxonItems
                    .Join(taxonDescItems, taxon => taxon.TaxonId, taxDesc => taxDesc.TaxonId, (taxon, taxDesc) => new
                    {
                        taxon,
                        taxonDescription = taxDesc

                    }).Join(taxonImages, taxonDescList => taxonDescList.taxon.TaxonId, taxImage => taxImage.TaxonId, (taxonDescList, taxImage) => new
                    {
                        taxonDescList.taxon,
                        taxonDescList.taxonDescription,
                        taxonImages = taxImage
                    }).GroupBy(tax => new { tax.taxon }).ToList();
                **/
                /**TODO: load taxonDescriptions via ajax on-demand **/
                var result = taxonItems
                    .GroupJoin(taxonImages, taxonItem => taxonItem.TaxonId, taxImage => taxImage.TaxonId, (taxonItem, taxImage) => new
                    {
                        taxonItem,
                        taxImage
                    })
                    .GroupJoin(taxonDescItems, taxonItem => taxonItem.taxonItem.TaxonId, td => td.TaxonId, (taxItem, taxdesc) => new
                    {
                        taxonItem = new {
                            taxItem.taxonItem.TaxonName,
                            taxItem.taxonItem.EdaphobaseId,
                            taxItem.taxonItem.TaxonId,
                            taxItem.taxonItem.TaxonDescription,
                            Taxonomy = new {
                                KingdomName = taxItem.taxonItem.KingdomId != null ? taxItem.taxonItem.Kingdom.TaxonName : null,
                                PhylumName = taxItem.taxonItem.PhylumId != null ? taxItem.taxonItem.Phylum.TaxonName : null,
                                SubPhylumName = taxItem.taxonItem.SubphylumId != null ? taxItem.taxonItem.Subphylum.TaxonName : null,
                                ClassName = taxItem.taxonItem.ClassId != null ? taxItem.taxonItem.Class.TaxonName : null,
                                SubclassName = taxItem.taxonItem.SubclassId != null ? taxItem.taxonItem.Subclass.TaxonName : null,
                                OrderName = taxItem.taxonItem.OrderId != null ? taxItem.taxonItem.Order.TaxonName : null,
                                SuborderName = taxItem.taxonItem.SuborderId != null ? taxItem.taxonItem.Suborder.TaxonName : null,
                                FamilyName = taxItem.taxonItem.FamilyId != null ? taxItem.taxonItem.Family.TaxonName : null,
                                SubfamilyName = taxItem.taxonItem.SubfamilyId != null ? taxItem.taxonItem.Subfamily.TaxonName : null,
                                GenusName = taxItem.taxonItem.GenusId != null ? taxItem.taxonItem.Genus.TaxonName : null,
                            },
                            General = new {
                                taxItem.taxonItem.DisplayLength,
                                taxItem.taxonItem.AdditionalInfo,
                                taxItem.taxonItem.RedListSource,
                                taxItem.taxonItem.RedListType?.RedListTypeName,
                                taxItem.taxonItem.LiteratureSource,
                                taxItem.taxonItem.Diagnosis,
                                taxItem.taxonItem.DistributionEurope,
                                taxItem.taxonItem.TaxonDistribution,
                                taxItem.taxonItem.TaxonBiotopeAndLifestyle,
                                taxItem.taxonItem.SliderImages
                            },
                            Synonyms = taxItem.taxonItem.Synonyms != null? JsonConvert.DeserializeObject(taxItem.taxonItem.Synonyms): null
                        },
                        taxImage = taxItem.taxImage.Where(t => t.LicenseId != null).OrderBy(i => i.ImagePriority).ToList(),
                        taxonDesc = taxdesc.GroupBy(td => td.KeyGroupName).Select(td => new { KeyGroupName = td.Key, DescriptionKeys = td.Select(tdProp => new { tdProp.KeyName, tdProp.MaxValue, tdProp.MinValue }) })
                    })
                    .GroupBy(tax => new { tax.taxonItem }).ToList().Select(x => new { x.Key.taxonItem, taxonImages = x.Select(i => i.taxImage).ToList(), taxonDescriptions = x.Select(j => j.taxonDesc) });

                return Content(JsonConvert.SerializeObject(result));
            }
            return NotFound();
        }

        // POST: api/taxons/create
        [HttpPost("create")]
        public async Task<ActionResult<Taxon>> PostTaxonItem(Taxon item)
        {
            try
            {
                _infContext.Taxon.Add(item);
                await _infContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTaxonItem), new { id = item.TaxonId }, item);
            }
            catch (System.Exception e)
            {
                var exp = e;
            }
            return null;
        }


        // PUT: api/taxons/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutTodoItem(int? id, Taxon item)
        {
            if (id != item.TaxonId)
            {
                return BadRequest();
            }

            _infContext.Entry(item).State = EntityState.Modified;
            await _infContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/taxons/destroy/5
        [HttpDelete("destroy/{id}")]
        public async Task<IActionResult> DeleteTaxonItem(int? id)
        {
            var taxonItem = await _infContext.Taxon.FindAsync(id);

            if (taxonItem == null)
            {
                return NotFound();
            }

            _infContext.Taxon.Remove(taxonItem);
            await _infContext.SaveChangesAsync();

            return NoContent();
        }
    }
}