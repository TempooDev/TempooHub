using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TempooHub.AuthServer.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // El parámetro 'false' final es para no bloquear la cuenta tras fallos
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: true, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin logueado correctamente.");
                    return RedirectToPage("/Admin/Users");
                }
                
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            }

            return Page();
        }
    }
}