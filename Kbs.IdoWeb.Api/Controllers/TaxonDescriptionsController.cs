using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxonDescriptionsController : ControllerBase
    {
        public static DeterminationContext _detContext;
        public static InformationContext _infContext;
        private HashSet<int> _uniqueTaxonIdList = new HashSet<int>();
        /**
        private List<int> _taxIds_topLevelFiltered;
        private int? _topLevelFilterId = null;
        //taxLevelFilterId: e.g. get all taxa on order-level, species-level, etc
        private int? _taxLevelFilterId = null;
        private IQueryable<int> _taxIds_taxLevelFiltered;
        private int? _genderFilterId = null;
        private IQueryable<int> _taxIds_genderFiltered;
        private string _importGroupFilter = "regular";
        private List<int> _taxIds_dropDFiltered = null;
        private int? _dropdownGroupFilter = null;
        private List<int> _positionFilterDks = null;
        private List<int> _taxIds_positionFiltered;
        private List<int> _taxIds_lowLevelFiltered;
        private List<int> _taxIds_importGroupFiltered;
        private List<int> _taxIds_dkFiltered;
        **/
        Dictionary<int, List<int>> _selected_dkg_dk_dict = new Dictionary<int, List<int>>();
        Dictionary<int, decimal?> _selected_dkg_dk_val_dict = new Dictionary<int, decimal?>();

        public TaxonDescriptionsController(DeterminationContext detContext, InformationContext infContext)
        {
            _detContext = detContext;
            _infContext = infContext;
        }

        [HttpGet("gender")]
        public ActionResult<TaxonDescription> GetTaxDescGender(string taxonName)
        {
            var genderList = _detContext.TaxonDescription.Select(td => td.DescriptionKeyTypeId).Distinct().ToList();
            var map = _detContext.DescriptionKeyType.Select(dkt => new { dkt.DescriptionKeyTypeId, dkt.DescriptionKeyTypeName }).Where(dkt => genderList.Contains(dkt.DescriptionKeyTypeId)).ToList();
            map.Select(m => m.DescriptionKeyTypeName.Replace(";", "&"));
            return Content(JsonConvert.SerializeObject(map));
        }

        // GET: api/levels/
        [HttpGet("levels")]
        public ActionResult<TaxonLevel> GetTaxDescStateLevels(string toplevel, string lowLevel, string groupFilter)
        {
            try
            {
                List<int> taxonDescTaxIds = _infContext.Taxon.Where(td => td.HasTaxDescChildren.Value).Select(tax => tax.TaxonId).Distinct().ToList();
                HashSet<int> _uniqueTaxonIdList = _detContext.TaxonDescription.Select(tax => tax.TaxonId).Distinct().ToHashSet();

                foreach (String key in HttpContext.Request.Query.Keys)
                {
                    if (key.ToString().Contains("toplevel"))
                    {
                        //get all taxondescriptions available in table
                        if (toplevel != null)
                        {
                            var topLevelTaxonId = _infContext.Taxon.Where(tax => tax.TaxonName == toplevel).Select(tax => tax.TaxonId).FirstOrDefault();

                            if (topLevelTaxonId != 0)
                            {
                                var taxonIds = _infContext.Taxon
                                    .Where(tax => tax.HasTaxDescChildren == true && (tax.KingdomId == topLevelTaxonId || tax.PhylumId == topLevelTaxonId || tax.ClassId == topLevelTaxonId || tax.OrderId == topLevelTaxonId || tax.FamilyId == topLevelTaxonId))
                                    .Select(tax => tax.TaxonId).ToList();
                                taxonIds.Add(topLevelTaxonId);
                                //List<int> taxonTopLevelFiltered = _infContext.Taxon.Where(tax => taxonIds.Contains(tax.TaxonId)).Select(tax => tax.TaxonId).Distinct().ToList();
                                taxonDescTaxIds = taxonDescTaxIds.Intersect(taxonIds).ToList();
                            }
                        }
                    }
                    if (key.ToString().Contains("lowLevel"))
                    {
                        if (lowLevel != null)
                        {
                            var hierarchyLevel = _infContext.TaxonomyState.Where(ts => lowLevel.Trim().ToLower() == ts.StateDescription).Select(ts => ts.HierarchyLevel).FirstOrDefault();
                            var higherHierarchyLevels = _infContext.TaxonomyState.Where(ts => hierarchyLevel > ts.HierarchyLevel).OrderByDescending(ts => ts.HierarchyLevel).Select(ts => ts.StateId).ToList();
                            if (higherHierarchyLevels.Count > 1)
                            {
                                higherHierarchyLevels.RemoveAt(0);
                            }

                            var taxon_lowLevelFiltered = _infContext.Taxon
                                .Where(tax => higherHierarchyLevels.Contains(tax.TaxonomyStateId.Value))
                                .Select(tx => tx.TaxonId).ToList();
                            taxonDescTaxIds = taxonDescTaxIds.Intersect(taxon_lowLevelFiltered).ToList();
                        }
                    }
                    if (key.ToString().Contains("groupFilter"))
                    {
                        if (groupFilter != null)
                        {
                            var taxon_dropDFiltered = _infContext.Taxon
                                .Where(tax => tax.Group.Contains(groupFilter.Trim()))
                                .Select(tx => tx.TaxonId).ToList();
                            taxonDescTaxIds = taxonDescTaxIds.Intersect(taxon_dropDFiltered).ToList();
                        }
                        else
                        {
                            var taxon_dropDFiltered = _infContext.Taxon
                                .Where(tax => tax.Group.Contains("regular"))
                                .Select(tx => tx.TaxonId).ToList();
                            taxonDescTaxIds = taxonDescTaxIds.Intersect(taxon_dropDFiltered).ToList();
                        }
                    }
                }



                List<Taxon> result = _infContext.Taxon.Where(tax => taxonDescTaxIds.Contains(tax.TaxonId)).Select(tax => new Taxon { TaxonId = tax.TaxonId, TaxonName = tax.TaxonName, TaxonomyStateId = tax.TaxonomyStateId, OrderId = tax.OrderId, ClassId = tax.ClassId }).Distinct().ToList();
                List<TaxonLevel> taxonLevels = new List<TaxonLevel>();
                //Isopoda only has children with TaxonomyStateId == 100
                bool startAt100 = false;
                if (!result.Select(t => t.TaxonomyStateId).ToList().Contains(119))
                {
                    startAt100 = true;
                }
                foreach (var taxonItem in result.OrderByDescending(t => t.TaxonomyStateId).ThenBy(t => t.TaxonName))
                {
                    if (!startAt100)
                    {
                        if (taxonItem.TaxonomyStateId == 119)
                        {
                            taxonLevels.Add(new TaxonLevel(taxonItem.TaxonId, taxonItem.TaxonName, taxonItem.TaxonomyStateId));
                        }
                        if (taxonItem.TaxonomyStateId == 117)
                        {
                            TaxonLevel parentTax = taxonLevels.FirstOrDefault(t => t.TaxonId == taxonItem.ClassId);
                            if (parentTax != null)
                            {
                                parentTax.Children.Add(new TaxonLevel(taxonItem.TaxonId, taxonItem.TaxonName, taxonItem.TaxonomyStateId));
                                if (parentTax.hasChildren == false)
                                {
                                    parentTax.hasChildren = true;
                                }
                            }
                        }
                        if (taxonItem.TaxonomyStateId == 100)
                        {
                            TaxonLevel gParentTax = taxonLevels.FirstOrDefault(t => t.TaxonId == taxonItem.ClassId);
                            if (gParentTax != null)
                            {
                                foreach (TaxonLevel parentTax in gParentTax.Children)
                                {
                                    if (parentTax.TaxonId == taxonItem.OrderId)
                                    {
                                        parentTax.Children.Add(new TaxonLevel(taxonItem.TaxonId, taxonItem.TaxonName, taxonItem.TaxonomyStateId));
                                        if (parentTax.hasChildren == false)
                                        {
                                            parentTax.hasChildren = true;
                                        }
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (taxonItem.TaxonomyStateId == 117)
                        {
                            taxonLevels.Add(new TaxonLevel(taxonItem.TaxonId, taxonItem.TaxonName, taxonItem.TaxonomyStateId));


                        }
                        if (taxonItem.TaxonomyStateId == 100)
                        {
                            TaxonLevel parentTax = taxonLevels.FirstOrDefault(t => t.TaxonId == taxonItem.OrderId);
                            if (parentTax != null)
                            {
                                parentTax.Children.Add(new TaxonLevel(taxonItem.TaxonId, taxonItem.TaxonName, taxonItem.TaxonomyStateId));
                                if (parentTax.hasChildren == false)
                                {
                                    parentTax.hasChildren = true;
                                }
                            }
                        }
                    }
                }

                List<TaxonLevel> res = taxonLevels
                    .OrderBy(gParent => gParent.TaxonName, StringComparer.InvariantCultureIgnoreCase)
                    .ThenBy(gParent => gParent.Children.ToList().Select(parent => parent.TaxonName))
                    .ThenBy(gParent => gParent.Children.ToList().Select(parent => parent.Children.ToList().Select(children => children.TaxonName))).ToList();
                return Content(JsonConvert.SerializeObject(res));

            }
            catch (Exception e)
            {
                var exp = e;
                throw (e);
            }
        }

        public class TaxonLevel
        {
            public TaxonLevel(int taxonId, string taxonName, int? taxonomyStateId)
            {
                TaxonId = taxonId;
                TaxonName = taxonName;
                TaxonomyStateId = taxonomyStateId;
                Children = new List<TaxonLevel>();
            }

            public int TaxonId { get; set; }
            public string TaxonName { get; set; }
            public int? TaxonomyStateId { get; set; }
            public bool hasChildren { get; set; }
            public List<TaxonLevel> Children { get; set; }
        }

        // GET: api/taxonDescriptions/
        [HttpGet]
        public ActionResult<TaxonDescription> GetTaxonDescriptionItemByDKId()
        {
            try
            {
                //GET ALL TAXDESCS
                var q = HttpContext.Request.Query;
                TaxDescFilterObject tdFilterObj = new TaxDescFilterObject();
                //tdFilterObj._uniqueTaxonIdList = _detContext.TaxonDescription.Select(td => td.TaxonId).Distinct().ToHashSet();
                var result = tdFilterObj.ParseQuery(q);
                if (result != null)
                {
                    return Content(JsonConvert.SerializeObject(result));
                }
                //_uniqueTaxonIdList = _detContext.TaxonDescription.Select(td => td.TaxonId).Distinct().ToHashSet();
                //topLevelFilterId needs to be passed thru till prediction values are calculated
                //set topmost taxon and get all taxa underneath
                //groupingFilter: get all taxa under a certain taxon, e.g. all species belonging to order lithobiomorpha
                //int? groupingFilterId = null;
                //Iterate Query params create TaxDesc Instances and query


                //use uniqueTaxonList to calc Rip-Order for DescKeyGroups
                //Should be an ordered list containing DKGIds in ASCending order
                //TODO: replace >= thru >
                /*
                if (_uniqueTaxonIdList.Count >= 1)
                {
                    (Dictionary<int, int> dkList, Dictionary<int, decimal> ripOrder)? ripValueOrderAndDkList = GetPayloadData();
                    return Content(JsonConvert.SerializeObject(new
                    {
                        taxonIdList = _uniqueTaxonIdList,
                        extraPayload = ripValueOrderAndDkList
                    }));
                }
                */
                return Content(JsonConvert.SerializeObject(new
                {
                    taxonIdList = _uniqueTaxonIdList,
                }));
            }
            catch (Exception e)
            {
                return Content(JsonConvert.SerializeObject(e));
            }
        }

        // GET: api/taxonDescriptions/count?descriptionKeyId=5
        [HttpGet("count")]
        public ActionResult<TaxonDescription> GetTaxonDescriptionCount(int? descriptionKeyId)
        {
            if (descriptionKeyId != null)
            {
                List<TaxonDescription> taxDescs = _detContext.TaxonDescription.Where(dk => dk.DescriptionKeyId == descriptionKeyId).AsNoTracking().ToList();
                return Content(JsonConvert.SerializeObject(taxDescs.Count), "application/json");
            }
            return Content(JsonConvert.SerializeObject(_detContext.TaxonDescription.AsNoTracking().ToList().Count), "application/json");
        }

        // GET: api/taxonDescriptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxonDescription>> GetTaxonDescriptionItem(int? id)
        {
            var todoItem = await _detContext.TaxonDescription.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/taxonDescriptions/create
        [HttpPost("create")]
        public async Task<ActionResult<TaxonDescription>> PostTaxonDescriptionItem(TaxonDescription item)
        {
            try
            {
                _detContext.TaxonDescription.Add(item);
                await _detContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetTaxonDescriptionItem), new { id = item.TaxonDescriptionId }, item);
            }
            catch (System.Exception e)
            {
                var exp = e;
            }
            return null;
        }


        // PUT: api/taxonDescriptions/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutTodoItem(int? id, TaxonDescription item)
        {
            if (id != item.TaxonDescriptionId)
            {
                return BadRequest();
            }

            _detContext.Entry(item).State = EntityState.Modified;
            await _detContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/taxonDescriptions/destroy/5
        [HttpDelete("destroy/{id}")]
        public async Task<IActionResult> DeleteTaxonDescriptionItem(int? id)
        {
            var taxonDescriptionItem = await _detContext.TaxonDescription.FindAsync(id);

            if (taxonDescriptionItem == null)
            {
                return NotFound();
            }

            _detContext.TaxonDescription.Remove(taxonDescriptionItem);
            await _detContext.SaveChangesAsync();

            return NoContent();
        }




        // GET: api/taxonDescriptions/sliderVals
        [HttpGet("sliderVals")]
        public ActionResult<Dictionary<int, List<decimal?>>> GetSliderMinMaxValues()
        {
            try
            {
                var dk_values = _detContext.TaxonDescription
                    .AsNoTracking()
                    .Where(td => td.MinValue != null && td.MaxValue != null)
                    .Distinct().ToList();

                var minVals = dk_values.GroupBy(key => key.DescriptionKeyId, grp => grp.MinValue, (k, g) => new
                {
                    DescriptionKeyId = k,
                    MinVal = g.Min()
                }).ToDictionary(k => k.DescriptionKeyId, v => v.MinVal);

                var maxVals = dk_values.GroupBy(key => key.DescriptionKeyId, grp => grp.MaxValue, (k, g) => new
                {
                    DescriptionKeyId = k,
                    MaxValue = g.Max()
                }).ToDictionary(k => k.DescriptionKeyId, v => v.MaxValue);

                var result = minVals.Union(maxVals)
                    .GroupBy(kv => kv.Key)
                    .ToDictionary(group => group.Key, group => group.Select(g => g.Value).ToList());

                return Content(JsonConvert.SerializeObject(result));

            }
            catch (Exception e)
            {
                return Content(JsonConvert.SerializeObject(e));
            }

        }

        private static string _getTaxonHierarchyLevel(Taxon tax)
        {
            //TODO: simplify with linq?
            if (tax.KingdomId == null)
            {
                return "KingdomId";
            }
            else if (tax.PhylumId == null)
            {
                return "PhylumId";
            }
            else if (tax.ClassId == null)
            {
                return "ClassId";
            }
            else if (tax.OrderId == null)
            {
                return "OrderId";
            }
            else if (tax.FamilyId == null)
            {
                return "FamilyId";
            }
            /** enable when subfamily in excel import
            else if (taxonGroupStateId.SubfamilyId == null)
            {
                return "SubfamilyId";
            }
            **/
            else if (tax.GenusId == null)
            {
                return "GenusId";
            }
            return null;
        }

        public class TaxDescFilterObject
        {
            //set through dropdown
            public int _dropDFilter { get; private set; }
            //set through template (top-level)
            public int _tlFilter { get; private set; }
            //set through template (low-level)
            public string _llFilter { get; private set; }
            //set by page (Bodentier || regular)
            public string _ggFilter { get; private set; }

            public int _taxLevelFilter { get; private set; }
            //KeyType = Data
            public Dictionary<int, decimal> _dataValues { get; private set; }
            //KeyType = Value
            public Dictionary<int, List<int>> _dkg_dk_dict { get; private set; }
            public static HashSet<int> _uniqueTaxonIdList { get; set; }
            //public static List<ValueTuple<int, int, decimal?, decimal?>> _taxD_DK_DataValues {get;set;}
            public static List<TaxD_DK_DataValues> _taxD_DK_DataValues;

            public TaxDescFilterObject()
            {
                _uniqueTaxonIdList = _detContext.TaxonDescription.AsNoTracking().Select(td => td.TaxonId).Distinct().ToHashSet();
                //_taxD_DK_DataValues = 
                _dataValues = new Dictionary<int, decimal>();
                _taxD_DK_DataValues = _detContext.TaxonDescription
                     .Select(td => new TaxD_DK_DataValues { TaxonId = td.TaxonId, DescriptionKeyId = td.DescriptionKeyId, MinValue = td.MinValue, MaxValue = td.MaxValue }).AsNoTracking().ToList();
            }

            public object ParseQuery(IQueryCollection q)
            {
                try
                {
                    if (int.TryParse(q["Group"], out int g)) { _dropDFilter = g; }
                    if (int.TryParse(q["TLF"], out int tl)) { _tlFilter = tl; }
                    _llFilter = q["LLF"].ToString();
                    if (int.TryParse(q["TaxLevel"], out int tax)) { _taxLevelFilter = tax; }
                    //"regular" || "Bodentiere"
                    _ggFilter = q["GGF"].ToString() == "Bodentiere" ? "Bodentiere" : "regular";
                    ParseDataAndDkValues(q.Where(qF => qF.Key.Contains("data_value") || Int32.TryParse(qF.Key, out int t)).ToDictionary(qF => qF.Key, qF => qF.Value.ToString()));
                    //true == Bodentiere --> ignore TLF,LLF,Group Filters
                    if (!ApplyGGFilterToTaxa())
                    {
                        ApplyTLFilterToTaxa();
                        ApplyLLFilterToTaxa();
                        ApplyGroupFilterToTaxa();
                    }

                    if (_uniqueTaxonIdList.Count >= 1)
                    {
                        (Dictionary<int, int> dkList, Dictionary<int, decimal> ripOrder)? ripValueOrderAndDkList = GetPayloadData();
                        return (new
                        {
                            taxonIdList = _uniqueTaxonIdList,
                            extraPayload = ripValueOrderAndDkList
                        });
                    }


                    /**
                    //If GGF-Filter is set to bodentiere, ignore TLF, LLF, Group
                    foreach (String key in HttpContext.Request.Query.Keys)
                    {
                        if (key.ToString().Contains("Group"))
                        {
                            if (Int32.TryParse(Request.Query[key], out int paramKey_groupingTaxonId))
                            {
                                var taxonGroupInst = _infContext.Taxon
                                    .Where(tax => tax.TaxonId == paramKey_groupingTaxonId)
                                    .FirstOrDefault();

                                //var hierLevel = _getTaxonHierarchyLevel(taxonGroupInst);
                                var taxon_dropDFiltered = _getTaxonIdsByHierarchyLevel(taxonGroupInst, _getTaxonHierarchyLevel(taxonGroupInst));
                                _taxIds_dropDFiltered = taxon_dropDFiltered;
                                _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                                _dropdownGroupFilter = paramKey_groupingTaxonId;
                            }
                        }
                        else if (key.ToString().Contains("TLF"))
                        {
                            //get taxStateLevelId
                            if (Int32.TryParse(Request.Query[key], out int paramKey_tlf))
                            {
                                //var paramKey_tlf = Request.Query[key].ToString().Trim();
                                var taxon_topLevelFiltered = _infContext.Taxon
                                    .Where(tax => tax.KingdomId == paramKey_tlf || tax.PhylumId == paramKey_tlf || tax.SubphylumId == paramKey_tlf || tax.ClassId == paramKey_tlf || tax.SubclassId == paramKey_tlf || tax.OrderId == paramKey_tlf || tax.SuborderId == paramKey_tlf || tax.FamilyId == paramKey_tlf || tax.SubfamilyId == paramKey_tlf || tax.GenusId == paramKey_tlf)
                                    .Select(tx => tx.TaxonId).ToList();
                                _uniqueTaxonIdList.IntersectWith(taxon_topLevelFiltered);
                                _taxIds_topLevelFiltered = taxon_topLevelFiltered;
                                _topLevelFilterId = paramKey_tlf;
                            }
                        }
                        else if (key.ToString().Contains("LLF"))
                        {
                            //comes in as 'family', 'subfamily', 'order', etc.
                            var paramKey_llf = Request.Query[key].ToString().Trim();
                            var hierarchyLevel = _infContext.TaxonomyState.Where(ts => paramKey_llf == ts.StateDescription).Select(ts => ts.HierarchyLevel).FirstOrDefault();
                            var higherHierarchyLevels = _infContext.TaxonomyState.Where(ts => hierarchyLevel > ts.HierarchyLevel).Select(ts => ts.StateId).ToList();

                            var taxon_lowLevelFiltered = _infContext.Taxon
                                .Where(tax => higherHierarchyLevels.Contains(tax.TaxonomyStateId.Value))
                                .Select(tx => tx.TaxonId).ToList();
                            _uniqueTaxonIdList.IntersectWith(taxon_lowLevelFiltered);
                            _taxIds_lowLevelFiltered = taxon_lowLevelFiltered;
                            //topLevelFilterId = paramKey_tlf;
                        }
                        else if (key.ToString().Contains("GGF"))
                        {
                            //"regular" or "Bodentiere"
                            var paramKey_ggf = Request.Query[key].ToString().Trim();
                            List<int> taxon_dropDFiltered = null;
                            if (paramKey_ggf != "")
                            {
                                taxon_dropDFiltered = _infContext.Taxon
                                    .Where(tax => tax.Group.Contains(paramKey_ggf.Trim()))
                                    .Select(tx => tx.TaxonId).ToList();
                                _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                                _importGroupFilter = paramKey_ggf;
                            }
                            else
                            {
                                taxon_dropDFiltered = _infContext.Taxon
                                    .Where(tax => tax.Group.Contains("regular"))
                                    .Select(tx => tx.TaxonId).ToList();
                                _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                            }
                            _taxIds_importGroupFiltered = taxon_dropDFiltered;
                        }
                        else if (key.ToString().Contains("TaxLevel"))
                        {
                            //get taxStateLevelId
                            if (Int32.TryParse(Request.Query[key], out int paramKey_taxStateId))
                            {
                                var taxon_stateFiltered = _infContext.Taxon
                                    .Where(ts => ts.TaxonomyStateId == paramKey_taxStateId)
                                    .Select(tx => tx.TaxonId);
                                _uniqueTaxonIdList.IntersectWith(taxon_stateFiltered);
                                _taxLevelFilterId = paramKey_taxStateId;
                                _taxIds_taxLevelFiltered = taxon_stateFiltered;
                            }
                        }
                        //get id from query param
                        else if (key.ToString().Contains("data_value_"))
                        {
                            string dkId = key.ToString().Replace("data_value_", "");
                            var style = NumberStyles.AllowDecimalPoint;
                            var queryString = Regex.Replace(Request.Query[key].ToString(), "[^0-9.,]", "");

                            if (Int32.TryParse(dkId, out int paramKey_dk) && Decimal.TryParse(queryString, style, CultureInfo.InvariantCulture, out decimal paramVal_dk_1))
                            {
                                List<int> taxDescInst = new List<int>();
                                taxDescInst = _detContext.TaxonDescription.Where(td => td.DescriptionKeyId == paramKey_dk && td.MinValue <= paramVal_dk_1 && td.MaxValue >= paramVal_dk_1).Select(td => td.TaxonId).Distinct().ToList();
                                _uniqueTaxonIdList.IntersectWith(taxDescInst);
                                var dkg_id = _detContext.DescriptionKey.Where(dk => dk.DescriptionKeyId == paramKey_dk).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                                _selected_dkg_dk_val_dict.Add(paramKey_dk, paramVal_dk_1);
                            }
                        }

                        //TODO SANITIZE INPUT?
                        else if (Int32.TryParse(key, out int paramKey_dkg))
                        {
                            List<int> taxDesc_OR = null;

                            //Single param in one dkg
                            if (Int32.TryParse(Request.Query[key], out int paramVal_dk_2))
                            {
                                taxDesc_OR = _detContext.TaxonDescription.Where(td => td.DescriptionKeyId == paramVal_dk_2).Select(td => td.TaxonId).Distinct().ToList();
                                var dkg_id = _detContext.DescriptionKey.Where(dk => dk.DescriptionKeyId == paramVal_dk_2).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                                _selected_dkg_dk_dict.Add(dkg_id, new List<int>() { paramVal_dk_2 });
                            }
                            else if (Request.Query[key].ToString().Contains(","))
                            {
                                var dKList = Request.Query[key].ToString().Split(",").Select(Int32.Parse).ToList();
                                taxDesc_OR = _detContext.TaxonDescription.Where(td => dKList.Contains(td.DescriptionKeyId)).Select(td => td.TaxonId).Distinct().ToList();
                                var dkg_id = _detContext.DescriptionKey.Where(dk => dKList.Contains(dk.DescriptionKeyId)).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                                _selected_dkg_dk_dict.Add(dkg_id, dKList);
                            }
                            _uniqueTaxonIdList.IntersectWith(taxDesc_OR);
                            _taxIds_dkFiltered = taxDesc_OR;
                        }
                    }
                    **/
                }
                catch (Exception e)
                {
                    throw e;
                }
                return null;
            }

            private void ApplyGroupFilterToTaxa()
            {
                var taxonGroupInst = _infContext.Taxon
                    .AsNoTracking()
                    .Where(tax => tax.TaxonId == _dropDFilter)
                    .FirstOrDefault();

                //var hierLevel = _getTaxonHierarchyLevel(taxonGroupInst);
                var taxon_dropDFiltered = _getTaxonIdsByHierarchyLevel(taxonGroupInst, _getTaxonHierarchyLevel(taxonGroupInst));
                //_taxIds_dropDFiltered = taxon_dropDFiltered;
                _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                //_dropdownGroupFilter = paramKey_groupingTaxonId;
            }

            public (Dictionary<int, int> dkList, Dictionary<int, decimal> ripOrder)? GetPayloadData()
            {
                //TODO: refactor!!
                try
                {
                    //get all resuming taxdesc + DescriptionKeys
                    List<TaxonDescription> taxDescList = _detContext.TaxonDescription
                        .Where(taxDesc => _uniqueTaxonIdList.Contains(taxDesc.TaxonId))
                        .Include(taxDesc => taxDesc.DescriptionKey)
                            .ThenInclude(dk => dk.DescriptionKeyGroup)
                        .ToList();

                    //Amount 
                    int taxon_Count = _uniqueTaxonIdList.Count();

                    //Iterate DKs and calc RipValues (per DKG)
                    List<DescriptionKey> dkList = null;
                    dkList = taxDescList.Select(taxDesc => taxDesc.DescriptionKey).Distinct().ToList();
                    /**
                    if (_importGroupFilter != null)
                    {
                        var taxIds = _infContext.Taxon.Where(tax => EF.Functions.Like(tax.Group, _importGroupFilter)).Select(tax => tax.TaxonId).Distinct().ToList();
                        var dkList_group = taxDescList.Where(taxDesc => taxIds.Contains(taxDesc.TaxonId)).Select(taxDesc => taxDesc.DescriptionKey).Distinct().ToList();
                        dkList = dkList.Intersect(dkList_group).ToList();
                    }

                    if (_positionFilterDks != null)
                    {
                        dkList.RemoveAll(dkItem => !_positionFilterDks.Contains(dkItem.DescriptionKeyId));
                    }
                    **/

                    //TODO: REWRITE BELOW!!!
                    //Get all available rest DKs from TaxDesc
                    var dkIdList = dkList.Select(dk => dk.DescriptionKeyId).ToList();
                    taxDescList.RemoveAll(td => !dkIdList.Contains(td.DescriptionKeyId));

                    //get DKGs available
                    List<int> uniqueDKG_Hash = taxDescList
                        .Where(taxDesc => dkIdList.Contains(taxDesc.DescriptionKeyId))
                        .Select(taxDesc => taxDesc.DescriptionKey.DescriptionKeyGroupId)
                        .Distinct().ToList();

                    Dictionary<int, decimal> orderedDKGList = uniqueDKG_Hash.ToDictionary(x => x, x => (decimal)0.0);
                    Dictionary<int, int> DKSplitValList = new Dictionary<int, int>();
                    DKSplitValList = dkList.Select(dk => dk.DescriptionKeyId).ToDictionary(x => x, x => 0);

                    //iterate TaxDesc
                    foreach (var item in taxDescList)
                    {
                        DescriptionKey descKey = item.DescriptionKey;
                        //Include relative value (erwartungswert, e.g. 1/5 and calc abs)
                        int dkgId = descKey.DescriptionKeyGroupId;
                        int dkId = descKey.DescriptionKeyId;
                        //
                        int dk_expectedVal = _detContext.DescriptionKey.Where(dk => dk.DescriptionKeyGroupId == dkgId).Count();

                        //count taxon hits
                        if (taxDescList.Select(taxdesc => taxdesc.DescriptionKeyId).Contains(dkId))
                        {
                            DKSplitValList[dkId] += 1;
                        }
                    }

                    //calc % how much of the resuming taxons are targeted by dk
                    foreach (var item in DKSplitValList)
                    {
                        decimal dk_relativeVal = decimal.Round((decimal)item.Value / taxon_Count, 2, MidpointRounding.AwayFromZero);
                        int dkId = item.Key;
                        int dkgId = _detContext.DescriptionKey.Where(dk => dk.DescriptionKeyId == dkId).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                        int dkg_nrOfDKs = taxDescList.Select(td => td.DescriptionKey)
                            .Where(dk => dk.DescriptionKeyGroupId == dkgId)
                            .Distinct()
                            .Count();

                        decimal dk_ideal_expected = (decimal)taxon_Count / dkg_nrOfDKs;

                        // > 1 --> first value is greater
                        decimal dk_difference = Math.Round(Decimal.Compare(dk_relativeVal, dk_ideal_expected) > 1 ? Math.Abs(dk_ideal_expected - dk_relativeVal) : Math.Abs(dk_relativeVal - dk_ideal_expected), 2);
                        if (dk_difference > orderedDKGList[dkgId])
                        {
                            orderedDKGList[dkgId] = dk_difference;
                        }
                    }

                    var result = (
                        dkList: GetPredictionVals(),
                        ripOrder: orderedDKGList
                    );
                    return result;
                }
                catch (System.Exception e)
                {
                    var exp = e;
                    return null;
                }
            }

            private Dictionary<int, int> GetPredictionVals()
            {
                //TODO: REWRITE!!!!
                //get all unused dks
                //combine with applied dks (in same dkg?)
                //get result
                var dkList = _detContext.DescriptionKey.AsNoTracking().Select(dk => dk.DescriptionKeyId).ToList();
                //List<int> dkList = null;
                List<int> taxonIdList = _taxD_DK_DataValues.Select(td => td.TaxonId).Distinct().ToList();

                //filter TaxIds by filters set on client
                //dropdown
                if (_dropDFilter != 0)
                {
                    var taxonGroupInst = _infContext.Taxon
                        .AsNoTracking()
                        .Where(tax => tax.TaxonId == _dropDFilter)
                        .FirstOrDefault();

                    var taxon_dropDFiltered = _getTaxonIdsByHierarchyLevel(taxonGroupInst, _getTaxonHierarchyLevel(taxonGroupInst));
                    //_taxIds_dropDFiltered = taxon_dropDFiltered;
                    //var taxonIdList_groupDDL = _infContext.Taxon.Where(tax => tax.Contains(_dropdownGroupFilter.Trim())).Select(tax => tax.TaxonId).Distinct().ToList();
                    taxonIdList = taxonIdList.Intersect(taxon_dropDFiltered).ToList();
                    var dkList_dropGroupL = _taxD_DK_DataValues
                        .Where(td => taxon_dropDFiltered.Contains(td.TaxonId))
                        .Select(td => td.DescriptionKeyId).Distinct().ToList();
                    dkList = dkList.Intersect(dkList_dropGroupL).ToList();
                }

                if (_ggFilter != "regular")
                {
                    var taxonIdList_groupL = _infContext.Taxon.AsNoTracking().Where(tax => tax.Group.Contains(_ggFilter.Trim())).Select(tax => tax.TaxonId).Distinct().ToList();
                    taxonIdList = taxonIdList.Intersect(taxonIdList_groupL).ToList();
                    var dkList_importGroupL = _taxD_DK_DataValues
                        .Where(td => taxonIdList_groupL.Contains(td.TaxonId))
                        .Select(td => td.DescriptionKeyId).Distinct().ToList();
                    dkList = dkList.Intersect(dkList_importGroupL).ToList();
                }

                if (_taxLevelFilter != 0)
                {
                    var taxonIdList_taxL = _infContext.Taxon.AsNoTracking().Where(td => td.TaxonomyStateId == _taxLevelFilter).Select(tx => tx.TaxonId).Distinct().ToList();
                    var dkList_taxL = _taxD_DK_DataValues
                        .Where(td => taxonIdList_taxL.Contains(td.TaxonId))
                        .Select(td => td.DescriptionKeyId).Distinct().ToList();

                    taxonIdList = taxonIdList.Intersect(taxonIdList_taxL).ToList();
                    dkList = dkList.Intersect(dkList_taxL).ToList();
                }

                if (_tlFilter != 0)
                {
                    var taxonIdList_topL = _infContext.Taxon
                        .AsNoTracking()
                        .Where(tax => tax.KingdomId == _tlFilter || tax.PhylumId == _tlFilter || tax.SubphylumId == _tlFilter || tax.ClassId == _tlFilter || tax.SubclassId == _tlFilter || tax.OrderId == _tlFilter || tax.SuborderId == _tlFilter || tax.FamilyId == _tlFilter || tax.SubfamilyId == _tlFilter || tax.GenusId == _tlFilter)
                        .Select(tx => tx.TaxonId).Distinct().ToList();

                    var dkList_topL = _taxD_DK_DataValues
                        .Where(td => taxonIdList_topL.Contains(td.TaxonId))
                        .Select(td => td.DescriptionKeyId).Distinct().ToList();

                    taxonIdList = taxonIdList.Intersect(taxonIdList_topL).ToList();
                    dkList = dkList.Intersect(dkList_topL).ToList();
                }
                /**
                if (_positionFilterDks != null)
                {
                    dkList = dkList.Intersect(_positionFilterDks).ToList();
                }
                if (_genderFilterId != null)
                {
                    var dkList_gender = _detContext.TaxonDescription.Where(td => td.DescriptionKeyTypeId == _genderFilterId).Select(td => td.DescriptionKeyId).ToList();
                    dkList = dkList.Intersect(dkList_gender).ToList();
                }
                **/
                //create list with DKIds and prediction values (badge values)
                Dictionary<int, int> result = new Dictionary<int, int>();
                foreach (int dkId in dkList)
                {
                    var temp_dkgid = _detContext.DescriptionKey.AsNoTracking().Where(dk => dk.DescriptionKeyId == dkId).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                    var predVal = CombineKeys(dkId, temp_dkgid, taxonIdList != null ? taxonIdList : null);
                    //rewrite below to also allow 0-value DKIds (as of 29-04-20)
                    //if (predVal > 0) { result.Add(dkId, predVal); }
                    result.Add(dkId, predVal);
                }
                return result;
            }

            private int CombineKeys(int descKeyId, int temp_dkgid, List<int> taxonIdList)
            {
                var temp_selected_dkg_dk_dict = _dkg_dk_dict;
                //is key in same keygroup with one of selected keys?
                if (temp_selected_dkg_dk_dict.Keys.Contains(temp_dkgid))
                {
                    //Add selectedDKG to dkg-subgroup
                    //OR
                    temp_selected_dkg_dk_dict[temp_dkgid].Add(descKeyId);
                }
                else
                {
                    //AND operator
                    temp_selected_dkg_dk_dict.Add(temp_dkgid, new List<int> { descKeyId });
                }

                List<int> taxDesc_OR = null;
                HashSet<int> _uniqueTaxonIdList = null;

                if (taxonIdList == null)
                {
                    _uniqueTaxonIdList = new HashSet<int>(_taxD_DK_DataValues.Select(td => td.TaxonId).Distinct().ToList());
                }
                else
                {
                    _uniqueTaxonIdList = new HashSet<int>(taxonIdList);
                }

                /** VALUE DKs first */
                if (_dataValues != null)
                {
                    foreach (KeyValuePair<int, decimal> dk in _dataValues)
                    {
                        var taxDescInst = _taxD_DK_DataValues.Where(td => td.DescriptionKeyId == dk.Key && td.MinValue <= dk.Value && td.MaxValue >= dk.Value).Select(td => td.TaxonId).Distinct().ToList();
                        _uniqueTaxonIdList.IntersectWith(taxDescInst);
                    }
                }

                /** OTHER DKs **/
                foreach (KeyValuePair<int, List<int>> dkg in temp_selected_dkg_dk_dict)
                {
                    //Single param in one dkg
                    if (dkg.Value.Count == 1)
                    {
                        taxDesc_OR = _taxD_DK_DataValues.Where(td => td.DescriptionKeyId == dkg.Value.FirstOrDefault()).Select(td => td.TaxonId).Distinct().ToList();
                    }
                    else if (dkg.Value.Count > 1)
                    {
                        taxDesc_OR = _taxD_DK_DataValues.Where(td => dkg.Value.Contains(td.DescriptionKeyId)).Select(td => td.TaxonId).Distinct().ToList();
                    }
                    _uniqueTaxonIdList.IntersectWith(taxDesc_OR);
                }

                temp_selected_dkg_dk_dict[temp_dkgid].Remove(descKeyId);
                if (temp_selected_dkg_dk_dict[temp_dkgid].Count < 1) { temp_selected_dkg_dk_dict.Remove(temp_dkgid); }

                return _uniqueTaxonIdList.Count;
            }

            private void ApplyLLFilterToTaxa()
            {
                if (_llFilter != "")
                {
                    //comes in as 'family', 'subfamily', 'order', etc.
                    var hierarchyLevel = _infContext.TaxonomyState.AsNoTracking().Where(ts => _llFilter == ts.StateDescription).Select(ts => ts.HierarchyLevel).FirstOrDefault();
                    var higherHierarchyLevels = _infContext.TaxonomyState.AsNoTracking().Where(ts => hierarchyLevel > ts.HierarchyLevel).Select(ts => ts.StateId).ToList();

                    var taxon_lowLevelFiltered = _infContext.Taxon
                        .AsNoTracking()
                        .Where(tax => higherHierarchyLevels.Contains(tax.TaxonomyStateId.Value))
                        .Select(tx => tx.TaxonId).ToList();
                    _uniqueTaxonIdList.IntersectWith(taxon_lowLevelFiltered);
                    //_taxIds_lowLevelFiltered = taxon_lowLevelFiltered;
                    //topLevelFilterId = paramKey_tlf;            }

                }
            }

            private void ApplyTLFilterToTaxa()
            {
                //var paramKey_tlf = Request.Query[key].ToString().Trim();
                var taxon_topLevelFiltered = _infContext.Taxon
                    .AsNoTracking()
                    .Where(tax => tax.KingdomId == _tlFilter || tax.PhylumId == _tlFilter || tax.SubphylumId == _tlFilter || tax.ClassId == _tlFilter || tax.SubclassId == _tlFilter || tax.OrderId == _tlFilter || tax.SuborderId == _tlFilter || tax.FamilyId == _tlFilter || tax.SubfamilyId == _tlFilter || tax.GenusId == _tlFilter)
                    .Select(tx => tx.TaxonId).ToList();
                _uniqueTaxonIdList.IntersectWith(taxon_topLevelFiltered);
                //_taxIds_topLevelFiltered = taxon_topLevelFiltered;
                //_topLevelFilterId = paramKey_tlf;
            }

            private bool ApplyGGFilterToTaxa()
            {
                List<int> taxon_dropDFiltered = new List<int>();
                if (_ggFilter == "Bodentiere")
                {
                    taxon_dropDFiltered = _infContext.Taxon
                        .AsNoTracking()
                        .Where(tax => tax.Group.Contains(_ggFilter.Trim()))
                        .Select(tx => tx.TaxonId).ToList();
                    _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                    return true;
                }
                else
                {
                    taxon_dropDFiltered = _infContext.Taxon
                        .AsNoTracking()
                        .Where(tax => tax.Group.Contains("regular"))
                        .Select(tx => tx.TaxonId).ToList();
                    _uniqueTaxonIdList.IntersectWith(taxon_dropDFiltered);
                    return false;
                }
                //_taxIds_importGroupFiltered = taxon_dropDFiltered;
            }

            private void ParseDataAndDkValues(Dictionary<string, string> q)
            {
                Dictionary<int, decimal> result = new Dictionary<int, decimal>();
                _dkg_dk_dict = new Dictionary<int, List<int>>();
                foreach (String key in q.Keys)
                {
                    if (key.ToString().Contains("data_value_"))
                    {
                        string dkId = key.ToString().Replace("data_value_", "");
                        var style = NumberStyles.AllowDecimalPoint;
                        var queryString = Regex.Replace(q[key].ToString(), "[^0-9.,]", "");

                        if (Int32.TryParse(dkId, out int paramKey_dk) && Decimal.TryParse(queryString, style, CultureInfo.InvariantCulture, out decimal paramVal_dk_1))
                        {
                            List<int> taxDescInst = new List<int>(); _dataValues.Add(paramKey_dk, paramVal_dk_1);
                            taxDescInst = _detContext.TaxonDescription
                            .AsNoTracking()
                            .Where(td => td.DescriptionKeyId == paramKey_dk && td.MinValue <= paramVal_dk_1 && td.MaxValue >= paramVal_dk_1)
                            .Select(td => td.TaxonId).Distinct().ToList();
                            _uniqueTaxonIdList.IntersectWith(taxDescInst);
                        }
                    }
                    else if (Int32.TryParse(key, out int paramKey_dkg))
                    {
                        List<int> taxDesc_OR = null;

                        //Single param in one dkg
                        if (Int32.TryParse(q[key], out int paramVal_dk_2))
                        {
                            taxDesc_OR = _detContext.TaxonDescription
                                .AsNoTracking()
                                .Where(td => td.DescriptionKeyId == paramVal_dk_2)
                                .Select(td => td.TaxonId).Distinct().ToList();
                            var dkg_id = _detContext.DescriptionKey
                                .AsNoTracking()
                                .Where(dk => dk.DescriptionKeyId == paramVal_dk_2).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                            _dkg_dk_dict.Add(dkg_id, new List<int>() { paramVal_dk_2 });
                        }
                        else if (q[key].ToString().Contains(","))
                        {
                            var dKList = q[key].ToString().Split(",").Select(Int32.Parse).ToList();
                            taxDesc_OR = _detContext.TaxonDescription.AsNoTracking().Where(td => dKList.Contains(td.DescriptionKeyId)).Select(td => td.TaxonId).Distinct().ToList();
                            var dkg_id = _detContext.DescriptionKey.AsNoTracking().Where(dk => dKList.Contains(dk.DescriptionKeyId)).Select(dk => dk.DescriptionKeyGroupId).FirstOrDefault();
                            _dkg_dk_dict.Add(dkg_id, dKList);
                        }
                        _uniqueTaxonIdList.IntersectWith(taxDesc_OR);
                        //_taxIds_dkFiltered = taxDesc_OR;
                    }
                }
            }

            /**TODO: how to make this more generic?? **/
            public static List<int> _getTaxonIdsByHierarchyLevel(Taxon taxonGroupInst, string hierLevel)
            {
                List<int> taxon_dropDFiltered = null;
                switch (hierLevel)
                {
                    case "KingdomId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.KingdomId == taxonGroupInst.TaxonId && tax.PhylumId == null && tax.TaxonomyState.StateDescription == "phylum")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "PhylumId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.PhylumId == taxonGroupInst.TaxonId && tax.ClassId == null && tax.TaxonomyState.StateDescription == "class")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "SubphylumId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.PhylumId == taxonGroupInst.TaxonId && tax.ClassId == null && tax.TaxonomyState.StateDescription == "class")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "ClassId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => (tax.ClassId == taxonGroupInst.TaxonId) && (tax.TaxonomyState.StateDescription == "order"))
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "SubclassId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.ClassId == taxonGroupInst.TaxonId && tax.TaxonomyState.StateDescription == "order")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    //currently order is the second to last level with taxonDescriptions --> only species underneath
                    case "OrderId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.OrderId == taxonGroupInst.TaxonId && tax.SpeciesId == null && tax.TaxonomyState.StateDescription == "species")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "SuborderId":
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.OrderId == taxonGroupInst.TaxonId && tax.SpeciesId == null && tax.TaxonomyState.StateDescription == "species")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "FamilyId":
                        //enable Subfamily when available in excel import
                        taxon_dropDFiltered = _infContext.Taxon
                            .AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.FamilyId == taxonGroupInst.TaxonId && tax.SpeciesId == null && tax.TaxonomyState.StateDescription == "species")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    case "SubfamilyId":
                        //enable Subfamily when available in excel import
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Include(tax => tax.TaxonomyState)
                            .Where(tax => tax.FamilyId == taxonGroupInst.TaxonId && tax.SpeciesId == null && tax.TaxonomyState.StateDescription == "species")
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                    default:
                        taxon_dropDFiltered = _infContext.Taxon.AsNoTracking()
                            .Select(tx => tx.TaxonId)
                            .ToList();
                        break;
                }
                return taxon_dropDFiltered;
            }
        }

        public class TaxD_DK_DataValues
        {
            public int TaxonId { get; set; }
            public int DescriptionKeyId { get; set; }
            public decimal? MaxValue { get; set; }
            public decimal? MinValue { get; set; }

        }
    }
}