using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.Home.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
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

                contentDialog.SecondaryButtonClick += (_s, _e) => {
                    this.Close();
                };

                await contentDialog.ShowAsync();
            }

            helper.CreateCSVFile();

            BoundingBoxes = helper.GetBoundingBox();

            viewModel.AllIndex = BoundingBoxes.Count;
            viewModel.CurrentIndex = 1;
            viewModel.CurrentBoundingBox = $"X: {BoundingBoxes[viewModel.CurrentIndex - 1].X}, Y: {BoundingBoxes[viewModel.CurrentIndex - 1].Y}, W: {BoundingBoxes[viewModel.CurrentIndex - 1].Width}, H: {BoundingBoxes[viewModel.CurrentIndex - 1].Height}";

            CreateBBox();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "btn_next":
                    if(viewModel.CurrentIndex < viewModel.AllIndex)
                    {
                        if (IsEditMode)
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

                        else
                        {
                            helper.AppendText(
                                (int)classType,
                                BoundingBoxes[viewModel.CurrentIndex - 1].X.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Y.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Width.ToString(),
                                BoundingBoxes[viewModel.CurrentIndex - 1].Height.ToString()
                            );
                        }

                        viewModel.CurrentIndex += 1;

                        if (viewModel.CurrentIndex == EditModeEndIndex) IsEditMode = false;

                        viewModel.CurrentBoundingBox = $"X: {BoundingBoxes[viewModel.CurrentIndex - 1].X}, Y: {BoundingBoxes[viewModel.CurrentIndex - 1].Y}, W: {BoundingBoxes[viewModel.CurrentIndex - 1].Width}, H: {BoundingBoxes[viewModel.CurrentIndex - 1].Height}";
                        btn_previous.IsEnabled = viewModel.CurrentIndex > 1;

                        if (viewModel.CurrentIndex < viewModel.AllIndex)
                        {
                            btn_next.IsEnabled = true;
                        }

                        else
                        {
                            btn_next.Content = "Done";
                        }

                        CreateBBox();
                    }

                    else
                    {
                        this.Close();
                    }

                    break;

                case "btn_previous":
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

                        CreateBBox();
                    }

                    break;
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
                Width = BoundingBoxes[viewModel.CurrentIndex].Width,
                Height = BoundingBoxes[viewModel.CurrentIndex].Height,
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            canvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, BoundingBoxes[viewModel.CurrentIndex].X);
            bBox.SetValue(Canvas.TopProperty, BoundingBoxes[viewModel.CurrentIndex].Y);
        }
    }
}
