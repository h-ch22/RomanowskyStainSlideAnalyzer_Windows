using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.Home.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Labeling.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LabelingWindow : Window
    {
        private LabelingViewModel viewModel;
        private LabelingHelper helper;
        private List<BoundingBoxDataModel> BoundingBoxes;
        private string path;
        private ClassTypeModel classType = ClassTypeModel.TYPE_A;
        private bool IsEditMode = false;
        private bool IsDone = false;
        private int EditModeEndIndex = 0;

        public LabelingWindow(LabelingViewModel viewModel, string path)
        {
            this.InitializeComponent();

            this.viewModel = viewModel;
            this.path = path;
            helper = new(path);
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 950));
            Init();
        }
        private async void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            if (helper.GetFileAlreadyExists())
            {
                var contentDialog = new ContentDialog
                {
                    Title = "File Already Exist",
                    Content = $"{path.Split(".txt")[0]}.csv The file already exists.\nClick OK to overwrite the file and continue.\nClick Cancel to take further action, such as renaming or moving the file and trying again.",
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary,
                    XamlRoot = App.window.Content.XamlRoot
                };

                var result = await contentDialog.ShowAsync();

                if (result == ContentDialogResult.Secondary)
                {
                    Close();
                    return;
                }
            }

            await Task.Run(() => {
                try
                {
                    helper.CreateCSVFile();
                }
                catch (Exception ex)
                {
                    ShowAlert("Error", $"An error occurred while creating the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}");
                }

                BoundingBoxes = helper.GetBoundingBox();

                DispatcherQueue.TryEnqueue(() =>
                {
                    viewModel.AllIndex = BoundingBoxes.Count;
                    viewModel.CurrentIndex = 1;
                    viewModel.CurrentBoundingBox = $"X: {BoundingBoxes[viewModel.CurrentIndex - 1].X}, Y: {BoundingBoxes[viewModel.CurrentIndex - 1].Y}, W: {BoundingBoxes[viewModel.CurrentIndex - 1].Width}, H: {BoundingBoxes[viewModel.CurrentIndex - 1].Height}";

                    if (btn_hideBBox.IsChecked == false)
                    {
                        CreateBBox();
                    }

                    CreateThumbnailBBox();

                    img_scrollView.Loaded += (s, e) =>
                    {
                        scrollTo();
                    };
                });
            });
        }

        private void ShowAlert(string title, string message, bool exit=true)
        {
            var contentDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = App.window.Content.XamlRoot
            };

            if(exit)
            {
                contentDialog.PrimaryButtonClick += (_s, _e) => {
                    this.Close();
                };
            }

            contentDialog.ShowAsync();
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "btn_next":
                    if(!IsDone)
                    {
                        if (IsEditMode)
                        {
                            try
                            {
                                helper.ChangeLine(
                                (int)classType,
                                BoundingBoxes[viewModel.CurrentIndex - 1].X.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Y.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Width.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Height.ToString(),
                                viewModel.CurrentIndex
                            );
                            }
                            catch (Exception ex)
                            {
                                ShowAlert("Error", $"An error occurred while writing the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}");
                            }
                        }

                        else
                        {
                            try
                            {
                                helper.AppendText(
                                (int)classType,
                                BoundingBoxes[viewModel.CurrentIndex - 1].X.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Y.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Width.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Height.ToString()
                            );
                            }
                            catch (Exception ex)
                            {
                                ShowAlert("Error", $"An error occurred while writing the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}");
                            }
                        }
                    }

                    if (viewModel.CurrentIndex < viewModel.AllIndex)
                    {
                        viewModel.CurrentIndex += 1;

                        if (viewModel.CurrentIndex == EditModeEndIndex) IsEditMode = false;

                        viewModel.CurrentBoundingBox = $"X: {BoundingBoxes[viewModel.CurrentIndex - 1].X}, Y: {BoundingBoxes[viewModel.CurrentIndex - 1].Y}, W: {BoundingBoxes[viewModel.CurrentIndex - 1].Width}, H: {BoundingBoxes[viewModel.CurrentIndex - 1].Height}";
                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;

                        if (viewModel.CurrentIndex >= viewModel.AllIndex)
                        {
                            btn_next.Content = "Done";
                        }

                        if (btn_hideBBox.IsChecked == false)
                        {
                            CreateBBox();
                        }

                        scrollTo();
                        CreateThumbnailBBox();
                    }

                    else
                    {
                        IsDone = true;
                        var folderPicker = new FolderPicker();
                        var window = App.window;
                        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                        folderPicker.ViewMode = PickerViewMode.Thumbnail;
                        folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                        var folder = await folderPicker.PickSingleFolderAsync();

                        if (folder != null)
                        {
                            try
                            {
                                helper.Copy(folder.Path);
                                this.Close();
                            }
                            catch(Exception ex)
                            {
                                ShowAlert("Error", $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}", false);
                            }
                        }

                    }

                    break;

                case "btn_previous":
                    IsDone = false;

                    if(viewModel.CurrentIndex > 1)
                    {
                        if(!IsEditMode || EditModeEndIndex < viewModel.CurrentIndex)
                        {
                            EditModeEndIndex = viewModel.CurrentIndex;
                        }
                        viewModel.CurrentIndex -= 1;
                        viewModel.CurrentBoundingBox = $"X: {BoundingBoxes[viewModel.CurrentIndex - 1].X}, Y: {BoundingBoxes[viewModel.CurrentIndex - 1].Y}, W: {BoundingBoxes[viewModel.CurrentIndex - 1].Width}, H: {BoundingBoxes[viewModel.CurrentIndex - 1].Height}";

                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;
                        IsEditMode = true;

                        if (btn_hideBBox.IsChecked == false)
                        {
                            CreateBBox();
                        }

                        CreateThumbnailBBox();
                        scrollTo();
                    }

                    break;
            }
        }

        private void scrollTo()
        {
            if (viewModel.IsZoomModeEnabled && img_scrollView.ZoomFactor <= 1F)
            {
                img_scrollView.ZoomTo(3F, new((float) BoundingBoxes[viewModel.CurrentIndex - 1].X, (float) BoundingBoxes[viewModel.CurrentIndex - 1].Y));
            }
            else if(viewModel.IsZoomModeEnabled)
            {
                double zoomFactor = img_scrollView.ZoomFactor;

                double viewportWidth = img_scrollView.ViewportWidth;
                double viewportHeight = img_scrollView.ViewportHeight;

                double scrollOffsetX = BoundingBoxes[viewModel.CurrentIndex - 1].X * zoomFactor - (viewportWidth / 2);
                double scrollOffsetY = BoundingBoxes[viewModel.CurrentIndex - 1].Y * zoomFactor - (viewportHeight / 2);

                img_scrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
            }
        }

        private void OnClassSelected(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Name)
            {
                case "radio_A":
                    classType = ClassTypeModel.TYPE_A;
                    break;

                case "radio_B":
                    classType = ClassTypeModel.TYPE_B;
                    break;

                case "radio_C":
                    classType = ClassTypeModel.TYPE_C;
                    break;

                default: break;
            }
        }

        private void CreateBBox()
        {
            foreach(var child in canvas.Children)
            {
                if(child.GetType() == typeof(Rectangle))
                {
                    canvas.Children.Remove(child);
                }
            }

            Rectangle bBox = new()
            {
                Width = BoundingBoxes[viewModel.CurrentIndex - 1].Width,
                Height = BoundingBoxes[viewModel.CurrentIndex - 1].Height,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            canvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, BoundingBoxes[viewModel.CurrentIndex - 1].X);
            bBox.SetValue(Canvas.TopProperty, BoundingBoxes[viewModel.CurrentIndex - 1].Y);
        }

        private void CreateThumbnailBBox()
        {
            foreach (var child in thumbnailCanvas.Children)
            {
                if (child.GetType() == typeof(Rectangle) && (child as Rectangle).Name != "ViewPortRect")
                {
                    thumbnailCanvas.Children.Remove(child);
                }
            }

            Rectangle bBox = new()
            {
                Width = (BoundingBoxes[viewModel.CurrentIndex - 1].Width) * 0.25,
                Height = (BoundingBoxes[viewModel.CurrentIndex - 1].Height) * 0.25,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 66, 66)),
                StrokeThickness = 2
            };

            thumbnailCanvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, (BoundingBoxes[viewModel.CurrentIndex - 1].X) * 0.25);
            bBox.SetValue(Canvas.TopProperty, (BoundingBoxes[viewModel.CurrentIndex - 1].Y) * 0.25);
        }

        private void img_scrollView_ViewChanged(ScrollView sender, object args)
        {
            if (sender != null)
            {
                double zoomFactor = sender.ZoomFactor;
                double scale = 128 / (512 * zoomFactor);

                double offsetX = sender.HorizontalOffset * scale;
                double offsetY = sender.VerticalOffset * scale;
                double viewPortW = sender.ViewportWidth * scale;
                double viewPortH = sender.ViewportHeight * scale;

                foreach (var child in thumbnailCanvas.Children)
                {
                    if (child.GetType() == typeof(Rectangle) && (child as Rectangle).Name == "ViewPortRect")
                    {
                        thumbnailCanvas.Children.Remove(child);
                    }
                }

                Rectangle viewPortBox = new()
                {
                    Width = viewPortW,
                    Height = viewPortH,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 52, 119, 235)),
                    StrokeThickness = 2
                };

                viewPortBox.Name = "ViewPortRect";

                thumbnailCanvas.Children.Add(viewPortBox);
                viewPortBox.SetValue(Canvas.LeftProperty, offsetX);
                viewPortBox.SetValue(Canvas.TopProperty, offsetY);
            }
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if(btn_hideBBox.IsChecked == true)
            {
                foreach (var child in canvas.Children)
                {
                    if (child.GetType() == typeof(Rectangle))
                    {
                        canvas.Children.Remove(child);
                    }
                }
            }
            else
            {
                CreateBBox();
            }
        }
    }
}
