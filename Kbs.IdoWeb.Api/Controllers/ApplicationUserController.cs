using Kbs.IdoWeb.Api.Middleware;
using Kbs.IdoWeb.Data.Authentication;
using Kbs.IdoWeb.Data.Observation;
using Kbs.IdoWeb.Data.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using NLog;
using Microsoft.EntityFrameworkCore;
using NLog.Fluent;
using System.Collections.Generic;

namespace Kbs.IdoWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PublicContext _publicContext;
        private readonly ObservationContext _obsContext;
        private readonly ApplicationSettings _appSettings;
        private readonly IConfiguration _smtpConfig;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly string _domain = ".bodentierhochvier.de";

        public ApplicationUserController(UserManager<ApplicationUser> userManager, IOptions<ApplicationSettings> appSettings, PublicContext pubContext, ObservationContext obsContext, IConfiguration smtpConfiguration)
        {
            _smtpConfig = smtpConfiguration;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _publicContext = pubContext;
            _obsContext = obsContext;
        }

        [HttpPost("Register")]
        //POST : /api/ApplicationUser/Register
        public async Task<Object> PostRegister(ApplicationUserModel model)
        {
            var applicationUser = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Comment = model.Comment,
                DataRestrictionId = model.DataRestrictionId
            };

            try
            {
                string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}"
                                    + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\"
                                    + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(model.Email))
                {
                    return IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(model.Email));
                }
                else
                {
                    var result = await _userManager.CreateAsync(applicationUser, model.Password);
                    if (result.Succeeded)
                    {
                        model.Role = "User";
                        await _userManager.AddToRoleAsync(applicationUser, model.Role);
                    }
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost("Login/Mobile")]
        public async Task<string> LoginMobileAsync(LoginModelMobile loginModel)
        {
            try
            {

                AspNetUserDevices device = _publicContext.AspNetUserDevices.Where(i => i.DeviceId == loginModel.deviceId).FirstOrDefault();

                if (device == null)
                {
                    var user = await _userManager.FindByNameAsync(Uri.UnescapeDataString(loginModel.username));
                    if (user != null && await _userManager.CheckPasswordAsync(user, Uri.UnescapeDataString(loginModel.password)))
                    {
                        device = new AspNetUserDevices
                        {
                            DeviceGuid = Guid.NewGuid(),
                            DeviceId = loginModel.deviceId,
                            UserId = user.Id,
                            LastAccess = DateTime.MinValue
                        };
                        _publicContext.Add(device);
                        _publicContext.SaveChanges();
                    }


                    MD5 md5 = System.Security.Cryptography.MD5.Create();

                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(device.DeviceId + device.DeviceGuid.ToString());

                    byte[] hash = md5.ComputeHash(inputBytes);


                    // step 2, convert byte array to hex string

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }

                    device.LastAccess = DateTime.Now;
                    device.DeviceHash = sb.ToString();
                    _publicContext.Update(device);
                    _publicContext.SaveChanges();

                    return sb.ToString();
                }
                else
                {
                    return device.DeviceHash.ToString();
                }
            }
            catch (Exception e)
            {
                return "invalid user";
            }
        }

        [HttpPost("Login/MobileV2")]
        public async Task<string> LoginMobileAsync_v2(LoginModelMobile loginModel)
        {
            try
            {

                AspNetUserDevices device = _publicContext.AspNetUserDevices.Where(i => i.DeviceId == loginModel.deviceId).FirstOrDefault();

                if (device == null)
                {
                    var user = await _userManager.FindByNameAsync(Uri.UnescapeDataString(loginModel.username));
                    if (user != null && await _userManager.CheckPasswordAsync(user, Uri.UnescapeDataString(loginModel.password)))
                    {
                        device = new AspNetUserDevices
                        {
                            DeviceGuid = Guid.NewGuid(),
                            DeviceId = loginModel.deviceId,
                            UserId = user.Id,
                            LastAccess = DateTime.MinValue
                        };
                        _publicContext.Add(device);
                        _publicContext.SaveChanges();
                    }


                    MD5 md5 = System.Security.Cryptography.MD5.Create();

                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(device.DeviceId + device.DeviceGuid.ToString());

                    byte[] hash = md5.ComputeHash(inputBytes);


                    // step 2, convert byte array to hex string

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }

                    device.LastAccess = DateTime.Now;
                    device.DeviceHash = sb.ToString();
                    _publicContext.Update(device);
                    _publicContext.SaveChanges();

                    return JsonConvert.SerializeObject(new { DeviceHash = sb.ToString(), FirstName = user.FirstName, LastName = user.LastName });
                }
                else
                {
                    var user = await _userManager.FindByNameAsync(Uri.UnescapeDataString(loginModel.username));
                    if (user != null)
                    {
                        //allow multiple logins on same device
                        if (user.Id != device.UserId)
                        {
                            device.UserId = user.Id;
                            device.LastAccess = DateTime.Now;
                            _publicContext.Update(device);
                            _publicContext.SaveChanges();
                        }
                        return JsonConvert.SerializeObject(new { DeviceHash = device.DeviceHash.ToString(), FirstName = user.FirstName, LastName = user.LastName });
                    }
                    return "invalid user";
                }
            }
            catch (Exception e)
            {
                return "invalid user";
            }
        }


        [HttpPost("Delete/Mobile")]
        public async Task<string> DeleteAccountFromMobileAsync(UserDeleteRequest userDeleteRequest)
        {
            try
            {
                if (userDeleteRequest != null)
                {
                    Logger.Error(JsonConvert.SerializeObject(userDeleteRequest));
                    //return JsonConvert.SerializeObject(userDeleteRequest);
                    if (!String.IsNullOrEmpty(userDeleteRequest.UserName) && !String.IsNullOrEmpty(userDeleteRequest.DeviceId) && !String.IsNullOrEmpty(userDeleteRequest.DeviceHash))
                    {
                        AspNetUserDevices device = _publicContext.AspNetUserDevices.Where(i => i.DeviceId == userDeleteRequest.DeviceId).FirstOrDefault();

                        if (device != null)
                        {
                            try
                            {
                                _publicContext.AspNetUserDevices.Remove(device);
                                _publicContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }


                            var user = await _userManager.FindByNameAsync(Uri.UnescapeDataString(userDeleteRequest.UserName));
                            if (user != null)
                            {
                                //_idoContext.AspNetUserDevices.Remove(device);
                                try
                                {
                                    List<Observation> obList = _obsContext.Observation.Where(obs => obs.UserId == user.Id).ToList();
                                    List<Event> evList = _obsContext.Event.Where(ev => ev.UserId == user.Id).ToList();
                                    _obsContext.Event.RemoveRange(evList);
                                    _obsContext.Observation.RemoveRange(obList);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }

                                IdentityResult iRes = await _userManager.DeleteAsync(user);
                                if (iRes != null)
                                {
                                    if (iRes.Succeeded)
                                    {
                                        return "success";
                                    }
                                    Logger.Error(iRes.Errors.ToString());
                                }
                                return iRes.Errors.ToString();
                            }
                            else
                            {
                                Logger.Warn(JsonConvert.SerializeObject(device));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.InnerException);
                return ex.InnerException.ToString();
            }
            return "error";
        }



        [HttpPost("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<ActionResult> PostLogin(LoginModel login)
        {
            var user = await _userManager.FindByNameAsync(Uri.UnescapeDataString(login.Email));
            if (user != null && await _userManager.CheckPasswordAsync(user, Uri.UnescapeDataString(login.Password)))
            {
                //get claims assigned to user
                var roles = await _userManager.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles.FirstOrDefault())
                    }),
                    Expires = DateTime.UtcNow.AddDays(21),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                Response.Cookies.Append(
                    "JWTToken",
                    token,
                    new CookieOptions()
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.None,//ONLY DURING DEVELOPMENT
                        Expires = DateTime.Now.AddDays(21),
                        Domain = _domain,
                        IsEssential = true,
                        //Secure = true
                    });
                /**
                AspNetUserTokens tok = new AspNetUserTokens();
                tok.UserId = user.Id;
                tok.Value = token;
                _idoContext.Add(tok);
                _idoContext.SaveChanges();
                **/
                return Ok(new { token });//ONLY DURING DEVELOPMENT
                                         //return Ok(IdentityResult.Success);
            }
            else
            {
                if (user == null)
                {
                    return Unauthorized(IdentityResult.Failed());
                }
                else
                {
                    return Unauthorized(IdentityResult.Failed(_userManager.ErrorDescriber.PasswordMismatch()));
                }
            }
        }


        [HttpPost("Register/Mobile")]
        public async Task<string> AddNewUserAsync(RegisterModelMobile regModel)
        {
            try
            {
                regModel.mail = regModel.mail.Trim();
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = regModel.mail,
                    Email = regModel.mail,
                    FirstName = regModel.givenname,
                    LastName = regModel.surname,
                    Comment = regModel.comment,
                    DataRestrictionId = 1
                };

                try
                {
                    string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}"
                                        + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\"
                                        + @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                    Regex re = new Regex(emailRegex);

                    if (!re.IsMatch(regModel.mail))
                    {
                        var iResult = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(applicationUser.Email));
                        return "invalid email";
                    }
                    else
                    {
                        var result = await _userManager.CreateAsync(applicationUser, regModel.password);
                        if (result.Succeeded)
                        {
                            string role = "User";
                            await _userManager.AddToRoleAsync(applicationUser, role);
                            return "success";
                        }
                        return result.Errors.First().Description.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost("Logout")]
        //POST : /api/ApplicationUser/Logout
        public ActionResult PostLogout()
        {
            Response.Cookies.Append("JWTToken", "", new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(-1),
                Domain = _domain,
                IsEssential = true
            });
            return Ok(IdentityResult.Success);
        }

        [HttpPost("ResetToken")]
        //POST : /api/ApplicationUser/SendResetMail
        public async Task<ActionResult> PostSendResetMail(dynamic EmailObject)
        {
            string userEmail = "";
            try { userEmail = EmailObject.Email; } catch (Exception) { }
            ApplicationUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _sendResetMail(userEmail, token);
                return Ok("success");//send this per Mail
            }
            else
            {
                return NotFound(IdentityResult.Failed());
            }
        }



        [HttpPost("PasswordReset")]
        //POST : /api/ApplicationUser/PasswordReset
        public async Task<ActionResult> PostPasswordReset(ResetPasswordModel reset)
        {
            string userEmail = reset.Email;
            ApplicationUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null)
            {
                IdentityResult result = await _userManager.ResetPasswordAsync(user, reset.Token, reset.NewPassword);
                try
                {

                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }
                return Ok(result);
            }
            else
            {
                return NotFound(IdentityResult.Failed());
            }
        }

        private async Task<ActionResult> _sendResetMail(string userEmail, string token)
        {
            string result;
            try
            {
                EMail.SendResetMail(userEmail, token, _smtpConfig);
                result = "success";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result = "Message: " + ex.Message + " InnerException: " + ex.InnerException.Message;
                return BadRequest(result);
            }
        }

        public class LoginModelMobile
        {
            public string username { get; set; }
            public string password { get; set; }
            public string deviceId { get; set; }
        }

        public class RegisterModelMobile
        {
            public string givenname { get; set; }
            public string surname { get; set; }
            public string mail { get; set; }
            public string password { get; set; }
            public string comment { get; set; }
            public string source { get; set; }
        }


        public class UserDeleteRequest
        {
            public string DeviceHash { get; set; }
            public string DeviceId { get; set; }
            public string UserName { get; set; }

            public UserDeleteRequest(string v1, string v2, string v3)
            {
                this.DeviceId = v1;
                this.DeviceHash = v2;
                this.UserName = v3;
            }
        }
    }
}