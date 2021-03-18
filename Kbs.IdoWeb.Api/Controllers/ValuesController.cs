using Kbs.IdoWeb.Api.Middleware;
using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DeterminationContext _detContext;
        private readonly InformationContext _infContext;
        private readonly ObservationContext _obsContext;
        private readonly MappingContext _mapContext;
        private readonly IConfiguration _smtpConfig;
        public ValuesController(DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext, IConfiguration smtpConfiguration )
        {
            _detContext = detContext;
            _infContext = infContext;
            _obsContext = obsContext;
            _mapContext = mapContext;
            _smtpConfig = smtpConfiguration;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Content(JsonConvert.SerializeObject(_obsContext.ImageLicense), "application/json");

        }

        // GET api/values/inf/Taxon
        [HttpGet("{context}/{id}")]
        [Authorize]
        public ActionResult<string> Get(string context, string id)
        {
            DbContext dbContext;
            if (_detContext.GetType().Name.StartsWith(context, StringComparison.CurrentCultureIgnoreCase))
            {
                dbContext = _detContext;
            }
            else if (_infContext.GetType().Name.StartsWith(context, StringComparison.CurrentCultureIgnoreCase))
            {
                dbContext = _infContext;
            }
            else if (_obsContext.GetType().Name.StartsWith(context, StringComparison.CurrentCultureIgnoreCase))
            {
                dbContext = _obsContext;
            }
            else if (_mapContext.GetType().Name.StartsWith(context, StringComparison.CurrentCultureIgnoreCase))
            {
                dbContext = _mapContext;
            }
            else
            {
                throw new Exception();
            }
            var table = dbContext.GetType().GetProperty(id);
            var entity = table?.GetValue(dbContext, null);
            return Content(JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
        }

        [HttpGet("Mail/SendFeedback")]
        // GET api/values/Mail/SendFeedback
        public ActionResult<string> SendFeedback(string text, string adress)
        {
            string result;
            try
            {
                EMail.SendFeedbackMail(adress, text, _smtpConfig);
                result = "success";
            }
            catch (Exception ex)
            {
                result = "Message: " + ex.Message + " InnerException: " + ex.InnerException.Message;
                Trace.WriteLine(result);
            }
            return result;
        }
    }
}