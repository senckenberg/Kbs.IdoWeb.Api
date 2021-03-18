using System;
using System.Collections.Generic;
using System.Linq;
using Kbs.IdoWeb.Data.Observation;
using Kbs.IdoWeb.Data.Information;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Web;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {

        private readonly ObservationContext _obsContext;
        private readonly InformationContext _infContext;

        public ImagesController(ObservationContext obsContext, InformationContext infContext)
        {
            _obsContext = obsContext;
            _infContext = infContext;
        }

        // GET: api/bytaxon/
        [HttpGet("bytaxon/{taxonid}")]
        public ActionResult<Image> GetImageByTaxonId(int taxonid)
        {
            try
            {
                var imagesByTaxonId = _obsContext.Image
                    .Include(img => img.License)
                    .Select(img => new { img.TaxonId, img.ImagePath, img.Author, img.CopyrightText, img.Description, img.License.LicenseName, img.License.LocalisationJson })
                    .Where(img => img.TaxonId == taxonid).ToList();
                return Content(JsonConvert.SerializeObject(imagesByTaxonId));
            }
            catch (Exception e)
            {
                var exp = e;
                return Content(JsonConvert.SerializeObject("Error in API"));
                throw (e);
            }
        }

        [HttpGet("byname/{imagename}")]
        public object GetImageByName(string imageName)
        {
            try
            {
                var imageData = _obsContext.Image
                    .Include(img => img.License)
                    .Select(img => new { img.ImagePath, img.Author, img.CopyrightText, img.Description, img.License.LicenseName, img.License.LicenseLink, img.License.LocalisationJson, img.TaxonId })
                    .Where(img => img.ImagePath == imageName)
                    .ToList();

                if(imageData != null)
                {
                    var taxonMap = _infContext.Taxon
                        .Where(tax => imageData.Select(img => img.TaxonId).Contains(tax.TaxonId))
                        .Select(tax => new { tax.TaxonId, tax.TaxonName })
                        .ToDictionary(x => x.TaxonId, y=>y.TaxonName);

                    var result = imageData.Select(img => new { img.ImagePath, img.Author, img.CopyrightText, img.Description, img.LicenseName, img.LicenseLink, img.LocalisationJson, TaxonName = img.TaxonId.HasValue ? taxonMap[img.TaxonId.Value]:null });

                    return Content(JsonConvert.SerializeObject(result));
                }

            }
            catch (System.Exception e)
            {
                var exp = e;
                throw (e);
            }

            return null;
        }

        [HttpGet("getImageApproved/{imagePath}")]
        public ActionResult GetImageApproved(string imagePath)
        {
            int fileExtPos = imagePath.LastIndexOf(".");
            imagePath = HttpUtility.UrlDecode(imagePath);
            if (fileExtPos >= 0)
                imagePath = imagePath.Substring(0, fileExtPos);

            var response = _obsContext.Image
                .FirstOrDefault(img => EF.Functions.Like(img.ImagePath, imagePath+"%"))?.IsApproved;

            if(response != null)
            {
                return Content(JsonConvert.SerializeObject(response));
            }
            return Content(null);
        }

        [HttpGet("getImageApprovedById/{cmsId}")]
        public ActionResult GetImageApprovedById(int cmsId)
        {
            var response = _obsContext.Image
                .FirstOrDefault(img => img.CmsId == cmsId)?.IsApproved;

            if (response != null)
            {
                return Content(JsonConvert.SerializeObject(response));
            }
            return Content(null);
        }

        [HttpPost("setImageApproved")]
        public ActionResult SetImageApproved(ImageRequest requestData)
        {
            try
            {
                var imagePath = requestData.ImagePath;
                imagePath = HttpUtility.UrlDecode(imagePath);
                int fileExtPos = imagePath.LastIndexOf(".");
                if (fileExtPos >= 0)
                    imagePath = imagePath.Substring(0, fileExtPos);
                var isApprovedState = requestData.IsApprovedState;
                Image image = _obsContext.Image
                    .FirstOrDefault(img => EF.Functions.Like(img.ImagePath, "%" + imagePath + "%"));

                if (image != null)
                {
                    image.IsApproved = isApprovedState;
                    _obsContext.Update(image);
                    _obsContext.SaveChangesAsync();
                    return Content(JsonConvert.SerializeObject("succes"));
                }

            } catch(Exception e)
            {
                return Content(JsonConvert.SerializeObject(e));
            }
            return Content("error");
        }

        [HttpPost("setImageApprovedById")]
        public ActionResult SetImageApprovedById(ImageRequest requestData)
        {
            try
            {
                var cmsId = requestData.CmsId;
                var isApprovedState = requestData.IsApprovedState;

                Image image = _obsContext.Image
                    .FirstOrDefault(img => img.CmsId == cmsId);

                if (image != null)
                {
                    image.IsApproved = isApprovedState;
                    _obsContext.Update(image);
                    _obsContext.SaveChangesAsync();
                    return Content(JsonConvert.SerializeObject("succes"));
                }

            }
            catch (Exception e)
            {
                return Content(JsonConvert.SerializeObject(e));
            }
            return Content("error");
        }

        [HttpPost("UploadImage")]
        public ActionResult UploadImage (List<Image> newImages)
        {
            try
            {
                if (newImages != null)
                {
                    foreach(Image imgItem in newImages) {
                        //TODO: check license default
                        imgItem.LicenseId = 1;
                        string taxonName = _infContext.Taxon.Where(t => t.TaxonId == imgItem.TaxonId).Select(t => t.TaxonName).FirstOrDefault();
                        imgItem.TaxonName = taxonName;
                        //imgItem.UserId = user.Id;
                        //imgItem.ObservationId = obs.ObservationId;
                        //var fileName = Path.GetFileName(imgItem.ImagePath);
                        //var physicalPath = Path.Combine(Hosting.HostingEnvironment.MapPath("~/App_Data"), fileName);
                        // The files are not actually saved in this demo
                        // file.SaveAs(physicalPath);
                        _obsContext.Add(imgItem);
                    }
                    _obsContext.SaveChanges();
                    return Content("");
                }
            } catch (Exception e)
            {
                //TODO catch
            }
            return Content("error");
        }

        public class ImageRequest
        {
            public int CmsId;
            public string ImagePath;
            public bool IsApprovedState;
        }
    }
}