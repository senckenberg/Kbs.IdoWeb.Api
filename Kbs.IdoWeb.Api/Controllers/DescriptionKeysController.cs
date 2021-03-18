using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DescriptionKeysController : ControllerBase
    {
        private readonly DeterminationContext _detContext;
        private readonly InformationContext _infContext;
        private readonly ObservationContext _obsContext;
        private readonly MappingContext _mapContext;

        public DescriptionKeysController(DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext)
        {
            _detContext = detContext;
            _infContext = infContext;
            _obsContext = obsContext;
            _mapContext = mapContext;
        }

        // GET: api/descriptionKey
        [HttpGet]
        public ActionResult<IEnumerable<DescriptionKey>> GetDescriptionKeyItems(int? descriptionKeyGroupId)
        {
            if(descriptionKeyGroupId != null)
            {
                var descKeys = _detContext.DescriptionKey.Where(dk => dk.DescriptionKeyGroupId == descriptionKeyGroupId).AsNoTracking();
                return Content(JsonConvert.SerializeObject(descKeys), "application/json");
            }
            return Content(JsonConvert.SerializeObject(_detContext.DescriptionKey.AsNoTracking()), "application/json");
        }

        // GET: api/descriptionKey/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DescriptionKey>> GetDescriptionKeyItem(int? id)
        {
            var todoItem = await _detContext.DescriptionKey.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/descriptionKey/create
        [HttpPost("create")]
        public async Task<ActionResult<DescriptionKey>> PostDescriptionKeyItem(DescriptionKey item)
        {
            try
            {
                _detContext.DescriptionKey.Add(item);
                await _detContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDescriptionKeyItem), new { id = item.DescriptionKeyId }, item);
            } catch (System.Exception e)
            {
                var exp = e;
            }
            return null;
        }



        // PUT: api/descriptionKey/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutTodoItem(int? id, DescriptionKey item)
        {
            if (id != item.DescriptionKeyId)
            {
                return BadRequest();
            }

            _detContext.Entry(item).State = EntityState.Modified;
            await _detContext.SaveChangesAsync();

            return NoContent();
        }

        

        // DELETE: api/descriptionKey/destroy/5
        [HttpDelete("destroy/{id}")]
        public async Task<IActionResult> DeleteDescriptionKeyItem(int? id)
        {
            var descriptionKeyItem = await _detContext.DescriptionKey.FindAsync(id);

            if (descriptionKeyItem == null)
            {
                return NotFound();
            }

            _detContext.DescriptionKey.Remove(descriptionKeyItem);
            await _detContext.SaveChangesAsync();

            return NoContent();
        }
    }
}