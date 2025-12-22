using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TempooHub.AuthServer.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken]
    public class RolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        [BindProperty] public string NewRoleName { get; set; } = "";

        public List<IdentityRole> Roles { get; set; } = new();

        public RolesModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task OnGetAsync()
        {
                Roles = await _roleManager.Roles.ToListAsync<IdentityRole>();
            }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(NewRoleName)) return RedirectToPage();
            var roleName = NewRoleName.Trim();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage();
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
            return RedirectToPage();
        }
    }
}
