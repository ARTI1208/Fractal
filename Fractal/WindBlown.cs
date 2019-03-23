using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fractal
{
    public class WindBlown : Fractal
    {
        private static double _angle1 = 55 * (Math.PI / 180);

        private static double Angle1
        {
            set => _angle1 = value * (Math.PI / 180);
        }

        private static double _angle2 = 35 * (Math.PI / 180);

        private static double Angle2
        {
            set => _angle2 = value * (Math.PI / 180);
        }
        
        private static double _ratio = 0.7;

        private static int LengthRatio
        {
            set => _ratio = value / 100d;
        }
        
        public override void Calculate()
        {
            Canvas.Children.Clear();
            FirstIterationLineLength = GetRealCanvasWidth()/4;            
            Draw(new Point(GetRealCanvasWidth()/2, GetRealCanvasHeight()), 1);
        }
        
        public override void Draw(Point startPoint, uint level)
        {
            Line line = new Line
            {
                X1 = startPoint.X,
                X2 = startPoint.X,
                Y1 = startPoint.Y,
                Y2 = startPoint.Y - FirstIterationLineLength,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Canvas.Children.Add(line);
            if (level < Depth)
            {
                Draw(line, level + 1, Math.PI/2);
            }
        }
        
        private void Draw(Line previousLine, uint level, double prevAngle)
        {
            double leftAngle = prevAngle +  _angle1;
            double rightAngle = prevAngle - _angle2;  
            Line line1 = new Line
            {
                X1 = previousLine.X2,
                X2 = previousLine.X2 + Math.Cos(leftAngle) * FirstIterationLineLength * Math.Pow(_ratio, level - 1),
                Y1 = previousLine.Y2,
                Y2 = previousLine.Y2 - Math.Sin(leftAngle) * FirstIterationLineLength * Math.Pow(_ratio, level - 1),
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Line line2 = new Line
            {
                X1 = previousLine.X2,
                X2 = previousLine.X2 + Math.Cos(rightAngle) * FirstIterationLineLength * Math.Pow(_ratio, level - 1),
                Y1 = previousLine.Y2,
                Y2 = previousLine.Y2 - Math.Sin(rightAngle) * FirstIterationLineLength * Math.Pow(_ratio, level - 1),
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Canvas.Children.Add(line1);
            Canvas.Children.Add(line2);
            if (level >= Depth) 
                return;
            Draw(line1, level + 1, leftAngle);
            Draw(line2, level + 1, rightAngle);
        }
        
        public void SetLengthRatio(int ratio)
        {
            LengthRatio = ratio;
            Calculate();
        }
        
        public void SetLeftLineAngle(uint angle)
        {
            Angle1 = angle;
            Calculate();
        }
        
        public void SetRightLineAngle(uint angle)
        {
            Angle2 = angle;
            Calculate();
        }
    }
}