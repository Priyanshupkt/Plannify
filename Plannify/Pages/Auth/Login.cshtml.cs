using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plannify.Models;
using System.ComponentModel.DataAnnotations;

namespace Plannify.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; } = new();

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            RedirectToPage("/Admin/Dashboard");
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(LoginInput.Email);
        if (user == null)
        {
            ViewData["ErrorMessage"] = "Invalid email or password";
            return Page();
        }

        if (!user.IsActive)
        {
            ViewData["ErrorMessage"] = "Your account has been deactivated. Please contact the administrator.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            LoginInput.Password,
            LoginInput.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var userRole = user.Role;
            var redirectUrl = userRole switch
            {
                "SuperAdmin" or "HOD" => "/Admin/Dashboard",
                "Teacher" => "/Teacher/Dashboard",
                _ => "/Index"
            };

            return LocalRedirect(redirectUrl);
        }

        if (result.IsLockedOut)
        {
            ViewData["ErrorMessage"] = "Your account has been locked due to multiple failed login attempts. Please try again later.";
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("/Auth/LoginWith2fa");
        }

        ViewData["ErrorMessage"] = "Invalid email or password";
        return Page();
    }
}
