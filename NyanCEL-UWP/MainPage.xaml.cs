// Copyright (c) 2024 Toshiki Iga
//
// Released under the MIT license
// https://opensource.org/license/mit

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using EmbedIO;
using EmbedIO.Authentication;
using EmbedIO.WebApi;
using Microsoft.Web.WebView2.Core;
using Nyan;
using NyanCEL;

namespace NyanCELUWP
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private WebServer _server;
        private ExtendedExecutionSession _session;

        public MainPage()
        {
            Nyan.NyanLog.Info("NyanCEL-UWP MainPage: Begin.");

            {
                // Toast
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                var toastTextElements = toastXml.GetElementsByTagName("text");
                toastTextElements[0].AppendChild(toastXml.CreateTextNode("NyanCEL は Excel ブック (.xlsx) を SQL で検索できるようにするサーバーツールです。"));
                toastTextElements[1].AppendChild(toastXml.CreateTextNode("サーバー機能のコンテンツとして利用するExcelファイル(.xlsx)を選択してください。"));
                toastTextElements[1].AppendChild(toastXml.CreateTextNode("※ドキュメントフォルダ、またはリムーバブルディスクにあるファイルが選択可能です。"));
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }

            this.InitializeComponent();
            this.InitializeAsync();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
            LoadExcel();
            StartRestServer();
        }

        private async void InitializeAsync()
        {
            try
            {
                await MyWebView2.EnsureCoreWebView2Async();
                MyWebView2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                MyWebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                MyWebView2.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                MyWebView2.CoreWebView2.Settings.AreDevToolsEnabled = false;

                MyWebView2.CoreWebView2.WebMessageReceived += HandleWebMessageReceived;

                Uri fileUri = new Uri("ms-appx:///Assets/MainPage.html");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
                var mainPageHtml = await FileIO.ReadTextAsync(file);
                MyWebView2.CoreWebView2.NavigateToString(mainPageHtml);
            }
            catch (Exception ex)
            {
                NyanLog.Error("Unexpected: " + ex.Message);
            }
        }

        private void HandleWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            NyanLog.Debug("clicked: " + message);
            // ShowVersionDialog();
        }

        private async void ShowVersionDialog()
        {
            ContentDialogResult result = await VersionDialog.ShowAsync();
        }

        public async Task<MemoryStream> ConvertStorageFileToMemoryStream(StorageFile storageFile)
        {
            // StorageFile をバッファーに読み込む
            var buffer = await FileIO.ReadBufferAsync(storageFile);

            // バッファーからメモリーストリームに書き込む
            var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);

            // メモリーストリームの位置を先頭に戻す
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        // Sqlite

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            StopRestServer();
        }

        /////////////////////////////////////////////////////////////////////////////////////
        // EmbedIO

        private void StartRestServer()
        {
            Nyan.NyanLog.Info("NyanCEL-UWP REST Server: Begin: EmbedIO");
            _server = new WebServer(o => o
                .WithUrlPrefix("http://*:28096/")
                .WithMode(HttpListenerMode.EmbedIO))
                .WithModule(new BasicAuthenticationModule("/api")
                .WithAccount("nyan", "cel"))
                .WithWebApi("/api", m => m.WithController<ApiController>());
            _server.RunAsync();
            RequestExtendedExecution();
        }

        // 拡張実行をリクエスト。
        private async void RequestExtendedExecution()
        {
            _session = new ExtendedExecutionSession
            {
                Reason = ExtendedExecutionReason.Unspecified,
                Description = "Running REST server in the background"
            };
            _session.Revoked += Session_Revoked;

            ExtendedExecutionResult result = await _session.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Denied)
            {
                // 拒否された
                NyanLog.Warn("ExtendedExecution Denied");
            }
        }

        private void Session_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            _session.Dispose();
            _session = null;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _server?.Dispose();
            _session?.Dispose();
            base.OnNavigatedFrom(e);
        }

        private void StopRestServer()
        {
            Nyan.NyanLog.Debug("NyanCEL-UWP REST Server: End.");
            _server?.Dispose();
        }

        private async void LoadExcel()
        {
            var excelFile = await PickExcelFileAsync();
            if (excelFile != null)
            {
                NyanLog.Info("NyanCEL-UWP Load Excel file: Begin: ClosedXML");
                // ClosedXML
                List<NyanTableInfo> tableInfoList = await NyanXlsx2Sqlite.LoadExcelFile(await NyanCELUtil.GetXlsxDatabaseInstance(), await ConvertStorageFileToMemoryStream(excelFile));

                // Toast
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                var toastTextElements = toastXml.GetElementsByTagName("text");
                toastTextElements[0].AppendChild(toastXml.CreateTextNode("指定したExcelファイルの内容が NyanCEL-UWP の REST server で利用可能になりました"));
                toastTextElements[1].AppendChild(toastXml.CreateTextNode("REST API に SQL 文を渡して、検索結果を JSON で取得できます。"));
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }

        private async Task<StorageFile> PickExcelFileAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".xlsx");

            StorageFile file = await picker.PickSingleFileAsync();
            return file;
        }
    }
}
