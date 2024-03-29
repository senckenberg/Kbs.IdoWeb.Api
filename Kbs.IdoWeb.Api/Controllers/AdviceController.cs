﻿using Kbs.IdoWeb.Data.Authentication;
using Kbs.IdoWeb.Data.Determination;
using Kbs.IdoWeb.Data.Information;
using Kbs.IdoWeb.Data.Location;
using Kbs.IdoWeb.Data.Mapping;
using Kbs.IdoWeb.Data.Observation;
using Kbs.IdoWeb.Data.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static Kbs.IdoWeb.Api.Controllers.AdviceController;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdviceController : ControllerBase
    {
        private readonly ObservationContext _obsContext;
        private readonly LocationContext _locContext;
        private readonly InformationContext _infContext;
        private readonly PublicContext _idoContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationSettings _appSettings;
        private readonly List<int> positiveApprovalStates = new List<int>(new int[] { 3, 5, 6 });
        private string _userId;
        private string imgSavePath_dev = "../public_html/wp-content/uploads/user_uploads/";
        private string imgSavePath_general_dev = "../public_html/wp-content/uploads/";
        private string imgSavePath_general = "../public_html/wp-content/uploads/";
        private string imgSavePath = "../public_html/wp-content/uploads/user_uploads/";
        private string imgPublicUrl = "https://bodentierhochvier.de/wp-content/uploads/";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static List<Observation> observationListByUser;

        /***
        ** Advice = Event + Observation
        ** Event = Fundort
        ** Observation = Fund
        ** Observation = ObservationInfo + optional List of Images
        ***/
        public AdviceController(UserManager<ApplicationUser> userManager, DeterminationContext detContext, InformationContext infContext, ObservationContext obsContext, MappingContext mapContext, LocationContext locContext, PublicContext idoContext, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _obsContext = obsContext;
            _locContext = locContext;
            _idoContext = idoContext;
            _infContext = infContext;
            _appSettings = appSettings.Value;
        }

        /**
        ** create ADVICE (EVENT + OBSERVATION)
        **/
        [HttpPost("SaveAdvice")]
        [Authorize]
        //POST : /api/Advice/SaveAdvice
        public async Task<ActionResult> PostSaveAdvice(AdviceObject adviceObject)
        {
            try
            {

                _userId = User.Claims.First(i => i.Type == "UserId").Value;
                ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                var user = await _userManager.FindByIdAsync(_userId);
                var roles = await _userManager.GetRolesAsync(user);
                Event adviceEvent = new Event();

                if (adviceObject.EventId != null)
                {
                    adviceEvent = _obsContext.Event.Where(e => e.EventId == adviceObject.EventId).FirstOrDefault();
                    if (adviceEvent == null)
                    {
                        adviceEvent = adviceObject;
                        adviceEvent.UserId = _userId;
                        adviceEvent.AuthorName = adviceObject.AuthorName;
                        adviceEvent.TkNr = await GetTk25Id(adviceEvent.LatitudeDecimal, adviceEvent.LongitudeDecimal);
                        _obsContext.Event.Add(adviceEvent);
                        _obsContext.SaveChanges();
                    }
                }

                foreach (ObservationImage adviceObservation in adviceObject.ObservationList)
                {
                    adviceObservation.EventId = adviceEvent.EventId;
                    adviceObservation.UserId = _userId;
                    adviceObservation.LastEditDate = DateTime.Now;

                    if (adviceObservation.ApprovalStateId == null)
                    {
                        adviceObservation.ApprovalStateId = 1;
                    }

                    if (adviceObservation.TaxonGuid == null)
                    {
                        adviceObservation.TaxonGuid = _infContext.Taxon.FirstOrDefault(t => t.TaxonId.Equals(adviceObservation.TaxonId)).Identifier;
                    }

                    foreach (Image img in adviceObservation.Image)
                    {
                        img.ImagePath = "user_uploads/" + img.ImagePath;
                        img.CmsId = img.CmsId;
                        img.Description = adviceObservation.ObservationComment;
                        img.CopyrightText = img.CopyrightText;
                        img.LicenseId = 1;
                        img.UserId = _userId;
                        img.Author = adviceEvent.AuthorName;
                        img.TaxonId = adviceObservation.TaxonId;
                        img.TaxonName = adviceObservation.TaxonName;
                        _obsContext.Add(img);
                    }

                    _obsContext.Observation.Add(adviceObservation);
                    _obsContext.SaveChanges();
                    var obsId = adviceObservation.ObservationId;

                    _obsContext.SaveChanges();
                }

                _obsContext.SaveChanges();
                return Ok(new { succeeded = true, errors = new string[] { } });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException;
                return Ok(new { succeeded = false, errors = new string[] { } });
            }
        }


        [HttpPost("SyncAdvices")]
        public string SyncAdvices(AuthorizationJson auth)
        {
            AspNetUserDevices devInfo = new AspNetUserDevices();
            var userId = _idoContext.AspNetUserDevices.Where(dev => dev.DeviceId == auth.DeviceId && dev.DeviceHash == auth.DeviceHash).Select(dev => dev.UserId).FirstOrDefault();

            if (userId != null)
            {
                try
                {
                    //var membershipUser = Membership.GetUser(userData.UserId);
                    string st = $@"{userId}-{auth.DeviceId}";
                    var result = _obsContext.LastSyncVersion.Where(lsv => lsv.SyncTypeName == st).ToList();

                    List<Observation> returnObs = new List<Observation>();

                    if (result.Count() < 1)
                    {
                        returnObs = _obsContext.Observation.Where(o => o.UserId == userId).ToList();
                        LastSyncVersion newLsv = new LastSyncVersion { SyncTypeName = st, VersionDate = DateTime.Now, DeviceId = auth.DeviceId, UserId = userId };
                        _obsContext.Add(newLsv);
                    }
                    else
                    {
                        foreach (LastSyncVersion lsvItem in result)
                        {
                            List<Observation> temp = _obsContext.Observation.Where(o => o.UserId == userId && o.LastEditDate >= lsvItem.VersionDate).ToList();
                            returnObs.AddRange(temp);
                            lsvItem.VersionDate = DateTime.Now;
                        }
                        _obsContext.UpdateRange(result);
                    }
                    _obsContext.SaveChanges();
                    //var resultIds = _obsContext.Observation.Where(o => o.UserId == userId && o.Version > ).Select(lsv => lsv.U).ToList();
                    return JsonConvert.SerializeObject(returnObs);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return JsonConvert.SerializeObject("invalid user");
        }

        /**
        ** create ADVICE (EVENT + OBSERVATION)
        ** comes from mobile
        **/
        [HttpPost("SyncAdviceList/Mobile")]
        //POST : /api/Advice/SyncAdviceList/Mobile
        public async Task<ActionResult> PostSyncAdviceListFromMobile([FromBody] SyncRequest syncRequest)
        {
            try
            {
                Logger.Info(JsonConvert.SerializeObject(syncRequest));
                //_userId = User.Claims.First(i => i.Type == "UserId").Value;
                AspNetUserDevices devInfo = new AspNetUserDevices();
                List<string> userIdList = _idoContext.AspNetUserDevices.Where(dev => dev.DeviceId == syncRequest.DeviceId && dev.DeviceHash == syncRequest.DeviceHash).Select(dev => dev.UserId).ToList();
                ApplicationUser _user = null;
                foreach (string userId in userIdList)
                {
                    _user = _userManager.Users.FirstOrDefault(u => userId.Equals(u.Id) && u.UserName.Equals(syncRequest.UserName));
                    if (_user != null)
                    {
                        break;
                    }
                }

                if (_user != null)
                {
                    observationListByUser = _obsContext.Observation
                        .Where(i => i.UserId.Equals(_user.Id))
                        .Include(o => o.Image)
                        .Include(i => i.Image)
                        .Include(i => i.Event)
                        .ToList();

                    List<AdviceJsonItemSync> ajiList_server = observationListByUser.Select(i => ConvertToJsonForSync(i, _user.UserName)).ToList();

                    //List<AdviceJsonItemSync> syncedresultList = new List<AdviceJsonItemSync>();
                    List<AdviceJsonItemSync> toBeDeletedList = new List<AdviceJsonItemSync>();
                    List<AdviceJsonItemSync> toBeUpdatedList = new List<AdviceJsonItemSync>();

                    if (syncRequest.AdviceList.Count > 0)
                    {
                        foreach (AdviceJsonItemSync adviceObject in syncRequest.AdviceList)
                        {
                            if (adviceObject != null)
                            {
                                adviceObject.IsSynced = true;
                                AdviceJsonItemSync ajiList_server_item = ajiList_server.Where(i => i != null).FirstOrDefault(i => i.Identifier.Equals(adviceObject.Identifier));

                                //matching advice identfiers
                                if (ajiList_server_item != null)
                                {
                                    //advice previously synced, now to be deleted
                                    if (adviceObject.DeletionDate != null && ajiList_server_item.DeletionDate == null)
                                    {
                                        //DeleteObservationAsync(_obsContext.Observation.FirstOrDefault(i => i.ObservationId.Equals(ajiList_server_item.ObservationId)));
                                        toBeDeletedList.Add(ajiList_server_item);
                                    }

                                    //different property values?                            
                                    if (!ajiList_server_item.Md5Checksum.Equals(adviceObject.Md5Checksum))
                                    {
                                        //prevent null values for taxa from mobile
                                        if ((adviceObject.TaxonGuid != null && !Guid.Empty.Equals(adviceObject.TaxonGuid)))
                                        {
                                            //compare last edit
                                            if (adviceObject.LastEditDate != null && ajiList_server_item.LastEditDate != null)
                                            {
                                                var lastEditCompare = ((DateTime)adviceObject.LastEditDate).CompareTo((DateTime)ajiList_server_item.LastEditDate);

                                                //server newer
                                                if (lastEditCompare < 0)
                                                {
                                                    /*
                                                    if (ajiList_server_item.MobileAdviceId == null || ajiList_server_item.MobileAdviceId < 1 || ajiList_server_item.MobileAdviceId != adviceObject.MobileAdviceId)
                                                    {
                                                        Observation obs = _obsContext.Observation.FirstOrDefault(o => o.Identifier.Equals(adviceObject.Identifier));
                                                        obs.MobileAdviceId = adviceObject.MobileAdviceId;
                                                        _obsContext.Update(obs);
                                                    }
                                                    else
                                                    {

                                                    }
                                                    ajiList_server_item.MobileAdviceId = adviceObject.MobileAdviceId;
                                                    syncedresultList.Add(ajiList_server_item);
                                                    */
                                                }
                                                //equal
                                                else if (lastEditCompare == 0)
                                                {
                                                    //check for possible missing fields
                                                    /*
                                                    if (adviceObject.TaxonGuid == null && ajiList_server_item.TaxonGuid != null)
                                                    {
                                                        syncedresultList.Add(ajiList_server_item);
                                                    }
                                                    */
                                                    //something went wrong during last sync
                                                    //missing taxonguid? event updated?

                                                    //Mobile AdviceId not updated?
                                                    Logger.Warn($@"Last sync warning for advices {ajiList_server_item.Identifier}, {ajiList_server_item.GlobalAdviceId}");
                                                    Logger.Warn(ajiList_server_item);
                                                    Logger.Warn(adviceObject);
                                                    toBeUpdatedList.Add(ajiList_server_item);
                                                }
                                                //mobile newer
                                                else
                                                {
                                                    /*
                                                    if (adviceObject.GlobalAdviceId != ajiList_server_item.GlobalAdviceId)
                                                    {
                                                        adviceObject.GlobalAdviceId = ajiList_server_item.GlobalAdviceId;
                                                    }
                                                    syncedresultList.Add(adviceObject);
                                                    */
                                                    AdviceComplex ac = ConvertToAdviceComplexFromSync(adviceObject, _user.Id);
                                                    Logger.Info($@"Saving Advice Async {JsonConvert.SerializeObject(adviceObject)}");
                                                    SaveAdviceComplexAsync(ac, false);
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Syncing Error");
                                                Logger.Warn(ajiList_server_item);
                                                Logger.Warn(adviceObject);

                                                /**
                                                ajiList_server_item.MobileAdviceId = adviceObject.MobileAdviceId;
                                                syncedresultList.Add(ajiList_server_item);
                                                */
                                            }

                                        }
                                    }
                                    else if (ajiList_server_item.Images.Count != adviceObject.Images.Count)
                                    {
                                        Logger.Info("ajiList_server_item.Images.Count != adviceObject.Images.Count");
                                        if (adviceObject.Images != null)
                                        {
                                            int observationId = _obsContext.Observation.FirstOrDefault(o => o.Identifier == ajiList_server_item.Identifier).ObservationId;

                                            Logger.Info("ac.Images != null");
                                            foreach (AdviceImageJsonItem img_in in adviceObject.Images)
                                            {
                                                Logger.Info(JsonConvert.SerializeObject(img_in));
                                                try
                                                {
                                                    if (ajiList_server_item.Images != null)
                                                    {
                                                        //check if updated image
                                                        AdviceImageJsonItem img_tbu = ajiList_server_item.Images.FirstOrDefault(i => i.ImageName.Contains(img_in.ImageName));
                                                        if (img_tbu != null)
                                                        {
                                                            string img_tbu_FullPath = Path.Combine(imgSavePath, img_in.ImageName);
                                                            byte[] img_in_imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                                                            try
                                                            {
                                                                byte[] img_tbu_imageArray = System.IO.File.ReadAllBytes(img_tbu_FullPath);
                                                                string img_tbu_base64 = Convert.ToBase64String(img_tbu_imageArray);
                                                                if (img_tbu_base64 != img_in.ImageBase64)
                                                                {

                                                                    System.IO.File.WriteAllBytes(img_tbu_FullPath, img_in_imageBytes);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                //existing foto corrupt
                                                                Logger.Error(ex);
                                                                System.IO.File.Delete(img_tbu_FullPath);
                                                                System.IO.File.WriteAllBytes(img_tbu_FullPath, img_in_imageBytes);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            SaveNewImage(img_in, ajiList_server_item, _user.Id, observationId);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        SaveNewImage(img_in, ajiList_server_item, _user.Id, observationId);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Error(ex);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("ac.Images != null");
                                        }
                                        toBeUpdatedList.Add(ajiList_server_item);
                                    }
                                }
                                else
                                {
                                    //New mobile Advice Add
                                    AdviceComplex ac = ConvertToAdviceComplexFromSync(adviceObject, _user.Id);
                                    Logger.Info($@"Saving Advice Async {JsonConvert.SerializeObject(adviceObject)}");
                                    SaveAdviceComplexAsync(ac, true);
                                }
                            }

                        }


                        foreach (AdviceJsonItemSync deleteAdvice in toBeDeletedList)
                        {
                            Observation obs = _obsContext.Observation.FirstOrDefault(o => o.Identifier == deleteAdvice.Identifier);
                            if (obs != null)
                            {
                                obs.DeletionDate = deleteAdvice.DeletionDate;
                                _obsContext.Observation.Update(obs);
                                _obsContext.SaveChanges();
                            }
                        }

                        foreach (AdviceJsonItemSync updateAdvice in toBeUpdatedList)
                        {
                            Observation obs = _obsContext.Observation.FirstOrDefault(o => o.Identifier == updateAdvice.Identifier);
                            if (obs != null)
                            {
                                obs.MobileAdviceId = obs.ObservationId;
                                _obsContext.Observation.Update(obs);
                                _obsContext.SaveChanges();
                            }

                        }
                    }


                    //List<AdviceJsonItemSync> syncedresultList = observationListByUser.Select(i => ConvertToJsonForSync(i, _user.UserName)).ToList();
                    List<AdviceJsonItemSync> syncedresultList = _obsContext.Observation
                        .Where(i => i.UserId.Equals(_user.Id))
                        .Include(o => o.Image)
                        .Include(i => i.Image)
                        .Include(i => i.Event)
                        .Select(i => ConvertToJsonForSync(i, _user.UserName)).ToList();

                    try
                    {
                        foreach (AdviceJsonItemSync ajis in syncedresultList)
                        {
                            Observation obs = _obsContext.Observation.FirstOrDefault(o => o.Identifier == ajis.Identifier);
                            if (obs != null)
                            {
                                obs.IsSynced = true;
                                _obsContext.Observation.Update(obs);
                                _obsContext.SaveChanges();
                                ajis.IsSynced = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    return Ok(new { succeeded = true, errors = new string[] { }, AdviceList = syncedresultList });
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        public void SaveAdviceComplexAsync(AdviceComplex ac, bool doInsert)
        {
            try
            {
                Logger.Info("SaveAdviceComplexAsync");
                Logger.Info(JsonConvert.SerializeObject(ac));
                if (doInsert)
                {
                    Logger.Info("DoInsert");
                    //AdviceComplex ac = ConvertToAdviceComplexFromSync(adviceObject);
                    _obsContext.Event.Add(ac.Ev);
                    _obsContext.SaveChanges();
                    ac.Obs.EventId = ac.Ev.EventId;
                    ac.Obs.LastEditDate = TruncateDateTime(DateTime.Now, TimeSpan.FromSeconds(1));
                    ac.Obs.IsSynced = true;
                    _obsContext.Observation.Add(ac.Obs);
                    _obsContext.SaveChanges();

                    if (ac.Images != null)
                    {
                        foreach (AdviceImageJsonItem img_in in ac.Images)
                        {
                            try
                            {
                                byte[] imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                                string filename = Path.Combine(imgSavePath, img_in.ImageName);
                                System.IO.File.WriteAllBytes(filename, imageBytes);

                                //Method to save images @bodentierhochvier.de / wordpress
                                //--> imgName 
                                Image img_new = new Image();
                                img_new.ImagePath = "user_uploads/" + img_in.ImageName;
                                //img_new.CmsId = img.CmsId;
                                img_new.ObservationId = ac.Obs.ObservationId;
                                img_new.Description = ac.Obs.ObservationComment;
                                //img_new.CopyrightText = img.CopyrightText;
                                img_new.LicenseId = 1;
                                img_new.UserId = _userId;
                                img_new.Author = ac.Obs.AuthorName;
                                img_new.TaxonName = ac.Obs.TaxonName;
                                img_new.TaxonId = ac.Obs.TaxonId;
                                img_new.UserId = ac.Obs.UserId;
                                _obsContext.Add(img_new);
                                _obsContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Error saving images", ex.Message);
                            }
                        }
                    }

                }
                else
                {
                    Logger.Info("DontInsert");
                    Observation obs_tbu = _obsContext.Observation.Include(o => o.Event).Include(i => i.Image).FirstOrDefault(i => i.Identifier.Equals(ac.Obs.Identifier));
                    //obs_tbu.ObservationId = ac.Obs.ObservationId;
                    if (obs_tbu.MobileAdviceId == null || obs_tbu.MobileAdviceId != obs_tbu.ObservationId)
                    {
                        obs_tbu.MobileAdviceId = obs_tbu.ObservationId;
                    }
                    obs_tbu.TaxonId = ac.Obs.TaxonId;
                    obs_tbu.TaxonGuid = ac.Obs.TaxonGuid;
                    obs_tbu.TaxonName = ac.Obs.TaxonName;
                    obs_tbu.HabitatDate = ac.Obs.HabitatDate;
                    obs_tbu.AdviceCount = ac.Obs.AdviceCount;
                    obs_tbu.MaleCount = ac.Obs.MaleCount;
                    obs_tbu.FemaleCount = ac.Obs.FemaleCount;
                    obs_tbu.AuthorName = ac.Obs.AuthorName;
                    obs_tbu.LastEditDate = TruncateDateTime(ac.Obs.LastEditDate, TimeSpan.FromSeconds(1));
                    obs_tbu.DeletionDate = ac.Obs.DeletionDate;
                    obs_tbu.IsSynced = true;
                    obs_tbu.DiagnosisTypeId = ac.Obs.DiagnosisTypeId;
                    obs_tbu.ApprovalStateId = ac.Obs.ApprovalStateId;
                    obs_tbu.ObservationComment = ac.Obs.ObservationComment;
                    //obs_tbu.IsEditable = true;
                    //obs_tbu.ImageCopyright = ac.Obs.ImageCopyright;
                    //obs_tbu.ImageLegend = ac.Obs.ImageLegend;
                    //obs_tbu.UserName = ac.Obs.UserName;
                    //obs_tbu.Identifier = ac.Obs.Identifier;
                    //obs_tbu.StateEgg = ac.Obs.StateEgg;
                    //obs_tbu.StateLarva = ac.Obs.StateLarva;
                    //obs_tbu.StateImago = ac.Obs.StateImago;
                    //obs_tbu.StateNymph = ac.Obs.StateNymph;
                    //obs_tbu.StatePupa = ac.Obs.StatePupa;
                    //obs_tbu.StateDead = ac.Obs.StateDead;

                    //Event has changed
                    if (ac.Ev.EventId != obs_tbu.Event.EventId || ac.Ev.LocalityName != obs_tbu.Event.LocalityName)
                    {
                        Event adviceEvent = new Event
                        {
                            AccuracyId = ac.Ev.AccuracyId,
                            LocalityName = ac.Ev.LocalityName,
                            LatitudeDecimal = ac.Ev.LatitudeDecimal,
                            LongitudeDecimal = ac.Ev.LongitudeDecimal,
                            HabitatDescription = ac.Ev.HabitatDescription,
                            UserId = ac.Obs.UserId,
                            AuthorName = ac.Obs.AuthorName,
                            TkNr = GetTk25Id(ac.Ev.LatitudeDecimal, ac.Ev.LongitudeDecimal).Result
                        };

                        _obsContext.Event.Add(adviceEvent);
                        _obsContext.SaveChanges();

                        obs_tbu.EventId = adviceEvent.EventId;
                        /*
                        obs_tbu.HabitatName = ac.Obs.Habi;
                        obs_tbu.HabitatDescription = ac.Obs.Comment;
                        obs_tbu.Latitude = Double.Parse(ac.Obs.Lat.ToString());
                        obs_tbu.Longitude = Double.Parse(ac.Obs.Lon.ToString());
                        obs_tbu.AccuracyTypeId = ac.Obs.AccuracyTypeId;
                        obs_tbu.LocalityTemplateId = ac.Obs.LocalityTemplateId;
                        */
                    }

                    _obsContext.Observation.Update(obs_tbu);
                    Logger.Info("_obsContext.Observation.Update(obs_tbu)");
                    _obsContext.SaveChanges();
                    Logger.Info("_obsContext.SaveChanges()");


                    if (ac.Images != null)
                    {
                        Logger.Info("ac.Images != null");
                        foreach (AdviceImageJsonItem img_in in ac.Images)
                        {
                            Logger.Info(JsonConvert.SerializeObject(img_in));
                            try
                            {
                                if (obs_tbu.Image != null)
                                {
                                    //check if updated image
                                    Image img_tbu = obs_tbu.Image.FirstOrDefault(i => i.ImagePath.Contains(img_in.ImageName));
                                    if (img_tbu != null)
                                    {
                                        string img_tbu_FullPath = Path.Combine(imgSavePath, img_in.ImageName);
                                        byte[] img_in_imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                                        try
                                        {
                                            byte[] img_tbu_imageArray = System.IO.File.ReadAllBytes(img_tbu_FullPath);
                                            string img_tbu_base64 = Convert.ToBase64String(img_tbu_imageArray);
                                            if (img_tbu_base64 != img_in.ImageBase64)
                                            {

                                                System.IO.File.WriteAllBytes(img_tbu_FullPath, img_in_imageBytes);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //existing foto corrupt
                                            Logger.Error(ex);
                                            System.IO.File.Delete(img_tbu_FullPath);
                                            System.IO.File.WriteAllBytes(img_tbu_FullPath, img_in_imageBytes);
                                        }
                                    }
                                    else
                                    {
                                        SaveNewImage(img_in, obs_tbu);
                                    }
                                }
                                else
                                {
                                    SaveNewImage(img_in, obs_tbu);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn("ac.Images != null");
                    }
                }

            }
            catch (Exception ex)
            {
                //adviceObject.DeletionDate = DateTime.Now;
                Logger.Error(ex);
                var dbg4 = ex;
            }
        }



        /**
        ** create ADVICE (EVENT + OBSERVATION)
        ** comes from mobile
        **/
        [HttpPost("SaveAdvice/Mobile")]
        //POST : /api/Advice/SaveAdvice/Mobile
        public async Task<ActionResult> PostSaveAdviceFromMobile(AdviceJsonItem adviceObject)
        {
            try
            {
                //_userId = User.Claims.First(i => i.Type == "UserId").Value;
                ApplicationUser _user = _userManager.Users.First(u => u.Email == adviceObject.UserName);
                if (_user != null)
                {
                    _userId = _user.Id;
                    var obsId = 0;
                    int? taxonId_edited = null;
                    //if user users horn-old app version --> name lookup
                    if (adviceObject.TaxonId > 0)
                    {
                        if (_infContext.Taxon.FirstOrDefault(tx => tx.TaxonId == adviceObject.TaxonId) == null)
                        {
                            taxonId_edited = _infContext.Taxon.Where(tx => tx.TaxonName == adviceObject.TaxonFullName).Select(tx => tx.TaxonId).FirstOrDefault();
                        }
                    }
                    else
                    {
                        adviceObject.TaxonFullName = "Unbekannte Art";
                    }

                    if (adviceObject.ObservationId == 0)
                    {
                        Event adviceEvent = new Event();
                        adviceEvent.AccuracyId = adviceObject.AccuracyType;
                        adviceEvent.LocalityName = adviceObject.AdviceCity;
                        //adviceEvent. = adviceObject.AdviceCount;
                        //adviceEvent.TkNr = adviceObject.AreaWkt;
                        adviceEvent.LatitudeDecimal = adviceObject.Lat;
                        adviceEvent.LongitudeDecimal = adviceObject.Lon;
                        adviceEvent.HabitatDescription = adviceObject.HabitatDescriptionForEvent;
                        adviceEvent.UserId = _userId;
                        adviceEvent.AuthorName = adviceObject.ReportedByName;
                        adviceEvent.TkNr = await GetTk25Id(adviceEvent.LatitudeDecimal, adviceEvent.LongitudeDecimal);
                        _obsContext.Event.Add(adviceEvent);
                        _obsContext.SaveChanges();

                        Observation adviceObservation = new Observation();
                        adviceObservation.EventId = adviceEvent.EventId;
                        adviceObservation.UserId = _userId;

                        {
                            if (adviceObservation.ApprovalStateId == null)
                                adviceObservation.ApprovalStateId = 1;
                        }
                        adviceObservation.AdviceCount = adviceObject.AdviceCount;
                        adviceObservation.AuthorName = adviceObject.ReportedByName;
                        adviceObservation.UserId = _userId;
                        //adviceObservation.DiagnosisTypeId = adviceObject.Diagn;
                        adviceObservation.FemaleCount = adviceObject.FemaleCount;
                        adviceObservation.HabitatDate = adviceObject.AdviceDate;
                        //adviceObservation.HabitatDateTo = adviceObject.AdviceCount;
                        //adviceObservation.JuvenileCount = adviceObject.Cou;
                        adviceObservation.LocalityTypeId = adviceObject.LocalityTemplateId;
                        adviceObservation.MaleCount = adviceObject.MaleCount;
                        adviceObservation.ObservationComment = adviceObject.Comment;
                        //adviceObservation.SizeGroupId = adviceObject.AdviceCount;

                        if (adviceObservation.TaxonGuid != null)
                        {
                            adviceObservation.TaxonName = _infContext.Taxon.FirstOrDefault(i => i.Identifier.Equals(adviceObservation.TaxonGuid)).TaxonName;
                        }
                        else if (taxonId_edited != null)
                        {
                            adviceObservation.TaxonId = (int)taxonId_edited;
                        }
                        else
                        {
                            adviceObservation.TaxonId = adviceObject.TaxonId;
                        }
                        adviceObservation.TaxonName = adviceObject.TaxonFullName;
                        adviceObservation.LastEditDate = TruncateDateTime(DateTime.Now, TimeSpan.FromSeconds(1));
                        _obsContext.Observation.Add(adviceObservation);
                        _obsContext.SaveChanges();

                        obsId = adviceObservation.ObservationId;

                        foreach (AdviceImageJsonItem img_in in adviceObject.Images)
                        {
                            byte[] imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                            string filename = Path.Combine(imgSavePath, img_in.ImageName);
                            System.IO.File.WriteAllBytes(filename, imageBytes);

                            //Method to save images @bodentierhochvier.de / wordpress
                            //--> imgName 
                            Image img_new = new Image();
                            img_new.ImagePath = "user_uploads/" + img_in.ImageName;
                            //img_new.CmsId = img.CmsId;
                            img_new.ObservationId = obsId;
                            img_new.Description = adviceObservation.ObservationComment;
                            //img_new.CopyrightText = img.CopyrightText;
                            img_new.LicenseId = 1;
                            img_new.UserId = _userId;
                            img_new.Author = adviceEvent.AuthorName;
                            if (taxonId_edited != null)
                            {
                                img_new.TaxonId = (int)taxonId_edited;
                            }
                            else
                            {
                                img_new.TaxonId = adviceObservation.TaxonId;
                            }
                            img_new.TaxonName = adviceObservation.TaxonName;
                            _obsContext.Add(img_new);
                        }

                    }
                    else
                    {
                        var obs_tbu = _obsContext.Observation.FirstOrDefault(o => o.ObservationId == adviceObject.ObservationId);
                        obs_tbu.UserId = _userId;
                        obs_tbu.AdviceCount = adviceObject.AdviceCount;
                        obs_tbu.AuthorName = adviceObject.Name;
                        //adviceObservation.DiagnosisTypeId = adviceObject.Diagn;
                        obs_tbu.FemaleCount = adviceObject.FemaleCount;
                        obs_tbu.HabitatDate = adviceObject.AdviceDate;
                        //adviceObservation.HabitatDateTo = adviceObject.AdviceCount;
                        //adviceObservation.JuvenileCount = adviceObject.Cou;
                        obs_tbu.LocalityTypeId = adviceObject.LocalityTemplateId;
                        obs_tbu.MaleCount = adviceObject.MaleCount;
                        obs_tbu.ObservationComment = adviceObject.Comment;
                        //adviceObservation.SizeGroupId = adviceObject.AdviceCount;

                        if (taxonId_edited != null)
                        {
                            obs_tbu.TaxonId = (int)taxonId_edited;
                        }
                        else
                        {
                            obs_tbu.TaxonId = adviceObject.TaxonId;
                        }
                        obs_tbu.TaxonName = adviceObject.TaxonFullName;
                        obs_tbu.LastEditDate = TruncateDateTime(DateTime.Now, TimeSpan.FromSeconds(1));
                        _obsContext.Observation.Update(obs_tbu);
                        _obsContext.SaveChanges();
                        obsId = obs_tbu.ObservationId;
                    }

                    _obsContext.SaveChanges();

                    return Ok(new { succeeded = true, errors = new string[] { }, ObservationId = obsId });

                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }

        }

        /**
        ** READ ALL OBS per Admin
        **/
        [HttpGet("Observations/Admin")]
        [Authorize(Roles = "Admin")]
        //GET : /api/Advice/Observation
        public ActionResult GetObservationsByAdmin()
        {
            try
            {
                var response = _obsContext.Observation
                    .Include(obs => obs.Image)
                    .Include(obs => obs.Event)
                    .Select(obs => new { obs.Event.LocalityName, obs.AdviceCount, obs.ApprovalStateId, obs.DiagnosisTypeId, obs.EventId, obs.Event.RegionId, obs.FemaleCount, obs.HabitatDate, obs.HabitatDateTo, obs.Image, obs.JuvenileCount, obs.LocalityTypeId, obs.MaleCount, obs.ObservationComment, obs.ObservationId, obs.SizeGroupId, obs.TaxonId, obs.UserId, obs.Event.AuthorName });
                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                return Ok(JsonConvert.SerializeObject(e.Message));
            }
        }

        /**
        ** READ ALL OBS per user
        **/
        [HttpGet("Observations/User")]
        [Authorize]
        //GET : /api/Advice/Observation
        public async Task<ActionResult> GetObservationsByUserAsync()
        {
            try
            {
                _userId = User.Claims.First(i => i.Type == "UserId").Value;

                ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                //var user = await _userManager.FindByIdAsync(_userId);
                //var roles = await _userManager.GetRolesAsync(user);
                var response = _obsContext.Observation
                    .Where(i => i.UserId == _userId && i.DeletionDate == null)
                    .Include(obs => obs.Image)
                    .Include(obs => obs.Event)
                    .Select(obs => new { obs.Event.LocalityName, obs.AdviceCount, obs.ApprovalStateId, obs.DiagnosisTypeId, obs.EventId, obs.Event.RegionId, obs.FemaleCount, obs.HabitatDate, obs.HabitatDateTo, obs.Image, obs.JuvenileCount, obs.LocalityTypeId, obs.MaleCount, obs.ObservationComment, obs.ObservationId, obs.SizeGroupId, obs.TaxonId, obs.UserId, obs.Event.AuthorName });
                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                return Ok(JsonConvert.SerializeObject(e.Message));
            }
        }


        /**
        ** READ SINGLE OBS
        **/
        [HttpGet("Observation/{observationId}")]
        [Authorize]
        //GET : /api/Advice/Observation/23
        public async Task<ActionResult> GetObservationByIdAsync(int observationId)
        {
            try
            {
                if (observationId != 0)
                {
                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                    var user = await _userManager.FindByIdAsync(_userId);
                    var roles = await _userManager.GetRolesAsync(user);

                    Observation obs = new Observation();
                    List<Image> imageList = new List<Image>();

                    try
                    {

                        if (roles.Contains("Admin"))
                        {
                            obs = _obsContext.Observation.Include(o => o.Event).AsNoTracking().Include(i => i.Image).AsNoTracking().FirstOrDefault(i => i.ObservationId == observationId);
                        }
                        else
                        {
                            obs = _obsContext.Observation.Include(o => o.Event).AsNoTracking().Include(i => i.Image).AsNoTracking().FirstOrDefault(i => i.UserId == _userId && i.ObservationId == observationId);
                        }

                        //imageList = _obsContext.Image.Where(i => i.ObservationId == observationId).AsNoTracking().ToList();
                        //obs.Image = imageList;
                        Logger.Info(JsonConvert.SerializeObject(obs));
                        return Ok(JsonConvert.SerializeObject(obs));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                return BadRequest(JsonConvert.SerializeObject(null));

            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        [HttpGet("Observations/CNC")]
        //GET : /api/Observations/CNC
        public ActionResult<CNCObservationItem> GetObservationsCNC()
        {
            try
            {
                string cncKey = _appSettings.CNC_Secret;
                List<Tk25Item> tk25List = GetAllTk25();

                var response = _obsContext.Observation
                    .Where(obs => obs.Image != null && obs.User != null && obs.ApprovalStateId.Equals(5))
                    .Include(obs => obs.Image)
                        .ThenInclude(img => img.License).AsNoTracking()
                    .Include(obs => obs.Event).AsNoTracking()
                    .Include(obs => obs.User).AsNoTracking()
                    .Where(obs => obs.User.DataRestrictionId > 0)
                    .Select(obs => new CNCObservationItem()
                    {
                        Image_URL = !String.IsNullOrEmpty(obs.Image.FirstOrDefault().ImagePath) ? $@"{imgPublicUrl}{obs.Image.FirstOrDefault().ImagePath}" : null,
                        Attribution = obs.User.DataRestrictionId.Equals(1) ? "Obscured Lat, Long to TK25 Center Lat, Long. Obscured Sighting_date to year to January, 1st of the sighting's year" : null,
                        License_type = obs.Image != null ? obs.Image.FirstOrDefault().License != null ? obs.Image.FirstOrDefault().License.LicenseName : "© All rights reserved" : "© All rights reserved",
                        Latitude = obs.User.DataRestrictionId.Equals(1) ? tk25List.FirstOrDefault(t => t.Tk25Nr == obs.Event.TkNr).Wgs84CenterLat : (decimal)obs.Event.LatitudeDecimal,
                        Longitude = obs.User.DataRestrictionId.Equals(1) ? tk25List.FirstOrDefault(t => t.Tk25Nr == obs.Event.TkNr).Wgs84CenterLong : (decimal)obs.Event.LongitudeDecimal,
                        Is_Obscured = obs.User != null ? obs.User.DataRestrictionId.Equals(1) : false,
                        Sighting_address = obs.Event != null ? obs.Event.LocalityName : null,
                        Sighting_date = (DateTime)(obs.User.DataRestrictionId.Equals(1) ? DateTime.Parse($@"01-01-{obs.HabitatDate.Value.Year.ToString()}") : obs.HabitatDate),
                        Field_notes = obs.Event != null ? $@"{obs.Event.HabitatDescription}; {obs.ObservationComment}" : null,
                        Timezone = TimeZoneInfo.FindSystemTimeZoneById("CET").StandardName,
                        Sighting_ID = obs.ObservationId.ToString(),
                        User_id = EncryptString(obs.UserId, cncKey),
                        User_name = String.IsNullOrEmpty(obs.AuthorName) ? $@"{obs.User.LastName}, {obs.User.FirstName}" : obs.AuthorName,
                        Species = obs.TaxonName
                    }).ToList();

                Logger.Error(JsonConvert.SerializeObject(response));

                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                //return Ok(JsonConvert.SerializeObject(e.Message));
                Trace.WriteLine(ex.Message);
                Logger.Error(ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /**
        ** CREATE EVENT
        **/
        [HttpPost("SaveEvent")]
        [Authorize]
        public async Task<ActionResult> CreateNewEvent(Event event_inc)
        {
            try
            {
                if (event_inc != null)
                {
                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    Event event_tba = new Event();
                    event_tba.HabitatDescription = event_inc.HabitatDescription;
                    event_tba.HabitatTypeId = event_inc.HabitatTypeId;
                    event_tba.AccuracyId = event_inc.AccuracyId;
                    event_tba.ApprovalStateId = event_inc.ApprovalStateId;
                    event_tba.LatitudeDecimal = event_inc.LatitudeDecimal;
                    event_tba.LongitudeDecimal = event_inc.LongitudeDecimal;
                    event_tba.LocalityName = event_inc.LocalityName;
                    event_tba.RegionId = event_inc.RegionId;
                    event_tba.CountryId = event_inc.CountryId;

                    event_tba.TkNr = await GetTk25Id(event_tba.LatitudeDecimal, event_tba.LongitudeDecimal);

                    _obsContext.Add(event_tba);
                    _obsContext.SaveChanges();

                    return Ok(new { succeeded = true, errors = new string[] { } });
                }
                return BadRequest(JsonConvert.SerializeObject(null));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        /**
        * UPDATE EVENT
        **/
        [HttpPost("UpdateEvent/{eventId}")]
        [Authorize]
        public async Task<ActionResult> UpdateEventById(Event event_inc)
        {
            try
            {
                if (event_inc != null)
                {
                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    var eId = event_inc.EventId;
                    Event event_tbu = _obsContext.Event.FirstOrDefault(o => o.EventId == eId);
                    event_tbu.HabitatDescription = event_inc.HabitatDescription;
                    event_tbu.HabitatTypeId = event_inc.HabitatTypeId;
                    event_tbu.AccuracyId = event_inc.AccuracyId;
                    event_tbu.ApprovalStateId = event_inc.ApprovalStateId;
                    event_tbu.LatitudeDecimal = event_inc.LatitudeDecimal;
                    event_tbu.LongitudeDecimal = event_inc.LongitudeDecimal;
                    event_tbu.LocalityName = event_inc.LocalityName;
                    event_tbu.RegionId = event_inc.RegionId;

                    event_tbu.TkNr = await GetTk25Id(event_tbu.LatitudeDecimal, event_tbu.LongitudeDecimal);

                    _obsContext.Update(event_tbu);
                    try
                    {
                        int eventId = event_tbu.EventId;
                        List<Observation> obsList = _obsContext.Observation.Where(obs => obs.EventId.Equals(eventId)).ToList();
                        foreach (Observation obs in obsList)
                        {
                            obs.LastEditDate = TruncateDateTime(DateTime.Now, TimeSpan.FromSeconds(1));
                        }
                        _obsContext.UpdateRange(obsList);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    _obsContext.SaveChanges();

                    return Ok(new { succeeded = true, errors = new string[] { } });
                }
                return BadRequest(JsonConvert.SerializeObject(null));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        /**
        * UPDATE OBSERVATION
        **/
        [HttpPost("UpdateObservation/{observationId}")]
        //POST: /api/Advice/Observation/23
        [Authorize]
        public async Task<ActionResult> UpdateObservationById(ObservationImage observation)
        {
            try
            {
                if (observation != null)
                {
                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                    var user = await _userManager.FindByIdAsync(_userId);
                    var roles = await _userManager.GetRolesAsync(user);

                    var obsId = observation.ObservationId;
                    Observation obs_tbu = new Observation();
                    if (roles.Contains("Admin"))
                    {
                        obs_tbu = _obsContext.Observation.FirstOrDefault(o => o.ObservationId == obsId);
                    }
                    else
                    {
                        obs_tbu = _obsContext.Observation.FirstOrDefault(o => o.ObservationId == obsId && o.UserId == _userId);
                    }
                    obs_tbu.ApprovalStateId = observation.ApprovalStateId;
                    obs_tbu.AdviceCount = observation.AdviceCount;
                    obs_tbu.AuthorName = observation.AuthorName;
                    obs_tbu.DiagnosisTypeId = observation.DiagnosisTypeId;
                    obs_tbu.FemaleCount = observation.FemaleCount;
                    obs_tbu.MaleCount = observation.MaleCount;
                    obs_tbu.JuvenileCount = observation.JuvenileCount;
                    obs_tbu.LocalityTypeId = observation.LocalityTypeId;
                    obs_tbu.ObservationComment = observation.ObservationComment;
                    obs_tbu.SizeGroupId = observation.SizeGroupId;
                    obs_tbu.TaxonId = observation.TaxonId;
                    obs_tbu.TaxonName = observation.TaxonName;
                    obs_tbu.EditorComment = observation.EditorComment;
                    obs_tbu.HabitatDate = observation.HabitatDate;
                    obs_tbu.HabitatDateTo = observation.HabitatDateTo;
                    obs_tbu.LastEditDate = TruncateDateTime(DateTime.Now, TimeSpan.FromSeconds(1));
                    _obsContext.Update(obs_tbu);
                    _obsContext.SaveChanges();

                    List<ImageIncoming> imgList_incoming = observation.Images;

                    UpdateObservationImages(imgList_incoming, observation, observation.ApprovalStateId);

                }

                _obsContext.SaveChanges();
                return Ok(new { succeeded = true, errors = new string[] { } });
            }

            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        /**Helper comparing incoming images to those saved in DB **/
        private void UpdateObservationImages(List<ImageIncoming> imgList_incoming, ObservationImage observation, int approvalStateId)
        {

            List<Image> imgList_existing = _obsContext.Image.Where(img => img.ObservationId == observation.ObservationId).ToList();
            foreach (ImageIncoming img in imgList_incoming)
            {
                if (img.ToBeDeleted == true)
                {
                    Image img_tbd = _obsContext.Image.FirstOrDefault(i => i.ImageId == img.ImageId);
                    img_tbd.IsApproved = false;
                    img_tbd.CmsId = null;
                    img_tbd.ObservationId = null;
                    img_tbd.Description += $@"; entfernt von ObservationId = {observation.ObservationId} nach Bearbeitung am {DateTime.Now.ToString("yyyy-MM-dd")} durch User = {_userId}";
                    _obsContext.Image.Update(img_tbd);
                    _obsContext.SaveChanges();
                }
                //Update Image
                else if (img.ImageId != 0)
                {
                    Image img_tbu = _obsContext.Image.FirstOrDefault(i => i.ImageId == img.ImageId);
                    img_tbu.Description = observation.ObservationComment;
                    img_tbu.Author = observation.AuthorName;
                    img_tbu.CopyrightText = img.CopyrightText;
                    img_tbu.TaxonName = observation.TaxonName;
                    img_tbu.TaxonId = observation.TaxonId;
                    img.IsApproved = positiveApprovalStates.Contains(approvalStateId) ? true : false;
                    _obsContext.Image.Update(img_tbu);
                    _obsContext.SaveChanges();
                }
                //Add New Image
                else
                {
                    Image new_img = new Image();
                    new_img.ImagePath = "user_uploads/" + img.ImagePath;
                    new_img.CmsId = img.CmsId;
                    new_img.ObservationId = observation.ObservationId;
                    new_img.LicenseId = 1;
                    new_img.Description = observation.ObservationComment;
                    new_img.UserId = _userId;
                    new_img.Author = observation.AuthorName;
                    new_img.TaxonId = observation.TaxonId;
                    new_img.TaxonName = observation.TaxonName;
                    _obsContext.Add(new_img);
                    _obsContext.SaveChanges();
                }

                _obsContext.SaveChanges();
            }

        }

        [HttpGet("ApprovalState")]
        [Authorize]
        //GET : /api/Advice/ApprovalState
        public ActionResult GetApprovalState()
        {
            _userId = User.Claims.First(i => i.Type == "UserId").Value;
            var response = _obsContext.ApprovalState.ToList();
            return Ok(JsonConvert.SerializeObject(response));
        }


        [HttpPost("Observation/Delete")]
        public async Task<ActionResult> DeleteObservationAsync(Observation observation)
        {
            try
            {
                if (observation != null)
                {

                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                    var roles = await _userManager.GetRolesAsync(_user);
                    Observation obs = new Observation();

                    if (!roles.Contains("Admin"))
                    {
                        obs = _obsContext.Observation.First(o => o.ObservationId == observation.ObservationId && o.UserId == _userId);
                    }
                    else
                    {
                        obs = _obsContext.Observation.First(o => o.ObservationId == observation.ObservationId);
                    }
                    if (obs != null)
                    {
                        List<Image> imgs = _obsContext.Image.Where(i => i.ObservationId == observation.ObservationId).ToList();
                        foreach (Image img in imgs)
                        {
                            _obsContext.Remove(img);
                        }
                        if (!obs.IsSynced)
                        {
                            _obsContext.Remove(obs);
                        }
                        else
                        {
                            obs.DeletionDate = DateTime.Now;
                        }
                        _obsContext.SaveChanges();
                    }
                    else
                    {
                        return BadRequest("No Observation Found");
                    }
                }
                return Ok(JsonConvert.SerializeObject("success"));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }

        [HttpPost("Event/Delete")]
        public async Task<ActionResult> DeleteEventAsync(Event event_inc)
        {
            try
            {
                if (event_inc != null)
                {

                    _userId = User.Claims.First(i => i.Type == "UserId").Value;
                    ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                    var roles = await _userManager.GetRolesAsync(_user);
                    Event event_tbd = new Event();
                    Observation obs_tbd = new Observation();
                    List<Image> imgs_tbd = new List<Image>();
                    if (!roles.Contains("Admin"))
                    {
                        obs_tbd = _obsContext.Observation.First(o => o.EventId == event_inc.EventId && o.UserId == _userId);
                        imgs_tbd = _obsContext.Image.Where(i => i.ObservationId == obs_tbd.ObservationId && i.UserId == _userId).ToList();
                        event_tbd = _obsContext.Event.First(o => o.EventId == event_inc.EventId && o.UserId == _userId);
                    }
                    else
                    {
                        obs_tbd = _obsContext.Observation.First(o => o.EventId == event_inc.EventId);
                        imgs_tbd = _obsContext.Image.Where(i => i.ObservationId == obs_tbd.ObservationId).ToList();
                        event_tbd = _obsContext.Event.First(o => o.EventId == event_inc.EventId);
                    }
                    if (event_tbd != null)
                    {
                        foreach (Image img in imgs_tbd)
                        {
                            _obsContext.Remove(img);
                        }
                        _obsContext.Remove(obs_tbd);
                        _obsContext.Remove(event_tbd);
                        _obsContext.SaveChanges();
                    }
                    else
                    {
                        return BadRequest("No Event Found");
                    }
                }
                return Ok(JsonConvert.SerializeObject("success"));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }
        }


        /**only used by inline-editing from table**/
        [HttpPost("Observation/Update")]
        [Authorize]
        //POST : /api/Advice/Observation/Update
        public async Task<ActionResult> UpdateObservation(ObservationImage requestData)
        {
            try
            {
                _userId = User.Claims.First(i => i.Type == "UserId").Value;
                ApplicationUser _user = _userManager.Users.First(u => u.Id == _userId);
                var roles = await _userManager.GetRolesAsync(_user);
                var obsId = requestData.ObservationId;
                if (obsId > 0)
                {
                    Observation oldObs = new Observation();
                    if (!roles.Contains("Admin"))
                    {
                        oldObs = _obsContext.Observation.FirstOrDefault(obs => obs.ObservationId == obsId && obs.UserId == _userId);
                    }
                    else
                    {
                        oldObs = _obsContext.Observation.FirstOrDefault(obs => obs.ObservationId == obsId);
                    }
                    oldObs.AdviceCount = requestData.AdviceCount;
                    oldObs.MaleCount = requestData.MaleCount;
                    oldObs.FemaleCount = requestData.FemaleCount;
                    oldObs.JuvenileCount = requestData.JuvenileCount;
                    oldObs.LocalityTypeId = requestData.LocalityTypeId;
                    oldObs.ObservationComment = requestData.ObservationComment;
                    oldObs.DiagnosisTypeId = requestData.DiagnosisTypeId;
                    oldObs.LastEditDate = DateTime.UtcNow;
                    //disabled due to kendo bug with date - 1 day
                    //oldObs.HabitatDate = requestData.HabitatDate;
                    if (requestData.TaxonId != null && requestData.TaxonId > 0)
                    {
                        oldObs.TaxonId = requestData.TaxonId;
                        Taxon tempTax = _infContext.Taxon.FirstOrDefault(i => i.TaxonId == requestData.TaxonId);
                        if (tempTax != null)
                        {
                            oldObs.TaxonName = tempTax.TaxonName;
                            oldObs.TaxonGuid = tempTax.Identifier;
                        }
                    }
                    oldObs.SizeGroupId = requestData.SizeGroupId;
                    oldObs.ApprovalStateId = requestData.ApprovalStateId;

                    if (requestData.AuthorName != null)
                    {
                        Event oldEvent = _obsContext.Event.FirstOrDefault(ev => ev.EventId == oldObs.EventId);
                        if (oldEvent != null)
                        {
                            oldEvent.AuthorName = requestData.AuthorName;
                            _obsContext.Update(oldEvent);
                        }
                    }
                    _obsContext.Update(oldObs);

                    //also update images' taxonId
                    List<Image> images = _obsContext.Image.Where(img => img.ObservationId == obsId).ToList();
                    images.ForEach(imgItem =>
                        imgItem.TaxonId = requestData.TaxonId
                    );
                    _obsContext.UpdateRange(images);
                    _obsContext.SaveChanges();
                    return Ok(JsonConvert.SerializeObject(true));
                };
            }
            catch (System.Exception e)
            {
                return Ok(JsonConvert.SerializeObject(e.InnerException));
            }

            return null;
        }


        /**
        ** READ ALL EVENTS
        **/
        [HttpGet("Events/Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetEventAsync()
        {
            try
            {
                return Ok(JsonConvert.SerializeObject(_obsContext.Event.ToList()));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }

        }

        /**
        ** READ ALL EVENTS
        **/
        [HttpGet("Events/User")]
        [Authorize]
        //GET : /api/Advice/Event
        public async Task<ActionResult> GetEventByUserAsync()
        {
            try
            {
                _userId = User.Claims.First(i => i.Type == "UserId").Value;
                return Ok(JsonConvert.SerializeObject(_obsContext.Event.Where(i => i.UserId == _userId)));
            }
            catch (Exception e)
            {
                return BadRequest(JsonConvert.SerializeObject(e.Message));
            }

        }


        /**
        ** READ SINGLE EVENT
        **/
        [HttpGet("Event/{eventId}")]
        [Authorize]
        //GET : /api/Advice/Event/123
        public ActionResult GetEventById(int? eventId)
        {
            if (eventId != null)
            {
                _userId = User.Claims.First(i => i.Type == "UserId").Value;
                return Ok(JsonConvert.SerializeObject(_obsContext.Event.Where(i => i.EventId == eventId)));
            }
            return Ok(JsonConvert.SerializeObject(null));
        }

        /**HELPER FOR tk25 based on coords**/
        private async Task<int?> GetTk25Id(decimal? lon, decimal? lat)
        {
            try
            {
                int result = 0;
                using (_locContext)
                {
                    var connection = _locContext.Database.GetDbConnection();
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    command.Parameters.Add(new NpgsqlParameter("Point", System.FormattableString.Invariant($"POINT({lon} {lat})")));
                    command.CommandText = @"Select id,geom, ST_Distance(ST_Transform(ST_GeomFromText(@Point,4123),3857),geom) as Distance, dtkname as Name from ""Map"".osm_new_tk25 ORDER BY Distance ASC LIMIT 1";
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        result = (int)reader.GetValue(0);
                    }
                }
                return result;
            }
            catch (System.Exception e)
            {
                var test = e.Message;
                return null;
            }
        }



        public class ObservationImage : Observation
        {
            public List<ImageIncoming> Images;
        }

        public class ImageIncoming : Data.Observation.Image
        {
            public bool ToBeDeleted;
        }

        public class AdviceObject : Data.Observation.Event
        {
            public List<ObservationImage> ObservationList { get; set; }
        }


        [Serializable]
        public class AdviceJsonItem
        {
            public int AdviceId { get; set; }
            public string UserName { get; set; }
            public string ReportedByName { get; set; }

            public int TaxonId { get; set; }

            public string TaxonFullName { get; set; }

            public DateTime AdviceDate { get; set; }
            public DateTime? DeletionDate { get; set; }

            public int? AdviceCount { get; set; }

            public string AdviceCity { get; set; }

            public int? MaleCount { get; set; }

            public int? FemaleCount { get; set; }

            public bool StateEgg { get; set; }

            public bool StateLarva { get; set; }

            public bool StateImago { get; set; }

            public bool StateNymph { get; set; }

            public bool StatePupa { get; set; }

            public bool StateDead { get; set; }

            public string Comment { get; set; }
            public string HabitatDescriptionForEvent { get; set; }

            public string Name { get; set; }

            public string ImageCopyright { get; set; }

            public string ImageLegend { get; set; }

            public int UploadCode { get; set; }

            public decimal? Lat { get; set; }

            public decimal? Lon { get; set; }

            public string AreaWkt { get; set; }

            public int? Zoom { get; set; }

            public int? AccuracyType { get; set; }
            public int? DiagnosisTypeId { get; set; }
            public string DeviceId { get; set; }
            public string DeviceHash { get; set; }
            public int? LocalityTemplateId { get; set; }
            public int ObservationId { get; set; }
            public AdviceImageJsonItem[] Images { get; set; }
            [DataMember]
            public Guid Identifier { get; set; }
            public string Md5Checksum { get; set; }
            public DateTime LastEditDate { get; set; }
            public AdviceJsonItem()
            {

            }
        }

        [DataContract]
        [Serializable]
        public class AdviceJsonItemSync : ISerializable
        {
            [DataMember]
            public int? GlobalAdviceId { get; set; }
            [DataMember]
            public Guid Identifier { get; set; }
            [DataMember]
            public string UserName { get; set; }
            [DataMember]
            public int TaxonId { get; set; }
            [DataMember]
            public string TaxonFullName { get; set; }
            [DataMember]
            public Guid? TaxonGuid { get; set; }
            [DataMember]
            public DateTime? AdviceDate { get; set; }
            [DataMember]
            public int? AdviceCount { get; set; }
            [DataMember]
            public string AdviceCity { get; set; }
            [DataMember]
            public int? MaleCount { get; set; }
            [DataMember]
            public int? FemaleCount { get; set; }
            [DataMember]
            public bool StateEgg { get; set; }
            [DataMember]
            public bool StateLarva { get; set; }
            [DataMember]
            public bool StateImago { get; set; }
            [DataMember]
            public bool StateNymph { get; set; }
            [DataMember]
            public bool StatePupa { get; set; }
            [DataMember]
            public bool StateDead { get; set; }
            [DataMember]
            public string Comment { get; set; }
            [DataMember]
            public string HabitatDescriptionForEvent { get; set; }
            [DataMember]
            public string ReportedByName { get; set; }
            [DataMember]
            public string ImageCopyright { get; set; }
            [DataMember]
            public string ImageLegend { get; set; }
            [DataMember]
            public string Lat { get; set; }
            [DataMember]
            public string Lon { get; set; }
            [DataMember]
            public int? AccuracyTypeId { get; set; }
            [DataMember]
            public int? LocalityTemplateId { get; set; }
            [DataMember]
            public string Md5Checksum { get; set; }
            [DataMember]
            public DateTime? LastEditDate { get; set; }
            [DataMember]
            public DateTime? DeletionDate { get; set; }
            [DataMember]
            public bool IsSynced { get; set; }
            [DataMember]
            public int ApprovalStateId { get; set; }
            [DataMember]
            public int? DiagnosisTypeId { get; set; }
            [DataMember]
            public List<AdviceImageJsonItem> Images { get; set; }

            public AdviceJsonItemSync() { }
            protected AdviceJsonItemSync(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new System.ArgumentNullException("info");
                }
                GlobalAdviceId = (int?)info.GetValue("GlobalAdviceId", typeof(int?));
                Identifier = (Guid)info.GetValue("Identifier", typeof(Guid));
                UserName = (string)info.GetValue("UserName", typeof(string));
                LastEditDate = (DateTime)info.GetValue("LastEditDate", typeof(DateTime));
                DeletionDate = (DateTime?)info.GetValue("DeletionDate", typeof(DateTime?));
                TaxonId = (int)info.GetValue("TaxonId", typeof(int));
                TaxonFullName = (string)info.GetValue("TaxonFullName", typeof(string));
                TaxonGuid = (Guid?)info.GetValue("TaxonGuid", typeof(Guid?));
                AdviceDate = (DateTime)info.GetValue("AdviceDate", typeof(DateTime));
                AdviceCount = (int)info.GetValue("AdviceCount", typeof(int));
                AdviceCity = (string)info.GetValue("AdviceCity", typeof(string));
                MaleCount = (int?)info.GetValue("MaleCount", typeof(int?));
                FemaleCount = (int?)info.GetValue("FemaleCount", typeof(int?));
                StateEgg = (bool)info.GetValue("StateEgg", typeof(bool));
                StateLarva = (bool)info.GetValue("StateLarva", typeof(bool));
                StateImago = (bool)info.GetValue("StateImago", typeof(bool));
                StateNymph = (bool)info.GetValue("StateNymph", typeof(bool));
                StatePupa = (bool)info.GetValue("StatePupa", typeof(bool));
                StateDead = (bool)info.GetValue("StateDead", typeof(bool));
                Comment = (string)info.GetValue("Comment", typeof(string));
                HabitatDescriptionForEvent = (string)info.GetValue("HabitatDescriptionForEvent", typeof(string));
                ReportedByName = (string)info.GetValue("ReportedByName", typeof(string));
                Lat = (string)info.GetValue("Lat", typeof(string));
                Lon = (string)info.GetValue("Lon", typeof(string));
                AccuracyTypeId = (int?)info.GetValue("AccuracyTypeId", typeof(int?));
                ApprovalStateId = (int)info.GetValue("ApprovalStateId", typeof(int));
                DiagnosisTypeId = (int?)info.GetValue("DiagnosisTypeId", typeof(int?));
                LocalityTemplateId = (int?)info.GetValue("LocalityTemplateId", typeof(int?));
                Md5Checksum = (string)info.GetValue("Md5Checksum", typeof(string));
                Images = (List<AdviceImageJsonItem>)info.GetValue("Images", typeof(List<AdviceImageJsonItem>));
                IsSynced = (bool)info.GetValue("IsSynced", typeof(bool));
            }

            [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
            protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("GlobalAdviceId", GlobalAdviceId);
                info.AddValue("Identifier", Identifier);
                info.AddValue("UserName", UserName);
                info.AddValue("LastEditDate", LastEditDate);
                info.AddValue("DeletionDate", DeletionDate);
                info.AddValue("TaxonId", TaxonId);
                info.AddValue("TaxonFullName", TaxonFullName);
                info.AddValue("TaxonGuid", TaxonGuid);
                info.AddValue("AdviceDate", AdviceDate);
                info.AddValue("AdviceCount", AdviceCount);
                info.AddValue("AdviceCity", AdviceCity);
                info.AddValue("MaleCount", MaleCount);
                info.AddValue("FemaleCount", FemaleCount);
                info.AddValue("StateEgg", StateEgg);
                info.AddValue("StateLarva", StateLarva);
                info.AddValue("StateImago", StateImago);
                info.AddValue("StateNymph", StateNymph);
                info.AddValue("StatePupa", StatePupa);
                info.AddValue("StateDead", StateDead);
                info.AddValue("Comment", Comment);
                info.AddValue("HabitatDescriptionForEvent", HabitatDescriptionForEvent);
                info.AddValue("ReportedByName", ReportedByName);
                info.AddValue("ImageCopyright", ImageCopyright);
                info.AddValue("DiagnosisTypeId", DiagnosisTypeId);
                info.AddValue("ImageLegend", ImageLegend);
                info.AddValue("Lat", Lat);
                info.AddValue("Lon", Lon);
                info.AddValue("AccuracyTypeId", AccuracyTypeId);
                info.AddValue("ApprovalStateId", ApprovalStateId);
                info.AddValue("LocalityTemplateId", LocalityTemplateId);
                info.AddValue("Md5Checksum", Md5Checksum);
                info.AddValue("IsSynced", IsSynced);
                info.AddValue("Images", Images);
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                    throw new ArgumentNullException("info");

                GetObjectData(info, context);
            }


            public void GenerateItemHash()
            {
                string hash = ComputeHash(ObjectToByteArray(this));
                Md5Checksum = hash;
            }

            private static readonly Object locker = new Object();

            private static byte[] ObjectToByteArray(Object objectToSerialize)
            {
                MemoryStream fs = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    //Here's the core functionality! One Line!
                    //To be thread-safe we lock the object
                    lock (locker)
                    {
                        formatter.Serialize(fs, objectToSerialize);
                    }
                    return fs.ToArray();
                }
                catch (SerializationException se)
                {
                    Console.WriteLine("Error occurred during serialization. Message: " +
                    se.Message);
                    return null;
                }
                finally
                {
                    fs.Close();
                }
            }

            private static string ComputeHash(byte[] objectAsBytes)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                try
                {
                    byte[] result = md5.ComputeHash(objectAsBytes);

                    // Build the final string by converting each byte
                    // into hex and appending it to a StringBuilder
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < result.Length; i++)
                    {
                        sb.Append(result[i].ToString("X2"));
                    }

                    // And return it
                    return sb.ToString();
                }
                catch (ArgumentNullException ane)
                {
                    //If something occurred during serialization, 
                    //this method is called with a null argument. 
                    Console.WriteLine("Hash has not been generated.");
                    return null;
                }
            }
        }

        public void GenerateItemHashV2(AdviceJsonItemSync aji)
        {
            string itemJson = JsonConvert.SerializeObject(aji);
            aji.Md5Checksum = GetHashString(itemJson);
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public class SyncRequest
        {
            public string DeviceId { get; set; }
            public string DeviceHash { get; set; }
            public string UserName { get; set; }
            public List<AdviceJsonItemSync> AdviceList { get; set; }
        }

        public AdviceJsonItemSync ConvertToJsonForSync(Observation rm, string userName)
        {
            try
            {
                Taxon tempTaxon;
                if (rm.TaxonId > 0)
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.TaxonId == (int)(rm.TaxonId));
                    var taxonName = (tempTaxon != null) ? tempTaxon.TaxonName : "";
                }
                else if (rm.TaxonGuid != null && !Guid.Empty.Equals(rm.TaxonGuid))
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.Identifier == rm.TaxonGuid);
                }
                else if (!String.IsNullOrEmpty(rm.TaxonName))
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.TaxonName == rm.TaxonName);
                }
                else
                {
                    tempTaxon = new Taxon { TaxonId = -1, TaxonName = "Unbekannte Art", Identifier = Guid.Empty };
                }

                AdviceJsonItemSync adviceJsonItem = new AdviceJsonItemSync
                {
                    GlobalAdviceId = rm.ObservationId,
                    Identifier = rm.Identifier,
                    TaxonId = tempTaxon.TaxonId,
                    TaxonFullName = tempTaxon.TaxonName,
                    TaxonGuid = tempTaxon.Identifier,
                    AdviceDate = rm.HabitatDate,
                    AdviceCount = rm.AdviceCount,
                    AdviceCity = rm.Event.LocalityName,
                    MaleCount = rm.MaleCount,
                    FemaleCount = rm.FemaleCount,
                    StateEgg = false,
                    StateLarva = false,
                    StateImago = true,
                    StateNymph = false,
                    StatePupa = false,
                    StateDead = false,
                    Comment = rm.ObservationComment,
                    HabitatDescriptionForEvent = rm.Event.HabitatDescription,
                    ReportedByName = rm.AuthorName,
                    LastEditDate = TruncateDateTime(rm.LastEditDate, TimeSpan.FromSeconds(1)),
                    DeletionDate = (DateTime?)(rm.DeletionDate.HasValue ? rm.DeletionDate : null),
                    Lat = rm.Event.LatitudeDecimal != null ? Math.Round((decimal)rm.Event.LatitudeDecimal, 6, MidpointRounding.AwayFromZero).ToString("N6") : null,
                    Lon = rm.Event.LongitudeDecimal != null ? Math.Round((decimal)rm.Event.LongitudeDecimal, 6, MidpointRounding.AwayFromZero).ToString("N6") : null,
                    AccuracyTypeId = rm.Event.AccuracyId,
                    ApprovalStateId = rm.ApprovalStateId,
                    LocalityTemplateId = rm.Event?.EventId,
                    Images = new List<AdviceImageJsonItem>(),
                    IsSynced = rm.IsSynced,
                    DiagnosisTypeId = rm.DiagnosisTypeId,
                    UserName = userName
                };

                foreach (Image img in rm.Image)
                {
                    try
                    {
                        string imgFullPath = $@"{imgSavePath_general}{img.ImagePath}";
                        byte[] imageArray = System.IO.File.ReadAllBytes(imgFullPath);
                        AdviceImageJsonItem newImg = new AdviceImageJsonItem { ImageName = img.ImagePath.Replace("user_uploads/", ""), ImageBase64 = Convert.ToBase64String(imageArray) };
                        adviceJsonItem.Images.Add(newImg);
                    }
                    catch (Exception ex)
                    {
                        var dbg = ex;
                        Logger.Error($@"Error binding image for {rm.Identifier}");
                    }
                }

                GenerateItemHashV2(adviceJsonItem);
                //adviceJsonItem.GenerateItemHash();

                return adviceJsonItem;

            }
            catch (Exception ex)
            {
                var dbg = ex;
                Logger.Info(JsonConvert.SerializeObject(rm));
                Logger.Error(ex.Message);
                return null;
            }
        }

        public static DateTime? TruncateDateTime(DateTime? dateTime, TimeSpan timeSpan)
        {
            if (dateTime != null)
            {
                if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
                if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
                return ((DateTime)dateTime).AddTicks(-(((DateTime)dateTime).Ticks % timeSpan.Ticks));
            }
            return null;
        }

        public AdviceComplex ConvertToAdviceComplexFromSync(AdviceJsonItemSync ajis, string reportedByUserId)
        {
            try
            {
                Taxon tempTaxon = new Taxon();

                if (ajis.TaxonGuid != null && !Guid.Empty.Equals(ajis.TaxonGuid))
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.Identifier == ajis.TaxonGuid);
                }
                else if (ajis.TaxonId != null)
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.TaxonId == (int)(ajis.TaxonId));
                }

                if (tempTaxon == null)
                {
                    tempTaxon = _infContext.Taxon.FirstOrDefault(i => i.TaxonName == ajis.TaxonFullName);
                    //Mapping failed; Taxonid needs to be set manually at server
                    if (tempTaxon == null)
                    {
                        tempTaxon = new Taxon { TaxonName = "Unbekannte Art", TaxonId = -1, Identifier = Guid.Empty };
                    }
                }

                var taxonName = (tempTaxon != null) ? tempTaxon.TaxonName : "";

                AdviceComplex ac = new AdviceComplex();

                Logger.Info("HUSSA!!!");
                Logger.Info(ajis.Lon);
                Logger.Info(Decimal.Parse(ajis.Lon, CultureInfo.InvariantCulture));

                Event ev = new Event
                {
                    LatitudeDecimal = Decimal.Parse(ajis.Lat, CultureInfo.InvariantCulture),
                    LongitudeDecimal = Decimal.Parse(ajis.Lon, CultureInfo.InvariantCulture),
                    LocalityName = ajis.AdviceCity,
                    AccuracyId = ajis.AccuracyTypeId,
                    UserId = reportedByUserId,
                    AuthorName = ajis.ReportedByName,
                    HabitatDescription = ajis.HabitatDescriptionForEvent,
                    TkNr = GetTk25Id(Decimal.Parse(ajis.Lat, CultureInfo.InvariantCulture), Decimal.Parse(ajis.Lon, CultureInfo.InvariantCulture)).Result
                };

                ac.Ev = ev;

                Observation obs = new Observation
                {
                    Identifier = ajis.Identifier,
                    TaxonId = tempTaxon.TaxonId,
                    TaxonName = tempTaxon.TaxonName,
                    TaxonGuid = tempTaxon.Identifier,
                    HabitatDate = ajis.AdviceDate,
                    AdviceCount = ajis.AdviceCount,
                    MaleCount = ajis.MaleCount,
                    FemaleCount = ajis.FemaleCount,
                    ObservationComment = ajis.Comment,
                    AuthorName = ajis.ReportedByName,
                    DiagnosisTypeId = ajis.DiagnosisTypeId > 0 ? ajis.DiagnosisTypeId : 1,
                    LastEditDate = ajis.LastEditDate,
                    DeletionDate = ajis.DeletionDate,
                    IsSynced = ajis.IsSynced,
                    UserId = reportedByUserId,
                    ApprovalStateId = ajis.ApprovalStateId
                };


                if (ajis.GlobalAdviceId != null)
                {
                    obs.ObservationId = (int)ajis.GlobalAdviceId;
                }

                ac.Obs = obs;

                if (ajis.Images != null)
                {
                    ac.Images = ajis.Images;
                }

                return ac;
            }
            catch (Exception ex)
            {
                var dbg = ex;
                Logger.Error(ex.Message);
                return null;
            }
        }

        public class CNCObservationItem
        {
            public string Image_URL { get; set; }
            public string Attribution { get; set; }
            public string License_type { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public bool Is_Obscured { get; set; }
            public string Sighting_address { get; set; }
            public DateTime Sighting_date { get; set; }
            public string Timezone { get; set; }
            public string Sighting_ID { get; set; }
            public string User_id { get; set; }
            public string User_name { get; set; }
            public string Field_notes { get; set; }
            public string Species { get; set; }
        }

        [Serializable]
        public class AdviceImageJsonItem : ISerializable
        {
            public string ImageName { get; set; }

            public string ImageBase64 { get; set; }
            public AdviceImageJsonItem() { }

            protected AdviceImageJsonItem(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new System.ArgumentNullException("info");
                }
                ImageBase64 = (string)info.GetValue("ImageBase64", typeof(string));
                ImageName = (string)info.GetValue("ImageName", typeof(string));
            }

            [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
            protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ImageBase64", ImageBase64);
                info.AddValue("ImageName", ImageName);
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                    throw new ArgumentNullException("info");

                GetObjectData(info, context);
            }
        }

        public class AdviceComplex
        {
            public Observation Obs { get; set; }
            public Event Ev { get; set; }
            public List<AdviceImageJsonItem> Images { get; set; }
        }

        public class AuthorizationJson
        {
            public string DeviceId { get; set; }
            public string DeviceHash { get; set; }
        }

        public static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string DecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public List<Tk25Item> GetAllTk25()
        {
            try
            {
                using (_locContext)
                {
                    using (NpgsqlConnection conn = new NpgsqlConnection(_locContext.Database.GetDbConnection().ConnectionString))
                    {
                        DataTable dataTable = new DataTable();
                        List<Tk25Item> dataList = new List<Tk25Item>();
                        conn.Open();
                        NpgsqlCommand cmd = new NpgsqlCommand(@"Select ""TkNr"", ""Wgs84CenterLat"", ""Wgs84CenterLong"" FROM ""Map"".""Tk25""", conn);
                        NpgsqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            dataTable.Load(dr);
                            dataList = dataTable.AsEnumerable().Select(i => new Tk25Item()
                            {
                                Tk25Nr = i.Field<int>("TkNr"),
                                Wgs84CenterLat = i.Field<decimal>("Wgs84CenterLat"),
                                Wgs84CenterLong = i.Field<decimal>("Wgs84CenterLong"),
                            }).ToList();
                            Logger.Info(JsonConvert.SerializeObject(dataList));
                        }
                        else
                        {
                            Console.WriteLine("Data does not exist");
                        }
                        dr.Close();
                        return dataList;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private void SaveNewImage(AdviceImageJsonItem img_in, Observation obs)
        {
            try
            {
                Logger.Info("SaveNewImage");
                byte[] imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                string filename = Path.Combine(imgSavePath, img_in.ImageName);
                try
                {
                    System.IO.File.WriteAllBytes(filename, imageBytes);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                //Method to save images @bodentierhochvier.de / wordpress
                //--> imgName 
                Image img_new = new Image();
                img_new.ImagePath = "user_uploads/" + img_in.ImageName;
                //img_new.CmsId = img.CmsId;
                img_new.ObservationId = obs.ObservationId;
                img_new.Description = obs.ObservationComment;
                //img_new.CopyrightText = img.CopyrightText;
                img_new.LicenseId = 1;
                img_new.UserId = _userId;
                img_new.Author = obs.AuthorName;
                img_new.TaxonName = obs.TaxonName;
                img_new.TaxonId = obs.TaxonId;
                img_new.UserId = obs.UserId;
                _obsContext.Add(img_new);
                _obsContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SaveNewImage(AdviceImageJsonItem img_in, AdviceJsonItemSync obs, string userId, int observationId)
        {
            try
            {
                Logger.Info("SaveNewImage");
                byte[] imageBytes = Convert.FromBase64String(img_in.ImageBase64);
                string filename = Path.Combine(imgSavePath, img_in.ImageName);
                try
                {
                    System.IO.File.WriteAllBytes(filename, imageBytes);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                //Method to save images @bodentierhochvier.de / wordpress
                //--> imgName 
                Image img_new = new Image();
                img_new.ImagePath = "user_uploads/" + img_in.ImageName;
                //img_new.CmsId = img.CmsId;
                img_new.ObservationId = observationId;
                img_new.Description = obs.ImageLegend;
                //img_new.CopyrightText = img.CopyrightText;
                img_new.LicenseId = 1;
                img_new.UserId = _userId;
                img_new.Author = obs.ReportedByName;
                img_new.TaxonName = obs.TaxonFullName;
                img_new.TaxonId = obs.TaxonId;
                img_new.UserId = userId;
                _obsContext.Add(img_new);
                _obsContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public class Tk25Item
        {
            public int Tk25Nr { get; set; }
            public decimal Wgs84CenterLat { get; set; }
            public decimal Wgs84CenterLong { get; set; }
        }
    }
}