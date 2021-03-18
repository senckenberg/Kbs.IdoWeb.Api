using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DescriptionKeyGroupsController : ControllerBase
    {
        private readonly DeterminationContext _detContext;
        private readonly InformationContext _infContext;
        private readonly ObservationContext _obsContext;
        private readonly MappingContext _mapContext;

        public DescriptionKeyGroupsController(DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext)
        {
            _detContext = detContext;
            _infContext = infContext;
            _obsContext = obsContext;
            _mapContext = mapContext;
        }

        // GET: api/descriptionKeyGroups
        [HttpGet]
        public ActionResult<IEnumerable<DescriptionKeyGroup>> GetDescriptionKeyGroupItems()
        {
            return Content(JsonConvert.SerializeObject(_detContext.DescriptionKeyGroup.AsNoTracking()), "application/json");
        }

        // GET: api/descriptionKeyGroups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DescriptionKeyGroup>> GetDescriptionKeyGroupItem(int? id)
        {
            var todoItem = await _detContext.DescriptionKeyGroup.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // GET: api/descriptionKeyGroups/compound
        [HttpGet("compound")]
        public ActionResult<DescriptionKeyGroup> GetDescriptionKeyGroupWithDescriptionKeys(int? descriptionKeyGroupId)
        {
            try
            {
                List<DescriptionKeyGroup> dkgs = null;
                if (descriptionKeyGroupId != null)
                {
                    dkgs = _detContext.DescriptionKeyGroup
                        .Where(dkg => dkg.DescriptionKeyGroupId == descriptionKeyGroupId)
                        .Include(dkg => dkg.DescriptionKey)
                        .OrderBy(dkg => dkg.OrderPriority)
                        .ThenBy(x => x.DescriptionKey.OrderBy(dk => dk.DescriptionKeyId)
                        .Select(dk => dk.DescriptionKeyId).FirstOrDefault())
                        .AsNoTracking()
                        .ToList();
                } else
                {
                    dkgs = _detContext.DescriptionKeyGroup
                        .Include(dkg => dkg.DescriptionKey)
                        .OrderBy(dkg => dkg.DescriptionKeyGroupId).ThenBy(dkg => dkg.OrderPriority)
                        .ThenBy(dkg => dkg.DescriptionKey.Select(dk => dk.DescriptionKeyId).FirstOrDefault())
                        //.Select(dk => dk.DescriptionKeyId).FirstOrDefault())
                        .AsNoTracking().ToList();

                }

                foreach (DescriptionKeyGroup dkg in dkgs)
                {
                    if (dkg.DescriptionKey.Count > 1)
                    {
                        dkg.DescriptionKey = dkg.DescriptionKey.OrderBy(dks => dks.DescriptionKeyId).ToList();
                    }
                }

                /**
                * disabled for testing 25.02.2020
                foreach (DescriptionKeyGroup item in dkgs)
                {
                    item.DescriptionKey = item.DescriptionKey.OrderBy(x => x.KeyName).ToList();
                }
                **/
                return Content(JsonConvert.SerializeObject(dkgs), "application/json");
            } catch (Exception e)
            {
                return Content(JsonConvert.SerializeObject(e));
            }
        }

        // GET: api/descriptionKeyGroups/5
        [HttpGet("viscats")]
        public ActionResult<VisibilityCategory> GetVisbilityCategories()
        {
            var visCats = _detContext.VisibilityCategory.ToList();
            if (visCats == null)
            {
                return NotFound();
            }
            return Content(JsonConvert.SerializeObject(visCats));
        }


        [HttpGet("positions")]
        public ActionResult<DescriptionKeyGroup> GetDescriptionKeyGroupLage()
        {
            var referencingDkgs = _detContext.DescriptionKeyGroup
                .Include(dkg => dkg.ParentDescriptionKeyGroup)
                .Where(dkg => dkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId != null)
                .Select(dkg => dkg.DescriptionKeyGroupId).ToList();

            List<int> referencedDKgs = _detContext.DescriptionKeyGroup
                .Where(dkg => dkg.ParentDescriptionKeyGroupId.HasValue)
                .Select(dkg => dkg.ParentDescriptionKeyGroupId.Value)
                .ToList();

            var result = _detContext.DescriptionKeyGroup
                .Include(dkg => dkg.InverseParentDescriptionKeyGroup)
                .Where(dkg => dkg.DescriptionKeyGroupDataType == null && dkg.ParentDescriptionKeyGroupId != null)
                .Select(dkg => new { dkg.KeyGroupName, dkg.DescriptionKeyGroupId,
                    ChildrenDescriptionKeyGroupIds = dkg.InverseParentDescriptionKeyGroup.Select(inv_dkg => new { inv_dkg.DescriptionKeyGroupId }).Distinct().ToList()
                }).ToList();

            var temp_Arr = result.ToList();
            foreach(var r in result)
            {
                if(r.KeyGroupName.Contains(";"))
                {
                    var multiNames = r.KeyGroupName.Split(';').Select(p => p.Trim()).ToList();
                    foreach(var name in multiNames)
                    {
                        temp_Arr.Add(new { KeyGroupName = name, DescriptionKeyGroupId = r.DescriptionKeyGroupId, ChildrenDescriptionKeyGroupIds = r.ChildrenDescriptionKeyGroupIds });
                    }
                }
            }
            temp_Arr.RemoveAll(s => s.KeyGroupName.Contains(";"));
            
            var result2 = temp_Arr.GroupBy(dkg => dkg.KeyGroupName);
            var clean_result = result2.Select(dkg_group => new {
                KeyGroupName = dkg_group.Key,
                DescriptionKeyGroupIds = dkg_group.Select(dkg_group_item => dkg_group_item.DescriptionKeyGroupId),
                ChildrenDescriptionKeyGroupIds = dkg_group.SelectMany(dkg_group_item => dkg_group_item.ChildrenDescriptionKeyGroupIds.Select(cdkg => cdkg.DescriptionKeyGroupId).Distinct().ToList()),
            }).ToList();

            return Content(JsonConvert.SerializeObject(clean_result));
        }

        [HttpGet("topmost")]
        public ActionResult<DescriptionKeyGroup> GetDescriptionKeyGroupRegion()
        {
            try
            {
                /** classFilter works on client-side through taxonResults **/
                var dkgs = _detContext.DescriptionKeyGroup
                    .Include(dkg => dkg.ParentDescriptionKeyGroup)
                        .ThenInclude(dkgp => dkgp.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup)
                    .Where(dkg => dkg.DescriptionKeyGroupDataType != null && (dkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId != null || dkg.ParentDescriptionKeyGroupId != null))
                    .AsEnumerable()
                    .GroupBy(dkg => dkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup ?? dkg.ParentDescriptionKeyGroup)
                    .Select(groupedDkg =>
                    {
                        //TODO: check how to combine two lists to one distinct
                        return new
                        {
                            Id = groupedDkg.Key.DescriptionKeyGroupId,
                            ParentParentIds = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId.Value).Distinct().ToList(),
                            ParentIds = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroupId.Value).Distinct().ToList(),
                            Ids = groupedDkg.Select(gdkg => gdkg.DescriptionKeyGroupId).Distinct().ToList(),
                            AllParentNames = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroup.KeyGroupName).Distinct().ToList(),
                            AllNames = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup == null).Select(gdkg => gdkg.KeyGroupName).Distinct().ToList(),
                            DkgName = groupedDkg.Key.KeyGroupName,
                            DkgParentName = groupedDkg.Key.ParentDescriptionKeyGroup?.ParentDescriptionKeyGroup?.KeyGroupName,
                            C = groupedDkg.Count()
                        };
                    })
                    .ToList();

                var dkgs_comb = dkgs.Select(dkg => {
                    return new
                    {
                        dkg.Id,
                        AllChildIds = dkg.ParentIds.Union(dkg.ParentParentIds).ToList().Union(dkg.Ids).Distinct(),
                        AllChildNames = dkg.AllParentNames.Where(dkg_item => dkg_item != dkg.DkgName).Union(dkg.AllNames.Where(dkg_item => dkg_item != dkg.DkgName)).ToList().Distinct(),
                        DkgName = dkg.DkgName,
                        DkgParentName = dkg.DkgParentName != null ? dkg.DkgParentName : "No Name",
                    };
                }).ToList();

                return Content(JsonConvert.SerializeObject(dkgs_comb), "application/json");

            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        [HttpGet("categories")]
        public ActionResult<DescriptionKeyGroup> GetDescriptionKeyGroupCategories(int? descriptionKeyGroupId)
        {
            try
            {
                /** classFilter works on client-side through taxonResults **/
                var dkgs = _detContext.DescriptionKeyGroup
                    .Include(dkg => dkg.ParentDescriptionKeyGroup)
                        .ThenInclude(dkgp => dkgp.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup)
                    .Where(dkg => dkg.DescriptionKeyGroupDataType != null && (dkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId != null || dkg.ParentDescriptionKeyGroupId != null))
                    .AsEnumerable()
                    .GroupBy(dkg => dkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup ?? dkg.ParentDescriptionKeyGroup)
                    .Select(groupedDkg =>
                    {
                        //TODO: check how to combine two lists to one distinct
                        return new
                        {
                            Id = groupedDkg.Key.DescriptionKeyGroupId,
                            ParentParentIds = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroupId.Value).Distinct().ToList(),
                            ParentIds = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroupId.Value).Distinct().ToList(),
                            Ids = groupedDkg.Select(gdkg => gdkg.DescriptionKeyGroupId).Distinct().ToList(),
                            AllParentNames = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroupId.HasValue).Select(gdkg => gdkg.ParentDescriptionKeyGroup.KeyGroupName).Distinct().ToList(),
                            AllNames = groupedDkg.Where(gdkg => gdkg.ParentDescriptionKeyGroup.ParentDescriptionKeyGroup == null).Select(gdkg => gdkg.KeyGroupName).Distinct().ToList(),
                            DkgName = groupedDkg.Key.KeyGroupName,
                            DkgParentName = groupedDkg.Key.ParentDescriptionKeyGroup?.ParentDescriptionKeyGroup?.KeyGroupName,
                            C = groupedDkg.Count()
                        };
                    })
                    .ToList();

                var dkgs_comb = dkgs.Select(dkg => {
                    return new
                    {
                        dkg.Id,
                        AllChildIds = dkg.ParentIds.Union(dkg.ParentParentIds).ToList().Union(dkg.Ids).Distinct(),
                        AllChildNames = dkg.AllParentNames.Where(dkg_item => dkg_item != dkg.DkgName).Union(dkg.AllNames.Where(dkg_item => dkg_item != dkg.DkgName)).ToList().Distinct(),
                        DkgName = dkg.DkgName,
                        DkgParentName = dkg.DkgParentName != null? dkg.DkgParentName : "No Name",
                    };
                }).ToList();

                return Content(JsonConvert.SerializeObject(dkgs_comb), "application/json");

            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        // POST: api/descriptionKeyGroups/create
        [HttpPost("create")]
        public async Task<ActionResult<DescriptionKeyGroup>> PostDescriptionKeyGroupItem(DescriptionKeyGroup item)
        {
            try
            {
                _detContext.DescriptionKeyGroup.Add(item);
                await _detContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDescriptionKeyGroupItem), new { id = item.DescriptionKeyGroupId }, item);
            } catch (System.Exception e)
            {
                var exp = e;
            }
            return null;
        }


        // PUT: api/descriptionKeyGroups/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutTodoItem(int? id, DescriptionKeyGroup item)
        {
            if (id != item.DescriptionKeyGroupId)
            {
                return BadRequest();
            }

            _detContext.Entry(item).State = EntityState.Modified;
            await _detContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/descriptionKeyGroups/destroy/5
        [HttpDelete("destroy/{id}")]
        public async Task<IActionResult> DeleteDescriptionKeyGroupItem(int? id)
        {
            var descriptionKeyGroupItem = await _detContext.DescriptionKeyGroup.FindAsync(id);

            if (descriptionKeyGroupItem == null)
            {
                return NotFound();
            }

            _detContext.DescriptionKeyGroup.Remove(descriptionKeyGroupItem);
            await _detContext.SaveChangesAsync();

            return NoContent();
        }
    }
}