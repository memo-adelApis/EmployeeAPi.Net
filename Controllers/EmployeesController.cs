using EmployeeApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // (2) نحتاج الـ Model

namespace EmployeeApi.Controllers
{
    [Route("api/[controller]")] // المسار سيكون: api/Employees
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        // (3) متغير خاص ليحمل نسخة الـ DbContext
        private readonly ApplicationDbContext _context;

        // (4) هذا هو "حقن التبعية" (DI)
        // عند إنشاء EmployeesController، سيقوم ASP.NET Core بتمرير DbContext جاهز لنا
        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- (أ) دالة جلب كل الموظفين (GET) ---
        // GET: api/Employees
        [HttpGet]
        [Authorize] // (3) نحدد الدور هنا
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            // نستخدم async/await لعدم حجز الـ thread أثناء انتظار قاعدة البيانات
            var employees = await _context.Employees.ToListAsync();
            return Ok(employees); // إرجاع نتيجة 200 OK مع قائمة الموظفين
        }

        // --- (ب) دالة إضافة موظف جديد (POST) ---
        // POST: api/Employees
        [HttpPost]
        [Authorize(Roles = "Admin")] // (3) نحدد الدور هنا
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            // (1) أضف الموظف الجديد إلى "الجدول" في الذاكرة
            _context.Employees.Add(employee);

            // (2) احفظ التغييرات فعلياً في قاعدة البيانات
            await _context.SaveChangesAsync();

            // (3) إرجاع نتيجة 201 Created مع بيانات الموظف الذي تم إنشاؤه
            // هذا يتبع أفضل ممارسات REST API
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
        }

        // --- (ج) دالة مساعدة لـ PostEmployee (غير مطلوبة للتشغيل ولكنها لإكمال المثال) ---
        // GET: api/Employees/5 
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound(); // إرجاع 404 إذا لم يتم العثور عليه
            }

            return Ok(employee);
        }
    }
}