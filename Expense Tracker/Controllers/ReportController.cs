using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexAsync(DateTime? Month)
        {
            DateTime defaultMonth = Month ?? DateTime.Now;
            int selectedYear = defaultMonth.Year;
            int selectedMonthNumber = defaultMonth.Month;

            // This year This month Transaction Selection
           
            var SelectTransactions = await _context.Transactions
                 .Include(x => x.Category)
                 .Where(y => y.Date.Year == selectedYear && y.Date.Month == selectedMonthNumber)
                 .ToListAsync();

            ViewBag.SelectedMonth = defaultMonth.ToString("MMMM yyyy");

            // This year This Month Total Expense
            var ThisYearMonthExpenses = SelectTransactions
               .Where(i => i.Category.Type == "Expense")
               .Sum(y => (decimal?)y.Amount) ?? 0;

           ViewBag.ThisYearMonthExpenses = ThisYearMonthExpenses;

            // Last year This Month Total Expense
            var SelectLastYearTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Year == selectedYear - 1 && y.Date.Month == selectedMonthNumber)
                .ToListAsync();

             var LastYearMonthExpenses = SelectLastYearTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(y => (decimal?)y.Amount) ?? 0;

            ViewBag.LastYearMonthExpenses = LastYearMonthExpenses;

            var expenseComparison = new[]
{
    new { Month = defaultMonth.ToString("MMM"), ThisYear = ThisYearMonthExpenses, LastYear = LastYearMonthExpenses }
};

            ViewBag.ExpenseComparison = expenseComparison;

            // Bar Chart
            List<BarChartData> ExpenseSummary = SelectTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.Title)  // Group by Category
                .Select(k => new BarChartData
                {
                Category = k.Key,           // Category name
                Amount = k.Sum(j => j.Amount) // Sum of all amounts in that category
                })
            .ToList();

            ViewBag.ExpenseSummary = ExpenseSummary;

            //Frequent Transaction
            List<BarChartData> FrequentTransactions = SelectTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.Title)  // Group by Category
                .Select(k => new BarChartData
                {
                    Category = k.Key,// Category name
                    Count = k.Count(), // count of transactions in that category
                    Amount = k.Sum(j => j.Amount) // Sum of all amounts in that category
                   
                })
                .OrderByDescending(c=>c.Count) // Order by descending count
                .Take(5) // take only top 5 Categories
                .ToList();
            ViewBag.FrequentTransactions = FrequentTransactions;

            // Heat Map
            var heatMapData = SelectTransactions
                            .Select(t => t.Date.Date)   // just the date part
                            .Distinct()
                            .ToList();
            ViewBag.HeatMapData = heatMapData;

            return View();
        }
    }
    public class BarChartData
    {
         public string Category { get; set; }
    public decimal Amount { get; set; }  
    public int Count { get; set; }  
    }
 
}
