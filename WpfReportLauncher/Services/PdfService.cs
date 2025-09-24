using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;
using System.IO;
using WpfReportLauncher.Models;


namespace WpfReportLauncher.Services
{
    public class PdfService
    {
        private readonly string _outDir;
        private readonly string _company;
        private readonly string _issuer;
        private readonly decimal _taxRate;


        public PdfService(string outputDir, string companyName, string issuerName, decimal taxRate)
        {
            _outDir = outputDir;
            _company = companyName;
            _issuer = issuerName;
            _taxRate = taxRate;
        }


        public string GenerateInvoice(string invoiceNo, string billTo, IEnumerable<InvoiceItem> items)
        {
            Directory.CreateDirectory(_outDir);
            var path = Path.Combine(_outDir+ "/Invoice", $"Invoice_{invoiceNo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");


            var itemList = items.ToList();
            var subtotal = itemList.Sum(i => i.Subtotal);
            var tax = Math.Round(subtotal * _taxRate, 0, MidpointRounding.AwayFromZero);
            var total = subtotal + tax;


            Document.Create(container =>
            {
            container.Page(page =>
            {
            page.Margin(30);
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_company).FontSize(18).SemiBold();
                    col.Item().Text(_issuer).FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"発行日: {DateTime.Now:yyyy/MM/dd}");
                    col.Item().Text($"請求書番号: {invoiceNo}");
                });
                row.ConstantItem(120).Height(50).Background(Colors.Grey.Lighten3).AlignCenter().AlignMiddle().Text("INVOICE").SemiBold();
            });


            page.Content().Column(col =>
            {
            col.Spacing(10);
            col.Item().Text($"ご請求先: {billTo}").FontSize(12);


            col.Item().Table(table =>
            {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(5); // 品目
                cols.RelativeColumn(2); // 数量
                cols.RelativeColumn(3); // 単価
                cols.RelativeColumn(3); // 小計
            });


                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("品目");
                    header.Cell().Element(CellHeader).Text("数量");
                    header.Cell().Element(CellHeader).Text("単価");
                    header.Cell().Element(CellHeader).Text("小計");


                    static IContainer CellHeader(IContainer container) => container
                    .DefaultTextStyle(x => x.SemiBold())
                    .Background(Colors.Grey.Lighten3)
                    .Padding(6);
                });


                foreach (var it in itemList)
                {
                    table.Cell().Element(Cell).Text(it.Description);
                    table.Cell().Element(Cell).Text(it.Quantity.ToString());
                    table.Cell().Element(Cell).Text(it.UnitPrice.ToString("#,##0"));
                    table.Cell().Element(Cell).Text(it.Subtotal.ToString("#,##0"));


                    static IContainer Cell(IContainer container) => container.Padding(6);
                }
            });


                col.Item().AlignRight().Column(sum =>
                {
                    sum.Item().Text($"小計: {subtotal:#,##0}");
                    sum.Item().Text($"消費税({(int)(_taxRate * 100)}%): {tax:#,##0}");
                    sum.Item().Text($"合計: {total:#,##0}").SemiBold().FontSize(14);
                });
            });


                page.Footer().AlignCenter().Text("本書類はシステムにより自動生成されています").FontColor(Colors.Grey.Darken2).FontSize(9);
            });
            }).GeneratePdf(path);


            Log.Information("Invoice generated: {Path}", path);
            return path;
        }
    }
}