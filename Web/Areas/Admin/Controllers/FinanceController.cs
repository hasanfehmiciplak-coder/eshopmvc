using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FinanceController : Controller
{
    private readonly AppDbContext _context;

    public FinanceController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var totalRevenue = _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        var totalRefund = _context.Refunds
.Where(r => r.Status == RefundStatus.Success)
.Sum(r => (decimal?)r.Amount) ?? 0;

        var netRevenue = totalRevenue - totalRefund;

        var orderCount = _context.Orders.Count();

        var dailyReport = _context.Orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.Date)
            .Take(14)
            .ToList();

        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalRefund = totalRefund;
        ViewBag.NetRevenue = netRevenue;
        ViewBag.OrderCount = orderCount;
        ViewBag.DailyReport = dailyReport;

        return View();
    }

    [HttpGet]
    public IActionResult ExportCsv()
    {
        var orders = _context.Orders
            .OrderBy(o => o.OrderDate)
            .Select(o => new
            {
                Date = o.OrderDate,
                Total = o.TotalPrice,
                Status = o.Status.ToString()
            })
            .ToList();

        var refunds = _context.Refunds
.Where(r => r.Status == RefundStatus.Success)
            .Select(r => new
            {
                Date = r.CreatedAt,
                Amount = r.Amount
            })
            .ToList();

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Tarih,Tip,Tutar,Durum");

        foreach (var o in orders)
        {
            sb.AppendLine($"{o.Date:yyyy-MM-dd},Sipariş,{o.Total},{o.Status}");
        }

        foreach (var r in refunds)
        {
            sb.AppendLine($"{r.Date:yyyy-MM-dd},İade,-{r.Amount},SUCCESS");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

        return File(
            bytes,
            "text/csv",
            $"finans-raporu-{DateTime.Now:yyyyMMdd}.csv"
        );
    }

    [HttpGet]
    public IActionResult ExportExcel()
    {
        var orders = _context.Orders
            .Select(o => new
            {
                Date = o.OrderDate,
                Amount = o.TotalPrice,
                Type = "Sipariş",
                Status = o.Status.ToString()
            })
            .ToList();

        var refunds = _context.Refunds
.Where(r => r.Status == RefundStatus.Success)
            .Select(r => new
            {
                Date = r.CreatedAt,
                Amount = -r.Amount, // iade eksi
                Type = "İade",
                Status = "SUCCESS"
            })
            .ToList();

        var data = orders.Concat(refunds)
            .OrderBy(x => x.Date)
            .ToList();

        using var stream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(
            stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Finans Raporu"
            });

            // HEADER
            sheetData.AppendChild(CreateRow(
                "Tarih", "Tip", "Tutar (₺)", "Durum"));

            // DATA
            foreach (var item in data)
            {
                sheetData.AppendChild(CreateRow(
                    item.Date.ToString("dd.MM.yyyy"),
                    item.Type,
                    item.Amount.ToString("0.00"),
                    item.Status
                ));
            }

            workbookPart.Workbook.Save();
        }

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"finans-raporu-{DateTime.Now:yyyyMMdd}.xlsx"
        );
    }

    private Row CreateRow(params string[] values)
    {
        var row = new Row();

        foreach (var value in values)
        {
            row.AppendChild(new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(value)
            });
        }

        return row;
    }
}