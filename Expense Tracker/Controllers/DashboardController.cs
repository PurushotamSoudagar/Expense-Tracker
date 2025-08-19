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
        public async Task<ActionResult> Index(DateTime? DateStart, DateTime? DateEnd)
        {
            // Total In range

            DateTime StartDate = DateStart ?? DateTime.Now.AddDays(-7);
            DateTime EndDate = DateEnd ?? DateTime.Now;
            // Pass selected range to ViewBag for DateRangePicker
            ViewBag.StartDate = StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = EndDate.ToString("yyyy-MM-dd");

            var SelectTransactions = await _context.Transactions
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
                .OrderBy(l=> l.amount)
                .ToList();

            //Spline-Chart- Income Vs Expense
            //Income
            List<SplineChartData> IncomeSummary = SelectTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData
                {
                    day = k.First().Date.ToString("dd-MM"),
                    income = k.Sum(j => j.Amount),
                   
                })
                .ToList();
            //Expense
            List<SplineChartData> ExpenseSummary = SelectTransactions
               .Where(i => i.Category.Type == "Expense")
               .GroupBy(j => j.Date)
               .Select(k => new SplineChartData
               {
                   day = k.First().Date.ToString("dd-MM"),
                   expense = k.Sum(j => j.Amount),
               })
               .ToList();

            //combine Income and expense
            string[] range = Enumerable.Range(0, (EndDate - StartDate).Days + 1)
                .Select(i => StartDate.AddDays(i).ToString("dd-MM"))
                .ToArray();

            ViewBag.SplineChartData = from day in range
                                  join income in IncomeSummary on day equals income.day into dayincomeJoined
                                  from income in dayincomeJoined.DefaultIfEmpty()
                                  join expense in ExpenseSummary on day equals expense.day into dayexpenseJoined
                                  from expense in dayexpenseJoined.DefaultIfEmpty()
                                  select new
                                  {
                                      day = day,
                                      income = income ==null? 0: income.income, // Use null-coalescing operator to handle nulls
                                      expense = expense == null? 0: expense.expense, // Use null-coalescing operator to handle nulls
                                      
                                  };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
