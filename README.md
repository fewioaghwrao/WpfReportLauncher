# 📊 Excel／帳票ランチャー（C# / .NET 8 / WPF）

C# (.NET 8 WPF) 製の **Excel 売上レポート & PDF 請求書 自動生成ランチャー** です。  
画面上の売上一覧から、ワンクリックで **Excel レポート（ClosedXML）** と **PDF 請求書（QuestPDF）** を出力します。

> ✅ こちらはポートフォリオ用のデモアプリです。

---

## 🔗 詳細ドキュメント

スクリーンショット・仕様・設計・セットアップ手順などの詳細は、  
以下のポートフォリオページにまとめています。

👉https://fewioaghwrao.github.io/my-portfoliohogwhigrox/CCharp/docs/report-readme.html

---

## 🛠 主な機能

- DataGrid に入力した「商品・数量・単価」から **Excel 売上レポート（合計行付き）** を生成
- 同じ内容から **PDF 請求書** を自動生成  
  - 会社名・部署名・税率は `appsettings.json` から反映
- `appsettings.json` の変更を **FileSystemWatcher で自動検知 → アプリへ即時反映**
- 商品マスタは `appsettings.json` の `products` 配列から読み込み
- 行ダブルクリックで削除確認ダイアログ表示
- 出力先は `Output/` 配下に自動作成  
  - `Output/SalesReport/` … `SalesReport_yyyyMMdd_HHmmss.xlsx`  
  - `Output/Invoice/` …… `Invoice_INV-yyyyMMdd_HHmmss.pdf`  
  - `Output/Log/` ………… `launcher.log`（Serilog）

---

## 🔧 技術スタック

- .NET 8 / WPF（`net8.0-windows`）
- CommunityToolkit.MVVM（`ObservableObject`, `[ObservableProperty]`, `RelayCommand`）
- ClosedXML（Excel レポート生成）
- QuestPDF（PDF 請求書レイアウト）
- Serilog + Serilog.Sinks.File（構造化ログ & 日次ローテーション）
- FileSystemWatcher（設定ファイルのホットリロード）

---

## 🚀 ビルド & 実行（概要）

1. Visual Studio 2022 以降でソリューションを開く  
2. NuGet パッケージを復元  
3. `Release / x64` でビルド  
4. 出力フォルダの `WpfReportLauncher.exe` を実行

詳しい操作方法や `appsettings.json` のサンプルは、上記ポートフォリオページを参照してください。

---

## 📄 ライセンス

ポートフォリオ用途のサンプルアプリです。  
使用している各ライブラリ（ClosedXML / QuestPDF / Serilog / CommunityToolkit.Mvvm など）のライセンスに従ってご利用ください。
