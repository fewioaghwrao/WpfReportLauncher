using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfReportLauncher.ViewModels;
using WpfReportLauncher.Models;
using System;

namespace WpfReportLauncher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isShuttingDownConfirmed = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            // 既に確認済みならそのまま終了
            if (_isShuttingDownConfirmed) return;

            // ここで確認ダイアログ（モーダル）を出す
            e.Cancel = true; // いったんキャンセルしてから判断
            vm?.SetStatusSafe("終了確認中...");

            var dlg = new ConfirmExitWindow
            {
                Owner = this
            };
            var ok = dlg.ShowDialog() == true;
            if (!ok)
            {
                vm?.SetStatusSafe("終了をキャンセルしました");
                return; // 続行
            }

            // OK の場合のみ、本当に閉じる
            _isShuttingDownConfirmed = true;
            vm?.SetStatusSafe("終了処理中...");

            // e.Cancel=false にして戻ると、このClosingハンドラが再度呼ばれずに終了へ進む
            e.Cancel = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // DataGridの行がダブルクリックされた時の処理
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is ReportItem selectedItem)
            {
                var vm = DataContext as MainViewModel;
                vm?.DeleteItemCommand.Execute(selectedItem);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose(); // FileSystemWatcher等の後片付け
            base.OnClosed(e);
        }
    }
}