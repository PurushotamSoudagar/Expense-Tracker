using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            var budgetData = new List<object>
            {
                new { Category = "Food", Budget = 500, Expense = 650 },
                new { Category = "Rent", Budget = 1000, Expense = 1000 },
                new { Category = "Travel", Budget = 300, Expense = 250 },
                new { Category = "Shopping", Budget = 400, Expense = 550 },
                new { Category = "Bills", Budget = 600, Expense = 620 }
            };

            ViewBag.BudgetVsExpense = budgetData;
            return View();
        }
    }
}
