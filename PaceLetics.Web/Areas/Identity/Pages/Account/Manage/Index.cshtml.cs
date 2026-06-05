// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AthleteDataAccessLibrary.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PaceLetics.AthleteModule.CodeBase.Models;
using PaceLetics.Web.Configuration;
using PaceLetics.Web.Data;

namespace PaceLetics.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TrainerVerificationOptions _trainerVerificationOptions;
        private readonly IAthleteData _athleteData;
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<TrainerVerificationOptions> trainerVerificationOptions,
            IAthleteData athleteData,
            IStringLocalizer<IndexModel> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _trainerVerificationOptions = trainerVerificationOptions.Value;
            _athleteData = athleteData;
            _localizer = localizer;
        }

        [Display(Name = "Name")]
        public string Username { get; set; }

        public string Roles { get; set; }

        public bool IsTrainer { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(64, MinimumLength = 3)]
            [Display(Name = "Oeffentlicher Nutzername")]
            public string PublicUserName { get; set; }

            [Url]
            [Display(Name = "Profilfoto URL")]
            public string ProfileImageUrl { get; set; }

            [Display(Name = "Oeffentliches Profil sichtbar")]
            public bool IsPublicProfileVisible { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Trainer-Verifikationscode")]
            public string TrainerVerificationCode { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var athlete = await GetOrCreateAthleteAsync(user.Id, userName, roles);
            var publicProfile = athlete.PublicProfile;

            Username = userName;
            Roles = string.Join(", ", roles.Select(GetRoleDisplayName));
            IsTrainer = roles.Contains(ApplicationRoles.Trainer);

            Input = new InputModel
            {
                PublicUserName = publicProfile?.PublicUserName ?? userName,
                ProfileImageUrl = publicProfile?.ProfileImageUrl,
                IsPublicProfileVisible = publicProfile?.IsProfileVisible ?? false
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var publicUserName = Input.PublicUserName?.Trim();
            if (!string.IsNullOrWhiteSpace(publicUserName))
            {
                if (!IsPublicUserNameFormatValid(publicUserName))
                {
                    ModelState.AddModelError("Input.PublicUserName", _localizer["PublicUserNameCharacters"]);
                }
                else if (await PublicUserNameExistsAsync(publicUserName, user.Id))
                {
                    ModelState.AddModelError("Input.PublicUserName", _localizer["PublicUserNameExists"]);
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains(ApplicationRoles.Trainer)
                ? ApplicationRoles.Trainer
                : ApplicationRoles.Athlete;

            if (!string.IsNullOrWhiteSpace(Input.TrainerVerificationCode))
            {
                var trainerResult = await TryAddTrainerRoleAsync(user, Input.TrainerVerificationCode);
                if (!trainerResult)
                {
                    await LoadAsync(user);
                    return Page();
                }

                role = ApplicationRoles.Trainer;
                roles = await _userManager.GetRolesAsync(user);
            }

            var athlete = await GetOrCreateAthleteAsync(user.Id, await _userManager.GetUserNameAsync(user), roles);
            athlete.Roles = CreateRoleModel(roles);
            athlete.PublicProfile = new PublicProfileModel
            {
                PublicUserName = publicUserName,
                NormalizedPublicUserName = NormalizePublicUserName(publicUserName),
                ProfileImageUrl = Input.ProfileImageUrl?.Trim(),
                IsProfileVisible = Input.IsPublicProfileVisible,
                PublicRole = role
            };

            await _athleteData.UpdateAthlete(athlete);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["ProfileUpdated"];
            return RedirectToPage();
        }

        private async Task<bool> TryAddTrainerRoleAsync(ApplicationUser user, string verificationCode)
        {
            if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Trainer))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(_trainerVerificationOptions.Code))
            {
                ModelState.AddModelError("Input.TrainerVerificationCode", _localizer["TrainerVerificationNotConfigured"]);
                return false;
            }

            if (!IsVerificationCodeValid(verificationCode, _trainerVerificationOptions.Code))
            {
                ModelState.AddModelError("Input.TrainerVerificationCode", _localizer["TrainerVerificationInvalid"]);
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, ApplicationRoles.Trainer);
            if (result.Succeeded)
            {
                return true;
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return false;
        }

        private static bool IsVerificationCodeValid(string inputCode, string configuredCode)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputCode.Trim());
            var configuredBytes = Encoding.UTF8.GetBytes(configuredCode.Trim());

            return inputBytes.Length == configuredBytes.Length
                && CryptographicOperations.FixedTimeEquals(inputBytes, configuredBytes);
        }

        private async Task<AthleteModel> GetOrCreateAthleteAsync(
            string userId,
            string userName,
            IList<string> roles)
        {
            var athlete = await _athleteData.GetAthlete(userId);
            var shouldSave = false;

            if (athlete is null)
            {
                athlete = new AthleteModel
                {
                    Id = userId,
                    Name = userName
                };
                shouldSave = true;
            }

            var roleModel = CreateRoleModel(roles);
            if (!HasSameRoles(athlete.Roles, roleModel))
            {
                athlete.Roles = roleModel;
                shouldSave = true;
            }

            if (IsMissingPublicProfile(athlete.PublicProfile))
            {
                athlete.PublicProfile = CreateDefaultPublicProfile(userName, roles);
                shouldSave = true;
            }
            else
            {
                var normalizedPublicUserName = NormalizePublicUserName(athlete.PublicProfile.PublicUserName);
                if (athlete.PublicProfile.NormalizedPublicUserName != normalizedPublicUserName)
                {
                    athlete.PublicProfile.NormalizedPublicUserName = normalizedPublicUserName;
                    shouldSave = true;
                }
            }

            if (shouldSave)
            {
                await _athleteData.UpdateAthlete(athlete);
            }

            return athlete;
        }

        private async Task<bool> PublicUserNameExistsAsync(string publicUserName, string exceptUserId)
        {
            var normalizedPublicUserName = NormalizePublicUserName(publicUserName);
            var athletes = await _athleteData.GetAthletes();

            return athletes.Any(athlete =>
                athlete.Id != exceptUserId
                && athlete.PublicProfile is not null
                && athlete.PublicProfile.NormalizedPublicUserName == normalizedPublicUserName);
        }

        private static PublicProfileModel CreateDefaultPublicProfile(string userName, IList<string> roles)
        {
            return new PublicProfileModel
            {
                PublicUserName = userName,
                NormalizedPublicUserName = NormalizePublicUserName(userName),
                ProfileImageUrl = null,
                IsProfileVisible = false,
                PublicRole = GetPublicRole(roles)
            };
        }

        private static RoleModel CreateRoleModel(IList<string> roles)
        {
            var assignedRoles = roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct()
                .ToList();

            if (!assignedRoles.Contains(ApplicationRoles.Athlete))
            {
                assignedRoles.Insert(0, ApplicationRoles.Athlete);
            }

            return new RoleModel
            {
                AssignedRoles = assignedRoles
            };
        }

        private static bool HasSameRoles(RoleModel currentRoleModel, RoleModel newRoleModel)
        {
            var currentRoles = currentRoleModel?.AssignedRoles ?? new List<string>();
            return currentRoles.OrderBy(role => role).SequenceEqual(
                newRoleModel.AssignedRoles.OrderBy(role => role));
        }

        private static bool IsMissingPublicProfile(PublicProfileModel publicProfile)
        {
            return publicProfile is null
                || string.IsNullOrWhiteSpace(publicProfile.PublicUserName)
                || publicProfile.PublicUserName == "NA";
        }

        private static string GetPublicRole(IList<string> roles)
        {
            return roles.Contains(ApplicationRoles.Trainer)
                ? ApplicationRoles.Trainer
                : ApplicationRoles.Athlete;
        }

        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                ApplicationRoles.Athlete => _localizer["RoleAthlete"],
                ApplicationRoles.Trainer => _localizer["RoleTrainer"],
                _ => role
            };
        }

        private static string NormalizePublicUserName(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static bool IsPublicUserNameFormatValid(string value)
        {
            return Regex.IsMatch(value, @"\A[A-Za-z0-9._-]+\z");
        }
    }
}
