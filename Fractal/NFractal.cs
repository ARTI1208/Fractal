using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fractal
{
    public class NFractal : Fractal
    {
        
        public override void Calculate()
        {
            if (Canvas == null)
                return;
            double h = GetRealCanvasHeight();
            int h1 = (int) ((h * (0.5 - 1)) / (Math.Pow(0.5, Depth) - 1));
            FirstIterationLineLength = h1;
            Canvas.Children.Clear();
            Draw(new Point(GetRealCanvasWidth() / 2, GetRealCanvasHeight() / 2), 1);
        }

        public override void Draw(Point startPoint, uint level)
        {
            if (level > Depth || Canvas == null) 
                return;
            double move = FirstIterationLineLength / (Math.Pow(2, level));
            Line left = new Line
            {
                X1 = startPoint.X - move,
                X2 = startPoint.X - move,
                Y1 = startPoint.Y - move,
                Y2 = startPoint.Y + move,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Line right = new Line
            {
                X1 = startPoint.X + move,
                X2 = startPoint.X + move,
                Y1 = startPoint.Y - move,
                Y2 = startPoint.Y + move,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Line center = new Line
            {
                X1 = startPoint.X - move,
                X2 = startPoint.X + move,
                Y1 = startPoint.Y,
                Y2 = startPoint.Y,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            
            Canvas.Children.Add(left);
            Canvas.Children.Add(right);
            Canvas.Children.Add(center);
            Draw(new Point(left.X1, left.Y1), level + 1);
            Draw(new Point(left.X2, left.Y2), level + 1);
            Draw(new Point(right.X1, right.Y1), level + 1);
            Draw(new Point(right.X2, right.Y2), level + 1);
        }
        
    }
}