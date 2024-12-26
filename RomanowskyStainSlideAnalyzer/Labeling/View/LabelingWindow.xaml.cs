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

            if(isMaskAvailable && isBBoxAvailable)
            {
                this.viewModel.EnableUseBBoxButton = true;
            } else
            {
                this.viewModel.EnableUseBBoxButton = false;

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
        }

        private void WriteLabelingData()
        {
            if (IsEditMode)
            {
                try
                {
                    helper.ChangeLine(
                        (int)classType,
                        currentData.X.ToString(),
                        currentData.Y.ToString(),
                        currentData.Width.ToString(),
                        currentData.Height.ToString(),
                        viewModel.CurrentIndex
                    );

                    labeledDatas[viewModel.CurrentIndex - 1] = (int)classType;
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
                    helper.AppendText(
                        (int)classType,
                        currentData.X.ToString(),
                        currentData.Y.ToString(),
                        currentData.Width.ToString(),
                        currentData.Height.ToString()
                    );

                    labeledDatas.Add((int)classType);
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
            switch((sender as Button).Name)
            {
                case "btn_next":
                    if(!IsDone)
                    {
                        WriteLabelingData();
                    }

                    if (viewModel.CurrentIndex < viewModel.AllIndex)
                    {
                        viewModel.CurrentIndex += 1;

                        if (viewModel.CurrentIndex == EditModeEndIndex) IsEditMode = false;

                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";
                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;

                        if (viewModel.CurrentIndex >= viewModel.AllIndex)
                        {
                            btn_next.Content = "Done";
                        }

                        LoadData();

                        if (btn_hideBBox.IsChecked == false)
                        {
                            if (viewModel.UseBBoxAsTarget) CreateBBox();
                            else DrawContours();
                        }

                        scrollTo();
                        CreateThumbnailBBox();
                    }

                    else
                    {
                        IsDone = true;

                        if(isMaskAvailable)
                        {
                            LabelingHelper.CreateMaskLabelingData(
                                $@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}",
                                true,
                                labeledDatas
                            );

                            LabelingHelper.CreateMaskLabelingData(
                                $@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}",
                                false,
                                labeledDatas
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
                                if(helper.IsDestinationFileExists(folder.Path))
                                {
                                    var splitPath = path.Split(@"\");
                                    var fileName = splitPath[splitPath.Length - 1];

                                    var dialogResult = await MainWindow.ShowContentDialogAsync(
                                        "File Already Exists",
                                        $@"The file {folder.Path}\{fileName.Split(@".txt")[0]}.csv already exists.\nDo you want to overwrite it?",
                                        "Yes",
                                        "No"
                                    );

                                    if(dialogResult)
                                    {
                                        helper.Copy(folder.Path);
                                    }
                                }

                                else
                                {
                                    helper.Copy(folder.Path);
                                }
                            }
                            catch(Exception ex)
                            {
                                await MainWindow.ShowContentDialogAsync(
                                    "Error",
                                    $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}",
                                    "OK"
                                );

                                return;
                            }
                        }

                        if(isMaskAvailable)
                        {
                            folder = null;

                            var result = await MainWindow.ShowContentDialogAsync(
                                "Export Data",
                                "How do you want to export the labeled mask data?",
                                "One file",
                                "Multiple files",
                                "Skip"
                            );

                            if(result == ContentDialogResult.Primary || result == ContentDialogResult.Secondary)
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
                        LoadData();
                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";

                        switch(labeledDatas[viewModel.CurrentIndex - 1])
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
                img_scrollView.ZoomTo(3F, new((float) currentData.X, (float) currentData.Y));
            }
            else if(viewModel.IsZoomModeEnabled)
            {
                double zoomFactor = img_scrollView.ZoomFactor;

                double viewportWidth = img_scrollView.ViewportWidth;
                double viewportHeight = img_scrollView.ViewportHeight;

                double scrollOffsetX = currentData.X * zoomFactor - (viewportWidth / 2);
                double scrollOffsetY = currentData.Y * zoomFactor - (viewportHeight / 2);

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
            DeleteMasks();

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
            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Rectangle))
                {
                    canvas.Children.Remove(child);
                }
            }

            var toRemove = canvas.Children.OfType<Microsoft.UI.Xaml.Shapes.Rectangle>()
                              .Where(rect => rect.Name == "contour")
                              .ToList();

            foreach (var rect in toRemove)
            {
                canvas.Children.Remove(rect);
            }
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if(btn_hideBBox.IsChecked == true)
            {
                DeleteMasks();
            }
            else
            {
                if (viewModel.UseBBoxAsTarget) CreateBBox();
                else DrawContours();
            }
        }

        private void DrawContours()
        {
            DeleteMasks();

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

            canvas.Children.Add(rect);
        }

        private async Task LoadData()
        {
            DeleteMasks();

            if(viewModel.UseBBoxAsTarget)
            {
                await Task.Run(() => {

                    if(BoundingBoxes == null || BoundingBoxes.Count == 0)
                    {
                        BoundingBoxes = helper.GetBoundingBox();                        
                    }

                    DispatcherQueue.TryEnqueue(() =>
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
                    });
                });
            }

            else
            {
                await Task.Run(() =>
                {
                    var data = AnalyzeHelper.GetMaskData($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}", (viewModel.CurrentIndex - 1).ToString());
                    var coords = AnalyzeHelper.CalculateMaskSize(data);

                    currentMask = AnalyzeHelper.GetMask($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}", (viewModel.CurrentIndex - 1).ToString());
                    currentData = new(coords.Item3, coords.Item4, coords.Item1, coords.Item2);
                    
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        viewModel.AllIndex = AnalyzeHelper.GetMaskCount($@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{inputFile}");
                        viewModel.CurrentBoundingBox = $"X: {currentData.X}, Y: {currentData.Y}, W: {currentData.Width}, H: {currentData.Height}";

                        if (btn_hideBBox.IsChecked == false)
                        {
                            DrawContours();
                        }

                        CreateThumbnailBBox();

                        scrollTo();
                    });
                });
            }
        }

        private void btn_useBBox_Click(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => viewModel.UseBBoxAsTarget = btn_useBBox.IsChecked == true);
            LoadData();
        }
    }
}
