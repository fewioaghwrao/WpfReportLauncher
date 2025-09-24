using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfReportLauncher.Views;
using static WpfReportLauncher.Models.SettingItems;
using static WpfReportLauncher.Services.SettingService;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections;
using System.Windows;


namespace WpfReportLauncher.ViewModels
{
    internal partial class SettingWindowViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private AppSettings _currentSettings;

        [ObservableProperty]
        private string companyName = string.Empty;

        [ObservableProperty]
        private string issuerName = string.Empty;

        [ObservableProperty]
        private string taxRateText = string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingWindowViewModel(ISettingsService settingsService = null)
        {
            _settingsService = settingsService ?? new JsonSettingsService();

            SaveCommand = new AsyncRelayCommand(SaveSettingsAsync);
            CancelCommand = new RelayCommand(CancelSettings);

            // 初期値設定
            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                _currentSettings = await _settingsService.LoadSettingsAsync();

                // UIに反映
                CompanyName = _currentSettings.Invoice.CompanyName;
                IssuerName = _currentSettings.Invoice.IssuerName;
                TaxRateText = (_currentSettings.Invoice.TaxRate * 100).ToString("0.##"); // 0.10 → 10 に変換
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定読み込みエラー: {ex.Message}");
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                // バリデーション
                if (!ValidateInputs())
                {
                    MessageBox.Show("入力に誤りがあります。再度確認してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 設定を更新
                _currentSettings.Invoice.CompanyName = CompanyName;
                _currentSettings.Invoice.IssuerName = IssuerName;

                // 税率を数値に変換（10 → 0.10）
                if (double.TryParse(TaxRateText, out double taxRate))
                {
                    _currentSettings.Invoice.TaxRate = taxRate / 100;
                }

                // 保存
                await _settingsService.SaveSettingsAsync(_currentSettings);
                MessageBox.Show("設定を保存しました。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定保存エラー: {ex.Message}");
            }
        }

        private void CancelSettings()
        {
            // 変更を破棄して再読み込み
            _ = LoadSettingsAsync();
            CloseWindow();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                // エラーメッセージ表示
                return false;
            }

            if (string.IsNullOrWhiteSpace(IssuerName))
            {
                // エラーメッセージ表示
                return false;
            }

            if (!double.TryParse(TaxRateText, out double taxRate) || taxRate < 0 || taxRate > 100)
            {
                // エラーメッセージ表示
                return false;
            }

            return true;
        }

        private void CloseWindow()
        {
            foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
            {
                if (window is SettingWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
