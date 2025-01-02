using Microsoft.UI;
using Microsoft.UI.Input;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Home.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PointSelectionView : Window
    {
        private SegmentParameterDataModel viewModel;
        private ClassTypeModel classType = ClassTypeModel.TYPE_A;

        public PointSelectionView(SegmentParameterDataModel viewModel)
        {
            this.InitializeComponent();
            this.viewModel = viewModel;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 850));
            Init();
        }

        private void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {

            }
        }

        private void OnClassSelected(object sender, RoutedEventArgs e)
        {
            switch((sender as RadioButton).Name)
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

        private void CreatePoint(object sender, PointerRoutedEventArgs e)
        {
            Pointer ptr = e.Pointer;
            PointerPoint ptrPoint = e.GetCurrentPoint(img_source);
            var point = ptrPoint.Position;
            var transformedPoint = new Point(point.Y, 512-point.X);

            switch (ptr.PointerDeviceType)
            {
                case PointerDeviceType.Mouse:
                case PointerDeviceType.Touchpad:

                    if (ptrPoint.Properties.IsLeftButtonPressed)
                    {
                        CreatePoint(transformedPoint.X, transformedPoint.Y);
                    }

                    break;

                default: break;
            }
        }

        private void RemovePoint(object sender, PointerRoutedEventArgs e)
        {
            var senderPoint = sender as Ellipse;
            Pointer ptr = e.Pointer;
            PointerPoint ptrPoint = e.GetCurrentPoint(senderPoint);

            var point = ptrPoint.Position;

            switch (ptr.PointerDeviceType)
            {
                case PointerDeviceType.Mouse:
                case PointerDeviceType.Touchpad:

                    if (ptrPoint.Properties.IsRightButtonPressed)
                    {
                        var value = viewModel.Points.First(i =>
                            senderPoint == i.ellipse
                        );

                        if(value != null)
                        {
                            viewModel.Points.Remove(value);
                        }

                        canvas.Children.Remove(senderPoint);
                    }

                    break;

                default: break;
            }
        }

        private Color GetColor()
        {
            switch(classType)
            {
                case ClassTypeModel.TYPE_A: return Color.FromArgb(204, 219, 66, 66);
                case ClassTypeModel.TYPE_B: return Color.FromArgb(204, 66, 219, 96);
                case ClassTypeModel.TYPE_C: return Color.FromArgb(204, 66, 66, 219);
                default: return Color.FromArgb(255, 0, 0, 0);
            }
        }

        private void CreatePoint(double x, double y)
        {
            Ellipse point = new()
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(GetColor())
            };

            canvas.Children.Add(point);
            point.SetValue(Canvas.LeftProperty, x - point.Width / 2);
            point.SetValue(Canvas.TopProperty, y - point.Height / 2);
            point.PointerPressed += RemovePoint;

            viewModel.Points.Add(
                new PointDataModel(x, y, classType, point)
            );
        }
    }
}
