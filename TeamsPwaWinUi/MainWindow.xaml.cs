using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TeamsPwaWinUi
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        const string TeamsUrl = "https://teams.cloud.microsoft/";
        const string OutlookUrl = "https://outlook.cloud.microsoft/";
        public MainWindow()
        {
            InitializeComponent();
            MyWebView.Loaded += MyWebView_Loaded;
        }

        private async void MyWebView_Loaded(object sender, RoutedEventArgs args)
        {
            MyWebView.Loaded -= MyWebView_Loaded;
            // Enable Windows-connected accounts in Microsoft's web sign-in page.
            var options = new CoreWebView2EnvironmentOptions
            {
                AllowSingleSignOnUsingOSPrimaryAccount = true
            };
            var environment = await CoreWebView2Environment.CreateWithOptionsAsync(
                null, null, options);
            await MyWebView.EnsureCoreWebView2Async(environment);
            var cw2 = MyWebView.CoreWebView2;
            await Task.WhenAll(new[]
            {
                CoreWebView2PermissionKind.Notifications,
                CoreWebView2PermissionKind.WindowManagement,
                CoreWebView2PermissionKind.Microphone,
                CoreWebView2PermissionKind.Camera
            }.SelectMany(permissionKind => new[] { TeamsUrl, OutlookUrl }
                .Select(url => cw2.Profile.SetPermissionStateAsync(
                    permissionKind,
                    url,
                    CoreWebView2PermissionState.Allow).AsTask())));
            cw2.FaviconChanged += CoreWebView2_FaviconChanged;
            cw2.Navigate(TeamsUrl);
        }

        private async void CoreWebView2_FaviconChanged(CoreWebView2 sender, object args)
        {
            if (string.IsNullOrWhiteSpace(sender.FaviconUri))
            {
                return;
            }

            try
            {
                using var client = new HttpClient();
                var favicon = await client.GetByteArrayAsync(sender.FaviconUri);
                var faviconPath = Path.Combine(Path.GetTempPath(), "TeamsPwaWinUi-favicon.ico");
                await File.WriteAllBytesAsync(faviconPath, favicon);

                var windowHandle = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
                AppWindow.GetFromWindowId(windowId).SetIcon(faviconPath);
            }
            catch (HttpRequestException)
            {
            }
        }
    }
}
