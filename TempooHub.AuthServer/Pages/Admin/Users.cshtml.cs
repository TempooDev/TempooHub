using System.Text;
using System.Security.Cryptography;

namespace TempooHub.AuthServer.Pages.Admin
{
    [Authorize(Roles = "Admin")] // SOLO administradores pueden entrar
    [IgnoreAntiforgeryToken]
    public partial class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender? _emailSender;
        [BindProperty] public string NewUserEmail { get; set; } = "";
        [BindProperty] public string NewUserPassword { get; set; } = "";
        public UsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = serviceProvider.GetService<IEmailSender>();
        }

        public List<UserViewModel> Users { get; set; } = new();
        public List<string> AllRoles { get; set; } = new();
        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            AllRoles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
            foreach (var user in users)
            {
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Roles = await _userManager.GetRolesAsync(user),
                    EmailConfirmed = await _userManager.IsEmailConfirmedAsync(user)
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = new IdentityUser { UserName = NewUserEmail, Email = NewUserEmail };
            var result = await _userManager.CreateAsync(user, NewUserPassword);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateRolesAsync(string userId, string[] selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            // If no roles selected, ensure we pass an empty array instead of null
            selectedRoles ??= Array.Empty<string>();

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (selectedRoles.Length > 0)
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendConfirmationAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            if (_emailSender != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var callbackUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}";
                var subject = "Confirmar correo - TempooHub";
                var body = $"Por favor confirma tu correo haciendo clic en el siguiente enlace: <a href=\"{callbackUrl}\">Confirmar email</a>";
                await _emailSender.SendEmailAsync(user.Email, subject, body);
                TempData["ConfirmationData"] = $"Enviado a: {user.Email}";
            }
            else
            {
                // Expose the encoded token and user id so admin can construct a confirmation link
                TempData["ConfirmationData"] = $"userId={user.Id}&code={code}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostGenerateTempPasswordAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var tempPassword = GenerateTempPassword(12);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, tempPassword);
            if (reset.Succeeded)
            {
                TempData["TempPassword"] = tempPassword;
                TempData["TempPasswordUser"] = user.Email ?? user.UserName ?? user.Id;
                if (_emailSender != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    var subject = "Contraseña temporal - TempooHub";
                    var body = $"Se ha generado una contraseña temporal: <strong>{tempPassword}</strong>. Por favor inicia sesión y cámbiala.";
                    await _emailSender.SendEmailAsync(user.Email, subject, body);
                    TempData["TempPassword"] = "(Enviada por correo)";
                }
            }
            else
            {
                TempData["TempPasswordError"] = string.Join("; ", reset.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        private static string GenerateTempPassword(int length = 12)
        {
            const string lowers = "abcdefghijklmnopqrstuvwxyz";
            const string uppers = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()-_+=";
            var all = lowers + uppers + digits + symbols;

            var chars = new char[length];
            // ensure at least one of each type
            chars[0] = lowers[RandomNumberGenerator.GetInt32(lowers.Length)];
            chars[1] = uppers[RandomNumberGenerator.GetInt32(uppers.Length)];
            chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            chars[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

            for (int i = 4; i < length; i++)
            {
                chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }

            // shuffle
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                var tmp = chars[i];
                chars[i] = chars[j];
                chars[j] = tmp;
            }

            return new string(chars);
        }
    }
}