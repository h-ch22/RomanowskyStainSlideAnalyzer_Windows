using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using RomanowskyStainSlideAnalyzer.Home.Helper;
using RomanowskyStainSlideAnalyzer.Home.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Home.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ParameterControlWindow : Window
    {
        private SegmentParameterDataModel viewModel;
        private List<string> presets;
        private string currentPreset = "Default";
        private string presetName { get; set; }

        public ParameterControlWindow(SegmentParameterDataModel viewModel)
        {
            this.InitializeComponent();
            presetName = "";
            this.viewModel = viewModel;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(750, 750));
            stackPanel.DataContext = viewModel;
            Init();
        }

        private void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            AddPreset();
        }

        private void AddPreset()
        {
            if(presets != null && presets.Count() > 0) presets.Clear();
            presets = SegmentationHelper.loadPresets();
            MenuFlyout menuFlyout = new();

            foreach (var preset in presets)
            {
                var presetFlyout = new MenuFlyoutItem();
                presetFlyout.Text = preset;
                presetFlyout.Click += OnPresetSelected;

                menuFlyout.Items.Add(presetFlyout);
            }

            btn_presets.Flyout = menuFlyout;
        }

        private void OnPresetSelected(object sender, RoutedEventArgs e)
        {
            currentPreset = (sender as MenuFlyoutItem).Text;
            ApplyPreset();
        }

        private void ApplyPreset()
        {
            var preset = SegmentationHelper.loadPreset(currentPreset);

            viewModel.PointsPerSide = preset.PointsPerSide;
            viewModel.PointsPerBatch = preset.PointsPerBatch;
            viewModel.PredIoUThresh = preset.PredIoUThresh;
            viewModel.StabilityScoreThresh = preset.StabilityScoreThresh;
            viewModel.StabilityScoreOffset = preset.StabilityScoreOffset;
            viewModel.MaskThreshold = preset.MaskThreshold;
            viewModel.BoxNMSThresh = preset.BoxNMSThresh;
            viewModel.CropNLayers = preset.CropNLayers;
            viewModel.CropNMSThresh = preset.CropNMSThresh;
            viewModel.CropOverlapRatio = preset.CropOverlapRatio;
            viewModel.CropNPointsDownScaleFactor = preset.CropNPointsDownScaleFactor;
            viewModel.MinMaskRegionArea = preset.MinMaskRegionArea;
        }

        public async void OnClick(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "btn_reset":
                    ApplyPreset();
                    break;

                case "btn_save":
                    if(presetName.ToLower() == "default")
                    {
                        await MainWindow.ShowContentDialogAsync("Error", "This preset name cannot be used.", "OK");
                        break;
                    }

                    else if(presetName == "")
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", "Please enter preset name.", "OK");
                        break;
                    }

                    try
                    {
                        var saveResult = SegmentationHelper.createPreset(presetName, viewModel);

                        if (saveResult == 1)
                        {
                            if (await MainWindow.ShowContentDialogAsync("Preset Already Exists", "This preset name already exists.\nDo you want to overwrite it?", "Yes", "No"))
                            {
                                saveResult = SegmentationHelper.createPreset(presetName, viewModel, true);
                            }
                        }

                        await MainWindow.ShowContentDialogAsync(saveResult == 0 ? "Done" : "Error",
                                                                saveResult == 0 ? "Preset have been added." : "An error occured while adding preset.",
                                                                "OK");

                        AddPreset();
                    }

                    catch(Exception ex)
                    {
                        await MainWindow.ShowContentDialogAsync("Error", $"An error occurred while setting preset.\n{ex.Message}", "OK");
                    }

                    break;
            }
        }
    }
}
