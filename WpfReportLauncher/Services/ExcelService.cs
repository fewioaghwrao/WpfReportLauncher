using ClosedXML.Excel;
using Serilog;
using System.Data;
using System.IO;

using WpfReportLauncher.Models;


namespace WpfReportLauncher.Services
{
    public class ExcelService
    {
        private readonly string _outDir;
        public ExcelService(string outputDir) => _outDir = outputDir;


        public string GenerateSalesReport(IEnumerable<ReportItem> items)
        {
            Directory.CreateDirectory(_outDir);
            var path = Path.Combine(_outDir+ "/SalesReport", $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");


            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("売上");


            // ヘッダー
            ws.Cell(1, 1).Value = "日付";
            ws.Cell(1, 2).Value = "商品";
            ws.Cell(1, 3).Value = "数量";
            ws.Cell(1, 4).Value = "単価";
            ws.Cell(1, 5).Value = "金額";
            ws.Range(1, 1, 1, 5).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.AliceBlue;


            var r = 2;
            foreach (var it in items)
            {
                ws.Cell(r, 1).Value = it.Date;
                ws.Cell(r, 1).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(r, 2).Value = it.Product;
                ws.Cell(r, 3).Value = it.Quantity;
                ws.Cell(r, 4).Value = it.UnitPrice;
                ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(r, 5).FormulaA1 = $"=C{r}*D{r}";
                ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0";
                r++;
            }


            // 合計
            ws.Cell(r, 4).Value = "合計";
            ws.Cell(r, 5).FormulaA1 = $"=SUM(E2:E{r - 1})";
            ws.Range(r, 4, r, 5).Style.Font.Bold = true;


            ws.Columns().AdjustToContents();
            wb.SaveAs(path);


            Log.Information("Sales report generated: {Path}", path);
            return path;
        }
    }
}