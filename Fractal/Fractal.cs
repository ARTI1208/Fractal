using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Fractal
{
    public abstract class Fractal
    {
        // ReSharper disable once PossibleNullReferenceException
        private static Color _endColor = (Color) ColorConverter.ConvertFromString("#ff0000");
        // ReSharper disable once PossibleNullReferenceException
        private static Color _startColor = (Color) ColorConverter.ConvertFromString("#0000ff");
        protected static uint Depth = 2;
        // ReSharper disable once UnusedMember.Global
        protected uint CurrentDepthLevel = 1;
        protected double FirstIterationLineLength;
        protected static Canvas Canvas;
        private static TextBox _depthTextBox;
        private const int LinesWarningCount = 100000;
        private static Color[] _gradientList;

        protected Fractal()
        {
            DispatcherTimer t = new DispatcherTimer();
            t.Tick += (sender, args) =>
            {  
                if (LinesToDraw(Depth) > LinesWarningCount)
                    ShowWarningWindow(WarningWindow.ChangeDepthTo2);
                Calculate();    
                t.Stop();
            };
            t.Interval= TimeSpan.FromMilliseconds(50);
            t.Start();
        }

        public abstract void Calculate();
        
        // ReSharper disable once UnusedMemberInSuper.Global
        public abstract void Draw(Point startPoint, uint level);

        protected static double GetRealCanvasHeight() => Canvas.Height / MainWindow.Zoom;
        
        protected static double GetRealCanvasWidth() => Canvas.Width / MainWindow.Zoom;
          
        public override string ToString() => $"{Depth} {_startColor} {_endColor}";

        public static void SetCanvas(Canvas canvas) => Canvas = canvas;

        public static void SetDepthTextBox(TextBox box) => _depthTextBox = box;

        private static int LinesToDraw(uint depth)
        {
            int[] m;
            switch (MainWindow.CurrentFractal)
            {
                default:
                    m = new [] {3, 4, 3};
                    break;
                case CCurve _:
                    m = new [] {1, 2, 1};
                    break;
                case WindBlown _:
                    m = new [] {1, 2, 1};
                    break;
            }                
            int lines = m[0];
            if (MainWindow.CurrentFractal is CCurve && !CCurve.ShowPreviousIterations)
            {
                lines = (int) Math.Pow(m[1], depth - 1) * m[2];
                
            }
            else
                for (int i = 1; i < depth; i++)
                {
                    lines += (int) Math.Pow(m[1], i) * m[2];
                }
            return lines;
        }

        public static bool ShouldDraw(int cancelAction)
        {
            return LinesToDraw(Depth) < LinesWarningCount || ShowWarningWindow(cancelAction);
        }

        private static bool ShowWarningWindow(int cancelAction)
        {
            if (!WarningWindow.DoNotShowAgain)
            {
                WarningWindow warningWindow = new WarningWindow();
                warningWindow.ShowDialog();
            }

            if (WarningWindow.ShouldConstruct())
                return true;
            _depthTextBox.Text = cancelAction == WarningWindow.LeaveOldDepthLevel ? $"{Depth}" : "2";
            return false;
        }
        
        public void SetDepthLevel(uint depth)
        {
            if (depth == Depth) 
                return;
            if (LinesToDraw(depth) > LinesWarningCount && !ShowWarningWindow(WarningWindow.LeaveOldDepthLevel))
                return;
            Depth = depth;
            UpdateGradient();
            Calculate();
        }        
        
        public void SetStartColor(Color startColor)
        {
            _startColor = startColor;
            UpdateGradient();
            Calculate();
        }
        
        public void SetEndColor(Color endColor)
        {
            _endColor = endColor;
            UpdateGradient();
            Calculate();
        }

        public static Color GetStartColor() => _startColor;

        public static Color GetEndColor() => _endColor;

        private static void UpdateGradient()
        {
            _gradientList = new Color[Depth];
            Color start = _startColor;
            Color end = _endColor;
            Color stepper = Color.FromArgb(
                (byte) ((end.A - start.A) / (double) (Depth - 1)),
                (byte) ((end.R - start.R) / (double) (Depth - 1)),
                (byte) ((end.G - start.G) / (double) (Depth - 1)),
                (byte) ((end.B - start.B) / (double) (Depth - 1)));
            for (int i = 0; i < Depth; i++)
            {
                _gradientList[i] = Color.FromArgb(
                    (byte) (start.A + stepper.A * i),
                    (byte) (start.R + stepper.R * i),
                    (byte) (start.G + stepper.G * i),
                    (byte) (start.B + stepper.B * i));
            }
        }

        protected static Color GetGradientColor(uint level)
        {
            if (_gradientList == null)
            {
                UpdateGradient();
            }

            // ReSharper disable once PossibleNullReferenceException
            return _gradientList[level - 1];
        }

    }
}