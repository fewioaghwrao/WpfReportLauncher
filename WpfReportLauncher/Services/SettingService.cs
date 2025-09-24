using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static WpfReportLauncher.Models.SettingItems;

namespace WpfReportLauncher.Services
{
    internal class SettingService
    {
        public interface ISettingsService
        {
            Task<AppSettings> LoadSettingsAsync();
            Task SaveSettingsAsync(AppSettings settings);
        }

        public class JsonSettingsService : ISettingsService
        {
            private readonly string _settingsFilePath;

            public JsonSettingsService(string settingsFilePath = "appsettings.json")
            {
                _settingsFilePath = settingsFilePath;
            }

            public async Task<AppSettings> LoadSettingsAsync()
            {
                try
                {
                    if (!File.Exists(_settingsFilePath))
                    {
                        // デフォルト設定を作成
                        return CreateDefaultSettings();
                    }

                    string jsonString = await File.ReadAllTextAsync(_settingsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };

                    return JsonSerializer.Deserialize<AppSettings>(jsonString, options) ?? CreateDefaultSettings();
                }
                catch (Exception ex)
                {
                    // ログ出力やエラーハンドリング
                    Console.WriteLine($"設定ファイル読み込みエラー: {ex.Message}");
                    return CreateDefaultSettings();
                }
            }

            public async Task SaveSettingsAsync(AppSettings settings)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    string jsonString = JsonSerializer.Serialize(settings, options);
                    await File.WriteAllTextAsync(_settingsFilePath, jsonString);
                }
                catch (Exception ex)
                {
                    // ログ出力やエラーハンドリング
                    Console.WriteLine($"設定ファイル保存エラー: {ex.Message}");
                    throw;
                }
            }

            private AppSettings CreateDefaultSettings()
            {
                return new AppSettings
                {
                    Output = new OutputSettings
                    {
                        BaseDirectory = "Output"
                    },
                    Invoice = new InvoiceSettings
                    {
                        CompanyName = "会社名未設定",
                        IssuerName = "部署名未設定",
                        TaxRate = 0.10
                    }
                };
            }
        }
    }
}
