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
using RomanowskyStainSlideAnalyzer.Analyze.Helper;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using RomanowskyStainSlideAnalyzer.Home.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private List<int> labeledDatas = new();
        private List<int> labeledMaskDatas = new();
        private BoundingBoxDataModel currentData;
        private int[,] currentMask;

        private string path;
        private string root;
        private string inputFile;

        private ClassTypeModel classType = ClassTypeModel.TYPE_A;
        private bool IsEditMode = false;
        private bool IsDone = false;
        private int EditModeEndIndex = 0;
        private bool isMaskAvailable;
        private bool isBBoxAvailable;

        public LabelingWindow(LabelingViewModel viewModel, string path, bool isMaskAvailable, bool isBBoxAvailable)
        {
            this.InitializeComponent();

            this.viewModel = viewModel;
            this.isMaskAvailable = isMaskAvailable;
            this.isBBoxAvailable = isBBoxAvailable;

            if (!isMaskAvailable || !isBBoxAvailable)
            {
                if (isMaskAvailable) this.viewModel.UseBBoxAsTarget = false;
                else this.viewModel.UseBBoxAsTarget = true;
            }

            this.path = path;
            var pathSplit = path.Split(@"\");
            root = pathSplit[pathSplit.Length - 2];
            inputFile = pathSplit[pathSplit.Length - 1].Split(".txt")[0];

            helper = new(path);
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32((isMaskAvailable && isBBoxAvailable) ? 1100 : 800, 950));
            Init();
        }
        private async void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            img_maskScrollView.Visibility = (isMaskAvailable && isBBoxAvailable) ? Visibility.Visible : Visibility.Collapsed;

            if (helper.GetFileAlreadyExists())
            {
                var isContinue = await MainWindow.ShowContentDialogAsync(
                    "File Already Exists",
                    $"{path.Split(".txt")[0]}.csv The file already exists.\nClick OK to overwrite the file and continue.\nClick Cancel to take further action, such as renaming or moving the file and trying again.",
                    "OK",
                    "Cancel"
                );

                if (!isContinue)
                {
                    Close();
                    return;
                }
            }

            try
            {
                helper.CreateCSVFile();
            }
            catch (Exception ex)
            {
                await MainWindow.ShowContentDialogAsync(
                    "Error",
                    $"An error occurred while creating the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}",
                    "OK"
                );
            }

            await LoadData();
            scrollTo();
        }

        private void WriteLabelingData()
        {
            if (IsEditMode)
            {
                try
                {
                    if (isBBoxAvailable && viewModel.CurrentIndex <= BoundingBoxes.Count) labeledDatas[viewModel.CurrentIndex - 1] = (int)classType;
                    if (isMaskAvailable && viewModel.CurrentIndex <= AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}")) labeledMaskDatas[viewModel.CurrentIndex - 1] = (int)classType;
                }
                catch (Exception ex)
                {
                    MainWindow.ShowContentDialogAsync(
                        "Error",
                        $"An error occurred while writing the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}",
                        "OK"
                    );
                }
            }

            else
            {
                try
                {
                    if (isBBoxAvailable && viewModel.CurrentIndex <= BoundingBoxes.Count) labeledDatas.Add((int)classType);
                    if (isMaskAvailable && viewModel.CurrentIndex <= AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}")) labeledMaskDatas.Add((int)classType);
                }
                catch (Exception ex)
                {
                    MainWindow.ShowContentDialogAsync(
                        "Error",
                         $"An error occurred while writing the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}",
                         "OK"
                    );
                }
            }
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "btn_next":
                    if (!IsDone)
                    {
                        WriteLabelingData();
                    }

                    if (viewModel.CurrentIndex < viewModel.AllIndex)
                    {
                        viewModel.CurrentIndex += 1;

                        if (viewModel.CurrentIndex == EditModeEndIndex) IsEditMode = false;

                        if (BoundingBoxes == null || BoundingBoxes.Count < viewModel.AllIndex)
                        {
                            if(viewModel.CurrentIndex <= BoundingBoxes.Count)
                            {
                                if (BoundingBoxes == null) BoundingBoxes = new();

                                if (IsEditMode)
                                {
                                    BoundingBoxes[viewModel.CurrentIndex - 1] = new(currentData.X, currentData.Y, currentData.Width, currentData.Height);
                                }

                                else
                                {
                                    BoundingBoxes.Add(new(currentData.X, currentData.Y, currentData.Width, currentData.Height));
                                }
                            }

                        }

                        if (IsEditMode)
                        {
                            switch (isBBoxAvailable ? labeledDatas[viewModel.CurrentIndex - 1] : labeledMaskDatas[viewModel.CurrentIndex - 1])
                            {
                                case 0:
                                    radio_A.IsChecked = true;
                                    break;

                                case 1:
                                    radio_B.IsChecked = true;
                                    break;

                                case 2:
                                    radio_C.IsChecked = true;
                                    break;

                                default: break;
                            }
                        }

                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";
                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;

                        if (viewModel.CurrentIndex >= viewModel.AllIndex)
                        {
                            btn_next.Content = "Done";
                        }

                        LoadData();

                        if (btn_hideBBox.IsChecked == false)
                        {
                            if (isBBoxAvailable) CreateBBox();
                            if (isMaskAvailable) DrawContours();
                        }

                        scrollTo();
                        CreateThumbnailBBox();
                    }

                    else
                    {
                        IsDone = true;

                        try
                        {
                            if (isBBoxAvailable)
                            {
                                helper.CreateLabelingData(labeledDatas, BoundingBoxes);
                            }

                            if (isMaskAvailable)
                            {
                                LabelingHelper.CreateMaskLabelingData(
                                    $@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}",
                                    true,
                                    labeledMaskDatas
                                );

                                LabelingHelper.CreateMaskLabelingData(
                                    $@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}",
                                    false,
                                    labeledMaskDatas
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            MainWindow.ShowContentDialogAsync(
                                "Error",
                                 $"An error occurred while writing the file.\nCheck if another process is using the file, or re-run the software.\nError: {ex.Message}",
                                 "OK"
                            );
                        }

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
                                if (helper.IsDestinationFileExists(folder.Path))
                                {
                                    var splitPath = path.Split(@"\");
                                    var fileName = splitPath[splitPath.Length - 1];

                                    var dialogResult = await MainWindow.ShowContentDialogAsync(
                                        "File Already Exists",
                                        $@"The file {folder.Path}\{fileName.Split(@".txt")[0]}.csv already exists.\nDo you want to overwrite it?",
                                        "Yes",
                                        "No"
                                    );

                                    if (dialogResult)
                                    {
                                        helper.Copy(folder.Path);
                                    }
                                }

                                else
                                {
                                    helper.Copy(folder.Path);
                                }
                            }
                            catch (Exception ex)
                            {
                                await MainWindow.ShowContentDialogAsync(
                                    "Error",
                                    $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}",
                                    "OK"
                                );

                                return;
                            }
                        }

                        if (isMaskAvailable)
                        {
                            folder = null;

                            var result = await MainWindow.ShowContentDialogAsync(
                                "Export Data",
                                "How do you want to export the labeled mask data?",
                                "One file",
                                "Multiple files",
                                "Skip"
                            );

                            if (result == ContentDialogResult.Primary || result == ContentDialogResult.Secondary)
                            {
                                folder = await folderPicker.PickSingleFolderAsync();

                                if (folder != null)
                                {
                                    try
                                    {
                                        LabelingHelper.Copy($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}", folder.Path, result == ContentDialogResult.Primary);
                                        LabelingHelper.writePythonFile(result == ContentDialogResult.Primary, folder.Path);

                                        await MainWindow.ShowContentDialogAsync(
                                            "Training Information",
                                            $"Copied the file(s) to {folder.Path}.\nTo train this file(s), use the code inside the main.py file.",
                                            "OK"
                                        );

                                        Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        await MainWindow.ShowContentDialogAsync(
                                            "Error",
                                            $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}",
                                            "OK"
                                        );
                                    }
                                }
                            }

                            else
                            {
                                Close();
                            }
                        }
                        else Close();
                    }

                    break;

                case "btn_previous":
                    IsDone = false;
                    btn_next.Content = "Next";

                    if (viewModel.CurrentIndex > 1)
                    {
                        if (!IsEditMode || EditModeEndIndex < viewModel.CurrentIndex)
                        {
                            EditModeEndIndex = viewModel.CurrentIndex;
                        }

                        viewModel.CurrentIndex -= 1;
                        LoadData();
                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";

                        switch ((isBBoxAvailable && viewModel.CurrentIndex <= BoundingBoxes.Count) ? labeledDatas[viewModel.CurrentIndex - 1] : labeledMaskDatas[viewModel.CurrentIndex - 1])
                        {
                            case 0:
                                radio_A.IsChecked = true;
                                break;

                            case 1:
                                radio_B.IsChecked = true;
                                break;

                            case 2:
                                radio_C.IsChecked = true;
                                break;
                        }

                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;
                        IsEditMode = true;

                        if (btn_hideBBox.IsChecked == false)
                        {
                            CreateBBox();
                            DrawContours();
                        }

                        CreateThumbnailBBox();
                        scrollTo();
                    }

                    break;
            }
        }

        private void scrollTo()
        {
            if (viewModel.IsZoomModeEnabled && (isBBoxAvailable && img_scrollView.ZoomFactor <= 1F))
            {
                img_scrollView.ZoomTo(3F, new((float)currentData.X, (float)currentData.Y));
            }

            else if (viewModel.IsZoomModeEnabled)
            {
                double zoomFactor = img_scrollView.ZoomFactor;

                double viewportWidth = img_scrollView.ViewportWidth;
                double viewportHeight = img_scrollView.ViewportHeight;

                double scrollOffsetX = currentData.X * zoomFactor - (viewportWidth / 2);
                double scrollOffsetY = currentData.Y * zoomFactor - (viewportHeight / 2);

                img_scrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
            }

            if (viewModel.IsZoomModeEnabled && (isMaskAvailable && img_maskScrollView.ZoomFactor <= 1F))
            {
                img_maskScrollView.ZoomTo(3F, new((float)currentData.X, (float)currentData.Y));
            }

            else if (viewModel.IsZoomModeEnabled)
            {
                double maskZoomFactor = img_maskScrollView.ZoomFactor;

                double viewportWidth = img_maskScrollView.ViewportWidth;
                double viewportHeight = img_maskScrollView.ViewportHeight;

                double maskOffsetX = currentData.X * maskZoomFactor - (viewportWidth / 2);
                double maskOffsetY = currentData.Y * maskZoomFactor - (viewportHeight / 2);

                img_maskScrollView.ScrollTo(maskOffsetX, maskOffsetY);
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
            DeleteBBoxes();

            if(viewModel.CurrentIndex <= BoundingBoxes.Count)
            {
                noDataPanel.Visibility = Visibility.Collapsed;
                img_scrollView.Visibility = Visibility.Visible;

                Rectangle bBox = new()
                {
                    Width = currentData.Width,
                    Height = currentData.Height,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 219, 66, 66))
                };

                canvas.Children.Add(bBox);
                bBox.SetValue(Canvas.LeftProperty, currentData.X);
                bBox.SetValue(Canvas.TopProperty, currentData.Y);
            }
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
                Width = (currentData.Width) * 0.25,
                Height = (currentData.Height) * 0.25,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 219, 66, 66)),
                StrokeThickness = 2
            };

            thumbnailCanvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, (currentData.X) * 0.25);
            bBox.SetValue(Canvas.TopProperty, (currentData.Y) * 0.25);
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

        private void DeleteMasks()
        {
            DeleteContours();
            DeleteBBoxes();
        }

        private void DeleteBBoxes()
        {
            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Rectangle))
                {
                    canvas.Children.Remove(child);
                }
            }
        }

        private void DeleteContours()
        {
            var toRemove = maskCanvas.Children.OfType<Rectangle>()
                  .Where(rect => rect.Name == "contour")
                  .ToList();

            foreach (var rect in toRemove)
            {
                maskCanvas.Children.Remove(rect);
            }
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btn_hideBBox.IsChecked == true)
            {
                DeleteMasks();
            }
            else
            {
                CreateBBox();
                DrawContours();
            }
        }

        private void DrawContours()
        {
            if(viewModel.CurrentIndex <= AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}"))
            {
                DeleteContours();

                for (int y = 1; y < 511; y++)
                {
                    for (int x = 1; x < 511; x++)
                    {
                        if (currentMask[y, x] == 1 && IsContour(currentMask, x, y))
                        {
                            DrawContourPixel(x, y);
                        }
                    }
                }
            }

        }

        private bool IsContour(int[,] data, int x, int y)
        {
            return data[y - 1, x] == 0 || data[y + 1, x] == 0 ||
                   data[y, x - 1] == 0 || data[y, x + 1] == 0;
        }

        private void DrawContourPixel(int x, int y)
        {
            var rect = new Rectangle
            {
                Width = 1,
                Height = 1,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 219, 66, 66))
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.Name = "contour";

            maskCanvas.Children.Add(rect);
        }

        private async Task LoadData()
        {
            DeleteMasks();

            await Task.Run(() => {
                if (BoundingBoxes == null || BoundingBoxes.Count < viewModel.AllIndex)
                {
                    BoundingBoxes = helper.GetBoundingBox();

                    if (BoundingBoxes.Count == 0)
                    {
                        BoundingBoxes = new();
                    }
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    if ((BoundingBoxes.Count >= viewModel.CurrentIndex) && isBBoxAvailable)
                    {
                        viewModel.AllIndex = BoundingBoxes.Count;
                        currentData = BoundingBoxes[viewModel.CurrentIndex - 1];
                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";

                        if (btn_hideBBox.IsChecked == false)
                        {
                            CreateBBox();
                        }

                        CreateThumbnailBBox();
                        scrollTo();
                    }

                    else
                    {
                        img_scrollView.Visibility = Visibility.Collapsed;
                        noDataPanel.Visibility = Visibility.Visible;
                    }
                });
            });

            if (isMaskAvailable)
            {
                await Task.Run(() =>
                {
                    var data = AnalyzeHelper.GetMaskData($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}", (viewModel.CurrentIndex - 1).ToString());
                    var coords = AnalyzeHelper.CalculateMaskSize(data);

                    currentMask = AnalyzeHelper.GetMask($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}", (viewModel.CurrentIndex - 1).ToString());
                    currentData = new(coords.Item3, coords.Item4, coords.Item1, coords.Item2);

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        viewModel.AllIndex = BoundingBoxes.Count > AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}") ? BoundingBoxes.Count : AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}");
                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";

                        if (btn_hideBBox.IsChecked == false && viewModel.CurrentIndex <= viewModel.AllIndex)
                        {
                            DrawContours();
                        }
                        else if (viewModel.CurrentIndex > viewModel.AllIndex)
                        {
                            img_maskScrollView.Visibility = Visibility.Collapsed;
                            noDataPanel.Visibility = Visibility.Visible;
                        }

                        CreateThumbnailBBox();

                        scrollTo();
                    });
                });
            }
        }
    }
}
