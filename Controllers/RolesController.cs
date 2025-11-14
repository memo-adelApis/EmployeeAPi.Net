using EmployeeApi.Models; // (1) لاستخدام DTO
using Microsoft.AspNetCore.Authorization; // (2) للحماية
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // (3) لاستخدام .ToListAsync()

namespace EmployeeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // (4) الأهم: حماية الـ Controller بالكامل للأدمن فقط
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        // (5) نحتاج فقط RoleManager لإدارة الأدوار
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // --- (أ) دالة جلب كل الأدوار ---
        // GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            // (6) جلب كل الأدوار من جدول "AspNetRoles"
            // نختار الأسماء فقط ليكون الرد نظيفاً
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            return Ok(roles);
        }

        // --- (ب) دالة إنشاء دور جديد ---
        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel model)
        {
            // (7) التحقق إذا كان الدور موجوداً بالفعل
            if (await _roleManager.RoleExistsAsync(model.RoleName))
            {
                return BadRequest("Role already exists.");
            }

            // (8) إنشاء الدور الجديد
            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));

            if (result.Succeeded)
            {
                return Ok(new { Message = "Role created successfully." });
            }

            return BadRequest(new { Message = "Failed to create role.", Errors = result.Errors });
        }

        // --- (ج) دالة حذف دور ---
        // DELETE: api/Roles/SomeRoleName
        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            // (9) البحث عن الدور
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // (10) تنفيذ الحذف
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Role deleted successfully." });
            }

            return BadRequest(new { Message = "Failed to delete role.", Errors = result.Errors });
        }
    }
}