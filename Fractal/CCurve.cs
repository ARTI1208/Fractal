using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Fractal
{
    public class CCurve : Fractal
    {
        public static bool ShowPreviousIterations;

        public override void Calculate()
        {
            FirstIterationLineLength = GetRealCanvasHeight() / 2;
            Canvas.Children.Clear();
            Draw(new Point(GetRealCanvasWidth() / 2, GetRealCanvasHeight() / 2), 1);
        }

        public override void Draw(Point startPoint, uint level)
        {
            if (level > Depth || Canvas == null)
                return;
            Line line = new Line
            {
                X1 = startPoint.X,
                X2 = startPoint.X,
                Y1 = startPoint.Y + FirstIterationLineLength / 2,
                Y2 = startPoint.Y - FirstIterationLineLength / 2,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            if (level == Depth || ShowPreviousIterations)
                Canvas.Children.Add(line);
            if (level < Depth)
                Draw(line, level + 1);
        }

        private static void Draw(Line previousLine, uint level)
        {
            if (level > Depth || Canvas == null)
                return;
            Point center = new Point((previousLine.X1 + previousLine.X2) / 2, (previousLine.Y1 + previousLine.Y2) / 2);
            double hh = (center.Y - (previousLine.X2 - previousLine.X1) / 2);
            double hw = (center.X + (previousLine.Y2 - previousLine.Y1) / 2);
            Point nc = new Point(hw, hh);
            Line line1 = new Line
            {
                X1 = nc.X,
                X2 = previousLine.X2,
                Y1 = nc.Y,
                Y2 = previousLine.Y2,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            Line line2 = new Line
            {
                X2 = nc.X,
                X1 = previousLine.X1,
                Y2 = nc.Y,
                Y1 = previousLine.Y1,
                Stroke = new SolidColorBrush(GetGradientColor(level))
            };
            if (level == Depth || ShowPreviousIterations)
            {
                Canvas.Children.Add(line1);
                Canvas.Children.Add(line2);
            }

            if (level >= Depth)
                return;
            Draw(line1, level + 1);
            Draw(line2, level + 1);
        }
    }
}