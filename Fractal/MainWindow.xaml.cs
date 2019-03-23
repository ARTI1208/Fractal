using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Fractal
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        public static Fractal CurrentFractal;
        public static int Zoom = 1;
        private ScaleTransform _canvasZoom = new ScaleTransform();
        private TranslateTransform _canvasTranslate = new TranslateTransform();
        private Point _previousPosition;
        private bool _isMouseCaptured;
        private Color _background = Brushes.Gold.Color;

        public MainWindow()
        {
            _timer.Tick += (sender, args) =>
            {                
                CurrentFractal.Calculate();
                _timer.Stop();   
            };
            InitializeComponent();
            Fractal.SetCanvas(FractalCanvas);
            Fractal.SetDepthTextBox(DepthTextBox);
            CurrentFractal = new NFractal();
            BackgroundColorTextBox.Text = $"#{Brushes.Gold.Color.ToString().Substring(3).ToLower()}";
        }

        private void OnDepthChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentFractal == null || DepthTextBox.Text.Length == 0)
                return;
            int caret = DepthTextBox.CaretIndex - 1;
            if (!uint.TryParse(DepthTextBox.Text, out var depth) || depth < 1)
            {
                if (depth < 1)
                    MessageBox.Show("Only positive int can be there");    
                else
                    MessageBox.Show("Integer expected");
                TextChange textChange = e.Changes.ElementAt(0);
                int iAddedLength = textChange.AddedLength;
                int iOffset = textChange.Offset;
                DepthTextBox.Text = DepthTextBox.Text.Remove(iOffset, iAddedLength);
                DepthTextBox.CaretIndex = caret;
                return;
            }
            CurrentFractal.SetDepthLevel(depth);
        }
     
        private void OnColorChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(EndColorTextBox))
            {
                var color = GetNewColor(EndColorTextBox, e, Fractal.GetEndColor());
                if (Fractal.GetEndColor() != color)
                {
                    CurrentFractal.SetEndColor(color);    
                }   
            }
            else
            {
                var color = GetNewColor(StartColorTextBox, e, Fractal.GetStartColor());
                if (Fractal.GetStartColor() != color)
                {
                    CurrentFractal.SetStartColor(color);    
                }
            }
        }

        private static Color GetNewColor(TextBox tb, TextChangedEventArgs e, Color oldColor)
        {
            int length = tb.Text.Length;            
            if (!tb.Text.StartsWith("#") || length > 7)
            {
                OnColorChangeError(tb, "Wrong format. Color should look like this: #rrggbb", e);
                return oldColor;
            }
            tb.Foreground = length < 7 ? Brushes.Red : Brushes.Black;
            if (length != 7) 
                return oldColor;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var color = (Color) ColorConverter.ConvertFromString(tb.Text);
                return color;
            }
            catch (Exception)
            {
                OnColorChangeError(tb, "Not a color", e);
                return oldColor;
            }
        }

        private static void OnColorChangeError(TextBox tb, string message, TextChangedEventArgs e)
        {
            int length = tb.Text.Length;
            int caret = tb.CaretIndex > 0 ? tb.CaretIndex - 1 : 1;
            TextChange textChange = e.Changes.ElementAt(0);
            int iAddedLength = textChange.AddedLength;
            int iOffset = textChange.Offset;
            tb.Text = tb.Text.Remove(iOffset, iAddedLength);
            if (length == 0)
                tb.Text = "#";
            tb.CaretIndex = caret;
            MessageBox.Show(message);
        }

        private void OnFractalChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is RadioButton rb)) 
                return;
               
            switch (rb.Name)
            {
                default:
                    CurrentFractal = new NFractal();
                    if (ThisSettings != null)
                        ThisSettings.Visibility = Visibility.Hidden; 
                    break;
                case "CCurveRadio":
                    CurrentFractal = new CCurve();
                    if (ThisSettings != null) 
                        ThisSettings.Visibility = Visibility.Visible;
                    WindBlownSettings.Visibility = Visibility.Collapsed;
                    CCurveSettings.Visibility = Visibility.Visible;
                    break;
                case "WindBlownRadio":
                    if (ThisSettings != null) 
                        ThisSettings.Visibility = Visibility.Visible;

                    WindBlownSettings.Visibility = Visibility.Visible;
                    CCurveSettings.Visibility = Visibility.Collapsed;
                    
                    CurrentFractal = new WindBlown();
                    break;
            }
        }

        private void OnSaveItemClick(object sender, RoutedEventArgs e)
        {
            var filename = "";
            switch (CurrentFractal)
            {
                default:
                    filename += "N-Fractal";
                    break;
                case CCurve _:
                    filename += "Levy-C-Curve";
                    break;
                case WindBlown _:
                    filename += "WindBlown-Tree";
                    break;
            }
            filename += $"_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            var dialog = new SaveFileDialog
            {
                AddExtension = true, Filter = "PNG(*.png)|*.png|JPEG(*.jpg)|*.jpg;", FileName = filename
            };
            if (dialog.ShowDialog() == true)
                SaveImage(dialog.FileName);
        }

        private void SaveImage(string filename)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(FractalCanvas);
            double dpi = 96d;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int) bounds.Width/Zoom, (int) bounds.Height/Zoom, dpi, dpi, 
                PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(FractalCanvas);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            BitmapEncoder encoder = new PngBitmapEncoder();                
            if (filename.EndsWith("jpg"))
                encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (FileStream file = File.Create(filename))
                encoder.Save(file);
        }

        private void OnLengthRatioChanged(object sender, TextChangedEventArgs e)
        {
            if (LengthRatioTextBox.Text.Length == 0)
                return;
            if (!(CurrentFractal is WindBlown treeFractal)) 
                return;
            bool num;
            if (!(num = int.TryParse(LengthRatioTextBox.Text, out var k)) || k <= 0 || k > 100)
            {
                int caret = LengthRatioTextBox.CaretIndex > 0 ? LengthRatioTextBox.CaretIndex - 1 : 1;
                TextChange textChange = e.Changes.ElementAt(0);
                int iAddedLength = textChange.AddedLength;
                int iOffset = textChange.Offset;
                LengthRatioTextBox.Text = LengthRatioTextBox.Text.Remove(iOffset, iAddedLength);
                LengthRatioTextBox.CaretIndex = caret;
                string message = num ? "Length ratio in % (1 - 100)" : "Integer expected";
                MessageBox.Show(message);
                return;
            }
            treeFractal.SetLengthRatio(k);
        }

        private void OnAngleChanged(object sender, TextChangedEventArgs e)
        {
            if (!(CurrentFractal is WindBlown treeFractal)) 
                return;
            TextBox tb = (TextBox) sender;
            bool num;
            if (!(num = uint.TryParse(tb.Text, out var angle)) || angle > 180)
            {
                int caret = tb.CaretIndex > 0 ? tb.CaretIndex - 1 : 1;
                TextChange textChange = e.Changes.ElementAt(0);
                int iAddedLength = textChange.AddedLength;
                int iOffset = textChange.Offset;
                tb.Text = tb.Text.Remove(iOffset, iAddedLength);
                tb.CaretIndex = caret;
                string message = num ? "Angle in degrees (0-180)" : "Integer expected";
                MessageBox.Show(message);
                return;
            }
            if (sender.Equals(LeftLineAngleTextBox))
                treeFractal.SetLeftLineAngle(angle);
            else
                treeFractal.SetRightLineAngle(angle);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (Math.Abs(sizeInfo.PreviousSize.Height) < Math.Pow(10, -8))
                return;
            if (!sizeInfo.HeightChanged)
                return;
            double k = sizeInfo.NewSize.Height / sizeInfo.PreviousSize.Height;
            if (Math.Abs(k - 1) < Math.Pow(10, -4))
                return;
            _timer.Stop();
            ResizableArea.Height *= k;
            ResizableArea.Width *= k;
            FractalCanvas.Height *= k;
            FractalCanvas.Width *= k;
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Start();
        }
        
        private void OnZoomItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem) sender;
            int newZoom = Convert.ToInt32(item.Tag);
            if (Zoom == newZoom)
                return;
            _canvasZoom = new ScaleTransform 
            {
                ScaleX = newZoom, 
                ScaleY = newZoom
            };
            FractalCanvas.Height = newZoom * FractalCanvas.Height/Zoom;
            FractalCanvas.Width = newZoom * FractalCanvas.Width/Zoom;            
            TransformGroup group = new TransformGroup();
            group.Children.Add(_canvasTranslate);
            group.Children.Add(_canvasZoom);
            FractalCanvas.RenderTransform = group;
            Zoom = newZoom;
            ResizableArea.HorizontalScrollBarVisibility = 
                newZoom == 1 ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
            ResizableArea.VerticalScrollBarVisibility = 
                newZoom == 1 ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
        }

        private void OnMoveStart(object sender, MouseButtonEventArgs e)
        {
            _previousPosition = new Point(e.GetPosition(this).X - _canvasTranslate.X*Zoom, e.GetPosition(this).Y -_canvasTranslate.Y*Zoom);
            _isMouseCaptured = true;
        }
        
        private void OnMove(object sender, MouseEventArgs e)
        {
            if (_isMouseCaptured)
                MoveCanvas(e.GetPosition(this));
        }
        
        private void OnMoveEnd(object sender, MouseButtonEventArgs e)
        {
            _previousPosition = e.GetPosition(this);
            _isMouseCaptured = false;
        }
        
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseCaptured = false;
        }

        private void MoveCanvas(Point position)
        {
            _canvasTranslate = new TranslateTransform
            {
               
                X = (position.X - _previousPosition.X)/Zoom,
                Y = (position.Y-_previousPosition.Y)/Zoom
               
            };
            TransformGroup group = new TransformGroup();
            group.Children.Add(_canvasTranslate);
            group.Children.Add(_canvasZoom);
            FractalCanvas.RenderTransform = group;
        }
        
        private void OnTranslateRemClick(object sender, RoutedEventArgs e)
        {
            TransformGroup group = new TransformGroup();
            _canvasTranslate = new TranslateTransform();
            group.Children.Add(_canvasZoom);
            FractalCanvas.RenderTransform = group;
        }
        
        private void OnBackChanged(object sender, TextChangedEventArgs e)
        {
            _background = GetNewColor(BackgroundColorTextBox, e, _background);
            ResizableArea.Background = new SolidColorBrush(_background);
            FractalCanvas.Background = new SolidColorBrush(_background);
        }

        private void PreviousIterationsCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            CCurve.ShowPreviousIterations = true;

            if (Fractal.ShouldDraw(WarningWindow.LeaveOldDepthLevel))
                CurrentFractal.Calculate();
            else
                ((CheckBox) sender).IsChecked = false;
        }

        private void PreviousIterationsCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            CCurve.ShowPreviousIterations = false;
            CurrentFractal.Calculate();
        }
    }
}