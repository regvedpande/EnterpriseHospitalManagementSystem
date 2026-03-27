using Hospital.Models.Enums;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;

namespace Hospital.Web.Areas.Accountant.Controllers
{
    [Area("Accountant")]
    [Authorize(Roles = WebSiteRoles.Website_Accountant)]
    public class HomeController : Controller
    {
        private readonly IBillService _bills;
        private readonly IPayrollService _payrolls;
        public HomeController(IBillService b, IPayrollService p)
        { _bills = b; _payrolls = p; }

        public IActionResult Index()
        {
            var allBills    = _bills.GetAll(1, 1000).Items;
            var allPayrolls = _payrolls.GetAll(1, 1000).Items;

            ViewBag.BillCount    = allBills.Count;
            ViewBag.PayrollCount = allPayrolls.Count;

            // Status breakdown
            ViewBag.PendingBills     = allBills.Count(b => b.Status == BillStatus.Pending);
            ViewBag.PaidBills        = allBills.Count(b => b.Status == BillStatus.Paid);
            ViewBag.OverdueBills     = allBills.Count(b => b.Status == BillStatus.Overdue);
            ViewBag.PartialBills     = allBills.Count(b => b.Status == BillStatus.PartiallyPaid);
            ViewBag.CancelledBills   = allBills.Count(b => b.Status == BillStatus.Cancelled);
            ViewBag.TotalRevenue     = allBills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.TotalBill);
            ViewBag.PendingRevenue   = allBills.Where(b => b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid).Sum(b => b.TotalBill);

            // Monthly revenue last 6 months
            var now = DateTime.Now;
            var months  = new string[6];
            var revenue = new decimal[6];
            for (int i = 5; i >= 0; i--)
            {
                var d = now.AddMonths(-i);
                months[5 - i]  = d.ToString("MMM yy");
                revenue[5 - i] = allBills
                    .Where(b => b.Status == BillStatus.Paid
                             && b.CreatedDate.Year  == d.Year
                             && b.CreatedDate.Month == d.Month)
                    .Sum(b => b.TotalBill);
            }
            ViewBag.MonthLabels     = JsonSerializer.Serialize(months);
            ViewBag.MonthlyRevenue  = JsonSerializer.Serialize(revenue);

            // Payroll status breakdown
            ViewBag.DraftPayrolls     = allPayrolls.Count(p => p.Status == PayrollStatus.Draft);
            ViewBag.ApprovedPayrolls  = allPayrolls.Count(p => p.Status == PayrollStatus.Approved);
            ViewBag.PaidPayrolls      = allPayrolls.Count(p => p.Status == PayrollStatus.Paid);
            ViewBag.TotalPayroll      = allPayrolls.Sum(p => p.NetSalary);

            // Recent
            ViewBag.RecentBills    = allBills.Take(6).ToList();
            ViewBag.RecentPayrolls = allPayrolls.Take(5).ToList();

            return View();
        }
    }
}
