using HealthCareManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static HealthCareManagementSystem.Repositories.IRepository;

namespace HealthCareManagementSystem.Areas.Identity.Controllers
{
    [Area(CD.IDENTITY_AREA)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;


        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOtp> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (registerVM == null)
            {
                return Content("Model is Null");
            }
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            // Check Username
            var existingUserName = await _userManager.FindByNameAsync(registerVM.UserName);

            if (existingUserName != null)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                return View(registerVM);
            }

            // Check Email
            var existingEmail = await _userManager.FindByEmailAsync(registerVM.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(registerVM);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Name = registerVM.Name,
                Address = registerVM.Address,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerVM);
            }

            TempData["Success-Notification"] = "User Registered Successfully";

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { area = "Identity", userId = user.Id, token },
                Request.Scheme);

            await _emailSender.SendEmailAsync(
                registerVM.Email,
                "Health Care Management System Confirm Email",
                $"Click <a href='{link}'>here</a> to confirm your email.");

            await _userManager.AddToRoleAsync(user, CD.PATIENT_ROLE);

            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                TempData["Error-Notification"] = "Invalid user ";
                return RedirectToAction(nameof(Login));
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["Error-Notification"] = "Can't Confirmed Email ";
                return RedirectToAction(nameof(Login));
            }
            TempData["Success-Notification"] = "Confirmed Email Successfuly";
            return RedirectToAction(nameof(Login));


        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            var user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password");
                return View(loginVM);
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "You are loked now try again to later");

                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Please Confirm Your Email First");

                }
                else
                {

                    ModelState.AddModelError("", "Invalid User Or Password");
                }
                return View(loginVM);
            }
            TempData["Success-Notification"] = " Logged In Successfuly";

            //return RedirectToAction("Index", "Home", new { area = "Patinet" });
            return RedirectToAction("Index", "Profile", new { area = "Identity" });

        }
        [HttpGet]
        public async Task<IActionResult> ResendEmailConfairmation()
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfairmation(ResendEmailConfirmationVM resendEmailConfairmation)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfairmation.UserNameOrEmail)
              ?? await _userManager.FindByNameAsync(resendEmailConfairmation.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password");
                return View(resendEmailConfairmation);
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(
                user.Email,
                "Health Care Management System Confirm Email",
                $"<h1>click <a href={link}> here </a> to confirm Your Email </h1>");
            TempData["Success-Notification"] = "Resend Email Confairmation Successfully ";

            return RedirectToAction(nameof(Login));

        }
       
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOrEmail)
              ?? await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password");
                return View(forgetPasswordVM);
            }
            var applicationUserOtps = await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var count = applicationUserOtps.Count(e => (DateTime.UtcNow - e.CreatedAt).TotalHours <= 24);
            if (count >= 5)
            {
                ModelState.AddModelError("", "To many attmpes please try again later");
                return View(forgetPasswordVM);
            }
            var otp = new Random().Next(1000, 9999).ToString();
            var applicationUserOtp = new ApplicationUserOtp(user.Id, otp);
            await _applicationUserOtpRepository.AddAsync(applicationUserOtp);
            await _applicationUserOtpRepository.CommitAsync();
            await _emailSender.SendEmailAsync(
               user.Email,
               "Health Care Management System Forget Password",
               $"<h1>use this otp <span style =\"color: red\">{otp}</span> to reset your password </h1>");
            return RedirectToAction(nameof(ValidateOtp), new { userId = user.Id });
        }
        // رمز التحقق
        [HttpGet]
        public IActionResult ValidateOtp(string userId)
        {
            return View(new ValidateOtpVM() { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOtp(ValidateOtpVM validateOtpVM)
        {
            if (!ModelState.IsValid)
            {
                return View(validateOtpVM);
            }
            var user = await _userManager.FindByIdAsync(validateOtpVM.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "invalid user ");
                return View(validateOtpVM);

            }
            var otps = await _applicationUserOtpRepository.GetAsync(e =>
               e.ApplicationUserId == user.Id &&
               e.IsValid == true &&
               e.ValidTo >= DateTime.UtcNow
                );
            var otp = otps.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
            if (otp is null || otp.Code != validateOtpVM.OTP)
            {
                ModelState.AddModelError("", "invalid / Expired OTP ");
                return View(validateOtpVM);
            }
            otp.IsValid = false;
            await _applicationUserOtpRepository.CommitAsync();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            TempData["token"] = token;
            return RedirectToAction(nameof(NewPassword), new { userId = user.Id });
        }
        [HttpGet]
        public IActionResult NewPassword(string userId)
        {
            var token = TempData["token"] as string;
            if (token is null)
            {
                return RedirectToAction(nameof(Login));
            }
            return View(new NewPasswordVM() { UserId = userId, Token = token });
        }
        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (newPasswordVM.Token is null)
            {
                return RedirectToAction(nameof(Login));
            }
            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "invalid user ");
                return View(newPasswordVM);

            }
            var result = await _userManager.ResetPasswordAsync(user, newPasswordVM.Token, newPasswordVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(newPasswordVM);
            }
            return RedirectToAction(nameof(Login));
        }
      
    }
}
