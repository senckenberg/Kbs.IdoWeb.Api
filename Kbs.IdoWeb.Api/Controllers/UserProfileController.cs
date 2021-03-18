using Kbs.IdoWeb.Data.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserProfileController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly AuthenticationContext _authContext;
		public UserProfileController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AuthenticationContext authContext)
		{
			_authContext = authContext;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		private ApplicationUser CurrentUser
		{
			get
			{
				string requestingUserId = User.Claims.First(i => i.Type == "UserId").Value;
				var currentUser = _userManager.FindByIdAsync(requestingUserId).Result;
				if (currentUser == null)
				{
					throw new ArgumentNullException("The user of this token does not exist");
				}
				return _userManager.FindByIdAsync(requestingUserId).Result;
			}
		}

		[HttpGet]
		[Authorize]
		// GET api/UserProfile?email=
		public async Task<ActionResult> GetUserProfile(string email)
		{
			try
			{
				var user = await GetUserToUpdate(email);
				return Content(JsonConvert.SerializeObject(new
				{
					user.Id,
					user.Email,
					user.FirstName,
					user.LastName,
					user.Comment,
					user.DataRestrictionId,
					UserRoles = await _userManager.GetRolesAsync(user)
				}), "application/json");
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException)
					return Unauthorized(IdentityResult.Failed());
				else if (ex is ArgumentException)
					return NotFound(IdentityResult.Failed());
				else
					throw ex;
			}
		}

		[HttpPost("UpdateUser")]
		[Authorize]
		// Post api/UserProfile/UpdateUser
		public async Task<ActionResult> PostUpdateUser(UpdateUserModel updateModel)
		{
			try
			{
				var userToChange = await _userManager.FindByIdAsync(updateModel.Id);
				if (updateModel.Comment != null) userToChange.Comment = updateModel.Comment;
				if (updateModel.DataRestrictionId != 0) userToChange.DataRestrictionId = updateModel.DataRestrictionId;
				if (updateModel.Email != null)//todo: change only after getting email token
				{
					var normalizer = new UpperInvariantLookupNormalizer();
					userToChange.Email = updateModel.Email;
					userToChange.NormalizedEmail = normalizer.Normalize(updateModel.Email);
					userToChange.UserName = updateModel.Email;
					userToChange.NormalizedUserName = normalizer.Normalize(updateModel.Email);
				}
				if (updateModel.FirstName != null) userToChange.FirstName = updateModel.FirstName;
				if (updateModel.LastName != null) userToChange.LastName = updateModel.LastName;

				# region set roles
				if (updateModel.UserRoles!= null && IsAdmin(CurrentUser))
				{
					var rolesToSet = updateModel.UserRoles.Split(',');
					foreach (var role in rolesToSet)
					{
						if (!await _roleManager.RoleExistsAsync(role))
							return Ok(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidRoleName(role)));
					}
					var user = await _userManager.FindByIdAsync(updateModel.Id);
					var oldRoles = await _userManager.GetRolesAsync(user);
					await _userManager.RemoveFromRolesAsync(user, oldRoles);

					foreach (var role in rolesToSet)
					{
						await _userManager.AddToRoleAsync(user, role);
					}
					//prevent user to remove the own admin role
					if (CurrentUser==user && !rolesToSet.Contains("Admin"))
					{
						await _userManager.AddToRoleAsync(user, "Admin");
					}

				}
				#endregion set roles
				await _authContext.SaveChangesAsync();
				return Ok("");//the result has to be an empty string for kendoGrid
			}
			catch (Exception ex)
			{
				if (ex is UnauthorizedAccessException)
					return Unauthorized(IdentityResult.Failed());
				else if (ex is ArgumentException)
					return NotFound(IdentityResult.Failed());
				else
					return Ok(IdentityResult.Failed());
			}

		}

		#region admin only services

		[HttpGet("AllUsers")]
		[Authorize(Roles = "Admin")]
		// GET api/UserProfile/AllUsers
		public async Task<ActionResult<string>> GetAllUsers()
		{
			var allUsers = _userManager.Users.ToList();
			var customAllUsers = new List<dynamic>();
			var propertiesToDelete = new List<string> { "PasswordHash", "NormalizedUserName", "NormalizedEmail", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "AccessFailedCount", "LockoutEnd", "LockoutEnabled" };
			foreach (var user in allUsers)
			{
				var jUser = JObject.Parse(JsonConvert.SerializeObject(user));
				foreach (var prop in propertiesToDelete)
				{
					jUser.Remove(prop);
				}
				jUser.Add("UserRoles", String.Join(",", await _userManager.GetRolesAsync(user)));
				customAllUsers.Add(jUser);
			}
			return Content(JsonConvert.SerializeObject(customAllUsers), "application/json");

		}

		[HttpGet("AllRoles")]
		[Authorize(Roles = "Admin")]
		// Post api/UserProfile/AllRoles/
		public ActionResult<string> GetAllRoles()
		{
			return Content(JsonConvert.SerializeObject(_roleManager.Roles.ToList()), "application/json");
		}


		#endregion admin only services

		private async Task<ApplicationUser> GetUserToUpdate(string username)
		{
			ApplicationUser user;
			if (IsAdmin(CurrentUser) && !String.IsNullOrEmpty(username))
			{
				user = await _userManager.FindByEmailAsync(username);
				if (user == null)
				{
					throw new ArgumentException();
				}
			}
			else
			{
				if (String.IsNullOrEmpty(username) || username == CurrentUser.UserName)
				{
					user = CurrentUser;
				}
				else
				{
					throw new UnauthorizedAccessException();
				}
			}
			return user;
		}


		private bool IsAdmin(ApplicationUser user)
		{
			var userRoles = _userManager.GetRolesAsync(user).Result;
			return userRoles.Contains("Admin");
		}
	}
}