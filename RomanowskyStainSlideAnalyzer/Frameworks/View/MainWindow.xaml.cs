using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.Helper;
using RomanowskyStainSlideAnalyzer.Home.View;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Frameworks.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private EnvironmentHelper environmentHelper = new();
        private static SemaphoreSlim dialogSemaphore = new SemaphoreSlim(1, 1);

        public MainWindow()
        {
            this.InitializeComponent();

            Init();
        }

        public static async Task<bool> ShowContentDialogAsync(string title, string content, string closeButtonText)
        {
            try
            {
                await dialogSemaphore.WaitAsync();

                var contentDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.window.Content.XamlRoot,
                };

                var result = await contentDialog.ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            finally
            {
                dialogSemaphore.Release();
            }
        }

        public static async Task<ContentDialogResult> ShowContentDialogAsync(string title, string content, string primaryButtonText, string secondaryButtonText, string closeButtonText)
        {
            try
            {
                await dialogSemaphore.WaitAsync();

                var contentDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = primaryButtonText,
                    SecondaryButtonText = secondaryButtonText,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.window.Content.XamlRoot,
                };

                var result = await contentDialog.ShowAsync();

                return result;
            }
            finally
            {
                dialogSemaphore.Release();
            }
        }

        public static async Task<bool> ShowContentDialogAsync(string title, string content, string primaryButtonText, string closeButtonText)
        {
            try
            {
                await dialogSemaphore.WaitAsync();

                var contentDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = primaryButtonText,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.window.Content.XamlRoot,
                };

                var result = await contentDialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            finally
            {
                dialogSemaphore.Release();
            }
        }

        public static async Task<StorageFile?> ShowSaveDialog(List<string> headers, List<string> types)
        {
            FileSavePicker savePicker = new();
            var window = App.window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;

            for(var i = 0; i < headers.Count(); i++)
            {
                savePicker.FileTypeChoices.Add(
                    headers[i], new List<string>() { types[i] }    
                );
            }

            StorageFile file = await savePicker.PickSaveFileAsync();

            return file;
        }

        private void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);
            ContentFrame.Navigated += OnNavigated;
            Navigate(typeof(HomeView), new EntranceNavigationTransitionInfo());

            var rssaFolder = @"C:\RomanowskyStainSlideAnalyzer";

            if (Directory.Exists(rssaFolder))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(rssaFolder);

                directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
            }

            var lastLaunchedVersion = environmentHelper.GetLastLaunchedVersion();

            if(lastLaunchedVersion != Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                environmentHelper.UpdateLastLaunchedVersion();

                ChangeLogView changeLogView = new();
                changeLogView.Activate();
            }
        }

        public void Navigate(
            Type navPageType,
            NavigationTransitionInfo transitionInfo
        )
        {
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            if (navPageType != null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, null, transitionInfo);
            }
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Navigate(typeof(SettingsView), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void OnDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            AppTitleBar.Margin = new Thickness()
            {
                Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
                Top = AppTitleBar.Margin.Top,
                Right = AppTitleBar.Margin.Right,
                Bottom = AppTitleBar.Margin.Bottom
            };
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingsView))
            {
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "Settings";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>()
                    .First(i => i.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()));

                NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }
    }
}
