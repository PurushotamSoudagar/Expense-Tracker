using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index()
        {
            // Last 7 days Transactions
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            // Total Income
            int TotalIncome = SelectTransactions
                .Where(i => i.Category != null && i.Category.Type == "Income") // Added null check for Category
                .Sum(j => j.Amount);
            CultureInfo incomeculture = CultureInfo.CreateSpecificCulture("en-IN");
            incomeculture.NumberFormat.CurrencySymbol = "₹";
            ViewBag.TotalIncome = String.Format(incomeculture, "{0:C}", TotalIncome);
           

            // Total Expense
            int TotalExpense = SelectTransactions
                .Where(e => e.Category != null && e.Category.Type == "Expense") // Added null check for Category
                .Sum(j => j.Amount);
            CultureInfo expenseculture = CultureInfo.CreateSpecificCulture("en-IN");
            expenseculture.NumberFormat.CurrencySymbol = "₹";
            ViewBag.TotalExpense = String.Format(expenseculture, "{0:C}", TotalExpense);
         

            // Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-IN");   
            culture.NumberFormat.CurrencyNegativePattern = 1;
            culture.NumberFormat.CurrencySymbol = "₹";
            ViewBag.Balance = String.Format(culture, "{0:C}",Balance);

            //Doughnut Chart- Expense By Category
            ViewBag.DoughnutChart = SelectTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon+" "+k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),

                })
                .ToList();
             

            return View();
        }
    }
}
