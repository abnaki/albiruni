using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Graphical scale capable of non-SI units.
    /// Code fork unfortunately.
    /// </summary>
    class ImperialScale : MapControl.MapScale
    {
        private double length;
        private Size size;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ParentMap != null && ParentMap.CenterScale > 0d)
            {
                length = MinWidth / ParentMap.CenterScale;
                var magnitude = Math.Pow(10d, Math.Floor(Math.Log10(length)));

                if (length / magnitude < 2d)
                {
                    length = 2d * magnitude;
                }
                else if (length / magnitude < 5d)
                {
                    length = 5d * magnitude;
                }
                else
                {
                    length = 10d * magnitude;
                }

                size.Width = length * ParentMap.CenterScale + StrokeThickness + Padding.Left + Padding.Right;
                size.Height = FontSize * FontFamily.LineSpacing + StrokeThickness + Padding.Top + Padding.Bottom;
            }
            else
            {
                size.Width = size.Height = 0d;
            }

            return size;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ParentMap != null)
            {
                var x1 = Padding.Left + StrokeThickness / 2d;
                var x2 = size.Width - Padding.Right - StrokeThickness / 2d;
                var y1 = size.Height / 2d;
                var y2 = size.Height - Padding.Bottom - StrokeThickness / 2d;
                var text = new FormattedText(
                    length >= 1000d ? string.Format("{0:0} km", length / 1000d) : string.Format("{0:0} m", length),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Foreground);

                drawingContext.DrawRectangle(Background ?? ParentMap.Background, null, new Rect(size));
                drawingContext.DrawLine(Pen, new Point(x1, y1), new Point(x1, y2));
                drawingContext.DrawLine(Pen, new Point(x2, y1), new Point(x2, y2));
                drawingContext.DrawLine(Pen, new Point(x1, y2), new Point(x2, y2));
                drawingContext.DrawText(text, new Point((size.Width - text.Width) / 2d, 0d));
            }
        }

    }
}
