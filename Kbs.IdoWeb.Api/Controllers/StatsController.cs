using Kbs.IdoWeb.Data.Authentication;
using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Location;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Kbs.IdoWeb.Data.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly ObservationContext _obsContext;
        private readonly LocationContext _locContext;
        private readonly InformationContext _infContext;
        private readonly PublicContext _idoContext;
        private readonly UserManager<ApplicationUser> _userManager;

        /***
        ** Advice = Event + Observation
        ** Event = Fundort
        ** Observation = Fund
        ** Observation = ObservationInfo + optional List of Images
        ***/
        public StatsController(UserManager<ApplicationUser> userManager, DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext, LocationContext locContext, PublicContext idoContext)
        {
            _userManager = userManager;
            _obsContext = obsContext;
            _locContext = locContext;
            _idoContext = idoContext;
            _infContext = infContext;
        }

        [HttpGet("GetStatistics")]
        public ActionResult<IEnumerable<Object>> GetStatistics ()
        {
            try
            {
                //Number of advices
                //Beobachtungen, Meldungen, Steckbriefe, Fotos, Arten mit Fund, Arten mit Fotos 

                //Meldungen Freigegeben
                var nrOfAdvices = _obsContext.Observation.Where(i => i.ApprovalStateId >= 5).ToList().Count;

                //Steckbriefe
                var nrSteckbriefe = _infContext.Taxon.Where(i => i.TaxonDescription != null).ToList().Count;

                //Anzahl Fotos
                var nrPhotos = _obsContext.Image.Where(i => i.IsApproved == true).Distinct().ToList().Count;

                //Arten mit Fund
                var nrSpeciesWithObs = _obsContext.Observation.Where(i => i.ApprovalStateId >= 5).Select(i => i.TaxonId).Distinct().Count();

                //Arten mit Fotos
                var nrSpeciesPhotos = _obsContext.Image.Select(i => i.TaxonId).Distinct().ToList().Count;

                return Content(JsonConvert.SerializeObject(new { nrOfAdvices, nrSteckbriefe, nrPhotos, nrSpeciesWithObs, nrSpeciesPhotos }), "application/json");

            } catch (Exception ex)
            {
                var dbg = ex.InnerException;
                return null;
            }
        }
    }
}