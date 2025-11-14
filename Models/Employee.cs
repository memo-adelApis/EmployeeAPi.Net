namespace WebApplication1.Models

{
    public class Employee
    {
        // 👇 تأكد من وجود هذا السطر تمامًا
        public int Id { get; set; }

        public string Name { get; set; }
        public string Department { get; set; }
        public decimal Salary { get; set; }
    }
}
