using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfReportLauncher.ViewModels;
using WpfReportLauncher.Views;

namespace WpfReportLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // あなたの利用条件に合わせて設定
            QuestPDF.Settings.License = LicenseType.Community;  // ★ 追加（必須）

            base.OnStartup(e);
        }
    }

}
