using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TempooHub.AuthServer.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken]
    public class RoleClaimsModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityRole Role { get; set; }
        public IList<Claim> Claims { get; set; }

        [BindProperty]
        public string NewClaimType { get; set; } = "";

        [BindProperty]
        public string NewClaimValue { get; set; } = "";

        public RoleClaimsModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            Role = role;
            Claims = await _roleManager.GetClaimsAsync(role);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            if (string.IsNullOrWhiteSpace(NewClaimType) || string.IsNullOrWhiteSpace(NewClaimValue))
            {
                return RedirectToPage(new { id });
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var claim = new Claim(NewClaimType.Trim(), NewClaimValue.Trim());
            var result = await _roleManager.AddClaimAsync(role, claim);

            if (result.Succeeded)
            {
                return RedirectToPage(new { id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            Role = role;
            Claims = await _roleManager.GetClaimsAsync(role);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id, string claimType, string claimValue)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var claim = new Claim(claimType, claimValue);
            await _roleManager.RemoveClaimAsync(role, claim);

            return RedirectToPage(new { id });
        }
    }
}
