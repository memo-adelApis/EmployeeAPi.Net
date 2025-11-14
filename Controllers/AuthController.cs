using EmployeeApi.Models; // (1) لاستخدام الـ DTOs
using Microsoft.AspNetCore.Identity; // (2) لاستخدام UserManager و RoleManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens; // (3) لاستخدام مفاتيح التشفير
using System.IdentityModel.Tokens.Jwt; // (4) لإنشاء التوكن
using System.Security.Claims; // (5) لتعريف "Claims"
using System.Text; // (6) لـ Encoding

namespace EmployeeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        // (7) نقوم بعمل "Inject" للخدمات التي نحتاجها
        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // --- (أ) دالة تسجيل مستخدم جديد ---
        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // (8) التحقق أولاً إذا كان اسم المستخدم موجود
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { Message = "User already exists!" });
            }

            // (9) إنشاء كائن المستخدم الجديد
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(), // (مهم)
                UserName = model.Username
            };

            // (10) حفظ المستخدم في قاعدة البيانات (سيقوم Identity بتشفير كلمة المرور)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // إذا فشل، أرجع رسائل الخطأ (مثل: كلمة المرور ضعيفة)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "User creation failed!", Errors = result.Errors });
            }

            // (اختياري: إضافة المستخدم الجديد تلقائياً لدور "User")
            // await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Message = "User created successfully!" });
        }


        // --- (ب) دالة تسجيل الدخول ---
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // (11) البحث عن المستخدم
            var user = await _userManager.FindByNameAsync(model.Username);

            // (12) التحقق من المستخدم وكلمة المرور
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // (13) إذا كان صحيحاً، قم بإنشاء التوكن
                var (token, expiration) = await GenerateJwtToken(user);

                // (14) إرجاع التوكن للـ Front-End
                return Ok(new AuthResponse(token, expiration));
            }

            // (15) إذا فشل تسجيل الدخول
            return Unauthorized(new { Message = "Invalid username or password" });
        }


        // --- (ج) دالة مساعدة لإنشاء التوكن ---
        private async Task<(string token, DateTime expiration)> GenerateJwtToken(IdentityUser user)
        {
            // (16) جلب "الأدوار" (Roles) الخاصة بالمستخدم
            var userRoles = await _userManager.GetRolesAsync(user);

            // (17) إنشاء قائمة الـ "Claims" (المعلومات التي سنضعها داخل التوكن)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id) // (مهم: UserId)
            };

            // (18) إضافة الأدوار إلى الـ Claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // (19) جلب المفتاح السري من "appsettings.json"
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // (20) إنشاء التوكن
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddHours(3), // (مدة صلاحية التوكن)
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var expiration = token.ValidTo;
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // (21) إرجاع التوكن ومدته
            return (tokenString, expiration);
        }
        // --- (د) دالة لإنشاء الأدوار (لأول مرة فقط) ---
        // POST: api/auth/seed-roles
        [HttpPost("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            // (22) التحقق إذا كان دور "Admin" موجوداً
            bool adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                // (23) إذا لم يكن موجوداً، قم بإنشائه
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // (24) التحقق إذا كان دور "User" موجوداً
            bool userRoleExists = await _roleManager.RoleExistsAsync("User");
            if (!userRoleExists)
            {
                // (25) إذا لم يكن موجوداً، قم بإنشائه
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            return Ok(new { Message = "Roles seeded successfully!" });
        }
    }
}