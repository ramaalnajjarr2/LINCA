using LINCA_v1.Models;
using LINCA_v1.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Routing;

namespace LINCA_v1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        private static readonly List<string> Universities = new()
    {
        "University of Jordan",
        "Jordan University of Science and Technology",
        "Yarmouk University",
        "Hashemite University",
        "Al-Balqa Applied University",
        "German Jordanian University",
        "Mutah University",
        "Al al-Bayt University",
        "Tafilah Technical University",
        "Al-Hussein Bin Talal University",
        "Princess Sumaya University for Technology",
        "University of Petra",
        "Applied Science Private University",
        "Philadelphia University",
        "Amman Arab University",
        "Zarqa University",
        "Isra University",
        "Al-Zaytoonah University of Jordan"
    };

        [HttpGet]
        public async Task<IActionResult> ProfileSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            ViewBag.Universities = Universities;

            var vm = new ProfileSettings
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                University = user.University ?? "",
                DateOfBirth = user.DateOfBirth
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProfileSettings(ProfileSettings model)
        {
            ViewBag.Universities = Universities;

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // ✅ Update ONLY editable fields
            user.PhoneNumber = model.PhoneNumber;
            user.University = model.University;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            ViewBag.Success = "Profile updated successfully.";
            return View(model);
        }


        // --------------------
        // Register
        // --------------------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(Registeration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ PUT THIS HERE
            var email = (model.Email ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email is required.");
                return View(model);
            }

            var user = new Users
            {
                UserName = email,      // Identity uses this
                Email = email,         // Identity validates this
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.phonenum
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Welcome", "Home");
        }


        // --------------------
        // Login
        // --------------------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LogIn model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = (model.Email ?? "").Trim();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,     // ✅ مهم جداً
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
                return RedirectToAction("University", "Home");

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // --------------------
        // Logout
        // --------------------
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Welcome", "Home");
        }

        // --------------------
        // Forgot Password (Verify Email -> Token -> Reset)
        // --------------------
       

        [HttpGet]
        public IActionResult ResetEmailSent() => View();

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(ResetEmailSent));

            return View(new ResetPassword { Email = email, Token = token });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ResetDone));

            // ✅ Decode token back to original
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            return RedirectToAction(nameof(ResetDone));
        }


        [HttpGet]
        public IActionResult ResetDone() => View();
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmail model)
        {
            Console.WriteLine(">>> VerifyEmail POST HIT <<<");

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Don't reveal existence
            if (user == null)
                return RedirectToAction(nameof(ResetEmailSent));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // ✅ Encode token for URL safety
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var values = new RouteValueDictionary
            {
                ["email"] = user.Email,
                ["token"] = encodedToken
            };
            var link = Url.Action(
                action: nameof(ResetPassword),
                controller: "Account",
                values:values,
                protocol: Request.Scheme
            );

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset your password",
                $"Click to reset: {link}"
            );


            return RedirectToAction(nameof(ResetEmailSent));
        }

    }
}