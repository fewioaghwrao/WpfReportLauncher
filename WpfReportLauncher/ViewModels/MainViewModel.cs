using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using WpfReportLauncher.Models;
using WpfReportLauncher.Services;
using WpfReportLauncher.Views;

namespace WpfReportLauncher.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private ExcelService _excel;
        private PdfService _pdf;
        private string _outDir;

        [ObservableProperty]
        private string status = "準備完了";

        // 商品追加機能のプロパティ
        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private int quantity = 1;

        [ObservableProperty]
        private decimal displayPrice;

        public IRelayCommand GenerateSalesReportCommand { get; }
        public IRelayCommand GenerateInvoicePdfCommand { get; }
        public IRelayCommand OpenOutputFolderCommand { get; }
        public ICommand OpenSettingCommand { get; }

        // 商品追加機能のコマンド
        public IRelayCommand IncreaseQuantityCommand { get; }
        public IRelayCommand DecreaseQuantityCommand { get; }
        public IRelayCommand AddProductCommand { get; }
        public IRelayCommand<ReportItem> DeleteItemCommand { get; }

        public ObservableCollection<ReportItem> SampleSales { get; } = new();
        public ObservableCollection<InvoiceItem> SampleInvoice { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();

        private FileSystemWatcher _configWatcher;

        public MainViewModel()
        {
            try
            {
                OpenSettingCommand = new RelayCommand(OpenSettingWindow);

                // 商品追加機能のコマンド初期化
                IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
                DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity, CanDecreaseQuantity);
                AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
                DeleteItemCommand = new RelayCommand<ReportItem>(DeleteItem);

                LoadConfiguration();

                // ダミーデータ
                var now = DateTime.Today;
                SampleSales.Add(new ReportItem { Date = now.AddDays(-1), Product = "コーヒー豆 200g", Quantity = 5, UnitPrice = 1200 });

                SampleInvoice.Add(new InvoiceItem { Description = "コーヒー豆 200g", Quantity = 5, UnitPrice = 1200 });

                GenerateSalesReportCommand = new RelayCommand(GenerateSalesReport);
                GenerateInvoicePdfCommand = new RelayCommand(GenerateInvoicePdf);
                OpenOutputFolderCommand = new RelayCommand(() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Path.GetFullPath(_outDir),
                    UseShellExecute = true
                }));
                SetupConfigurationWatcher();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初期化エラー");
                MessageBox.Show($"初期化でエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "初期化でエラーが発生しました";
            }
        }

        // 商品選択時の処理
        partial void OnSelectedProductChanged(Product? value)
        {
            if (value != null)
            {
                DisplayPrice = value.Price;
                AddProductCommand.NotifyCanExecuteChanged();
            }
            else
            {
                DisplayPrice = 0;
            }
        }

        // 数量変更時の処理
        partial void OnQuantityChanged(int value)
        {
            DecreaseQuantityCommand.NotifyCanExecuteChanged();
            AddProductCommand.NotifyCanExecuteChanged();
        }

        // 数量増加
        private void IncreaseQuantity()
        {
            Quantity++;
        }

        // 数量減少
        private void DecreaseQuantity()
        {
            if (Quantity > 1)
            {
                Quantity--;
            }
        }

        // 数量減少可能かチェック
        private bool CanDecreaseQuantity()
        {
            return Quantity > 1;
        }

        // 商品追加
        private void AddProduct()
        {
            if (SelectedProduct != null && Quantity > 0)
            {
                // SampleSalesに追加（DataGrid表示用）
                var newReportItem = new ReportItem
                {
                    Date = DateTime.Today,
                    Product = SelectedProduct.Name,
                    Quantity = Quantity,
                    UnitPrice = SelectedProduct.Price
                };
                SampleSales.Add(newReportItem);

                Status = $"商品を追加しました: {SelectedProduct.Name} x {Quantity}";

                // リセット
                Quantity = 1;
                SelectedProduct = null;
            }
        }

        // 商品追加可能かチェック
        private bool CanAddProduct()
        {
            return SelectedProduct != null && Quantity > 0;
        }

        // アイテム削除
        private void DeleteItem(ReportItem? item)
        {
            if (item == null) return;

            var result = MessageBox.Show(
                $"以下の項目を削除しますか？\n\n商品名: {item.Product}\n数量: {item.Quantity}\n単価: {item.UnitPrice:N0}円\n日付: {item.Date:yyyy/MM/dd}",
                "項目削除の確認",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                SampleSales.Remove(item);
                Status = $"項目を削除しました: {item.Product}";
            }
        }

        private void OpenSettingWindow()
        {
            var settingWindow = new SettingWindow();
            var settingViewModel = new SettingWindowViewModel();
            settingWindow.DataContext = settingViewModel;
            settingWindow.ShowDialog();
        }

        // 数値/文字列/百分率（"10%"）に柔軟対応
        static bool TryGetDecimalFlexible(JsonElement e, out decimal val)
        {
            if (e.ValueKind == JsonValueKind.Number)
            {
                if (e.TryGetDecimal(out val)) return true;
                if (e.TryGetDouble(out var d)) { val = (decimal)d; return true; }
            }
            else if (e.ValueKind == JsonValueKind.String)
            {
                var s = e.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    // "10%" → 0.10
                    if (s.EndsWith("%") &&
                        decimal.TryParse(s.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
                    { val = p / 100m; return true; }

                    if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                    { val = dec; return true; }
                }
            }
            val = default;
            return false;
        }

        private void GenerateSalesReport()
        {
            if (SampleSales == null || SampleSales.Count == 0)
            {
                // DataGridが空
                Status = "売上レポートを生成するには、売上データが必要です。";
            }
            else
            {
                try
                {
                    var path = _excel.GenerateSalesReport(SampleSales);
                    Status = $"Excel生成完了: {Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Excel生成エラー");
                    Status = "Excel生成でエラーが発生しました";
                }
            }
        }

        private void GenerateInvoicePdf()
        {
            if (SampleSales == null || SampleSales.Count == 0)
            {
                // DataGridが空
                Status = "請求書を生成するには、売上データが必要です。";
            }
            else
            {
                try
                {
                    // SampleSalesからSampleInvoiceを更新
                    UpdateInvoiceFromSales();

                    var path = _pdf.GenerateInvoice($"INV-{DateTime.Now:yyyyMMdd}", "Cat Coffee 御中", SampleInvoice);
                    Status = $"PDF生成完了: {Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PDF生成エラー");
                    Status = $"PDF生成でエラーが発生しました{ex.Message}";
                }
            }
        }

        // SampleSalesからSampleInvoiceを更新するメソッド
        private void UpdateInvoiceFromSales()
        {
            SampleInvoice.Clear();
            foreach (var salesItem in SampleSales)
            {
                var invoiceItem = new InvoiceItem
                {
                    Description = salesItem.Product,
                    Quantity = salesItem.Quantity,
                    UnitPrice = salesItem.UnitPrice
                };
                SampleInvoice.Add(invoiceItem);
            }
        }

        private void SetupConfigurationWatcher()
        {
            var configDir = AppContext.BaseDirectory;
            _configWatcher = new FileSystemWatcher(configDir, "appsettings.json");
            _configWatcher.Changed += (s, e) => LoadConfiguration();
            _configWatcher.EnableRaisingEvents = true;
        }

        private void LoadConfiguration()
        {
            try
            {
                Status = "設定を読み込み中...";

                // ❶ appsettings.json の絶対パス（exeと同じ場所）
                var cfgPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                string cfgText = File.Exists(cfgPath) ? File.ReadAllText(cfgPath) : "{}";

                // 既定値
                string outDir = "Output";
                string company = "Your Company";
                string issuer = "Issuer";
                decimal tax = 0.10m;

                using var doc = JsonDocument.Parse(cfgText);
                var root = doc.RootElement;

                // Output.BaseDirectory
                if (root.TryGetProperty("Output", out var output) &&
                    output.TryGetProperty("BaseDirectory", out var bd))
                {
                    outDir = bd.GetString() ?? outDir;
                }

                // Invoice.*
                if (root.TryGetProperty("Invoice", out var invoice))
                {
                    if (invoice.TryGetProperty("CompanyName", out var cn))
                        company = cn.GetString() ?? company;

                    if (invoice.TryGetProperty("IssuerName", out var iname))
                        issuer = iname.GetString() ?? issuer;

                    if (invoice.TryGetProperty("TaxRate", out var tr) &&
                        TryGetDecimalFlexible(tr, out var taxVal))
                        tax = taxVal;
                }

                // Products 配列の読み込み - products配列が存在する場合のみ更新
                if (root.TryGetProperty("products", out var productsArray) &&
                    productsArray.ValueKind == JsonValueKind.Array)
                {
                    var tempProducts = new List<Product>();
                    foreach (var productElement in productsArray.EnumerateArray())
                    {
                        string name = "";
                        decimal price = 0;

                        if (productElement.TryGetProperty("name", out var nameElement))
                            name = nameElement.GetString() ?? "";

                        if (productElement.TryGetProperty("price", out var priceElement) &&
                            TryGetDecimalFlexible(priceElement, out var priceVal))
                            price = priceVal;

                        if (!string.IsNullOrEmpty(name))
                        {
                            tempProducts.Add(new Product { Name = name, Price = price });
                        }
                    }

                    // products配列が存在する場合のみ、UIスレッドでコレクションを更新
                    if (Application.Current?.Dispatcher?.CheckAccess() == true)
                    {
                        UpdateProductsCollection(tempProducts);
                    }
                    else
                    {
                        Application.Current?.Dispatcher?.Invoke(() => UpdateProductsCollection(tempProducts));
                    }
                }
                // products配列が存在しない場合は、既存のProductsをそのまま保持

                // 必要に応じてフィールドへ反映
                _outDir = outDir;
                Directory.CreateDirectory(_outDir);
                LogService.Initialize(_outDir);

                // サービスを再初期化
                _excel = new ExcelService(_outDir);
                _pdf = new PdfService(_outDir, company, issuer, tax);

                SetStatusSafe("設定読み込み完了");
                Log.Information("設定読み込み完了: OutDir={OutDir}, Company={Company}, Issuer={Issuer}, Tax={Tax}",
                    outDir, company, issuer, tax);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "設定読み込みエラー");
                SetStatusSafe($"設定読み込みエラー: {ex.Message}");
            }
        }

        // コレクション更新を専用メソッドに分離
        private void UpdateProductsCollection(List<Product> newProducts)
        {
            Products.Clear();
            foreach (var product in newProducts)
            {
                Products.Add(product);
            }
        }

        public void Dispose()
        {
            try
            {
                _configWatcher?.Dispose();
                _configWatcher = null;
                Serilog.Log.Information("ViewModel disposed.");
            }
            catch { /* 終了時なので握りつぶしてOK */ }
        }

        // Closingからステータス更新を呼ぶための小ユーティリティ（UIスレッド保証）
        public void SetStatusSafe(string text)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                Status = text;
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(() => Status = text);
            }
        }
    }

    // Product クラス
    public class Product
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }
}