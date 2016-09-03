using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AutoDependencyPropertyMarker;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Graphical scale capable of non-SI units.
    /// Code fork unfortunately.
    /// </summary>
    class ImperialScale : MapControl.MapScale
    {
        private double meters;
        private Size size;

        [AutoDependencyProperty]
        public bool Metric { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ParentMap != null && ParentMap.CenterScale > 0d)
            {
                meters = MinWidth / ParentMap.CenterScale;
                var magnitude = Math.Pow(10d, Math.Floor(Math.Log10(meters)));

                if (meters / magnitude < 2d)
                {
                    meters = 2d * magnitude;
                }
                else if (meters / magnitude < 5d)
                {
                    meters = 5d * magnitude;
                }
                else
                {
                    meters = 10d * magnitude;
                }

                size.Width = meters * ParentMap.CenterScale + WasteWidth;
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
                string label;
                Size rsize;
                if (Metric)
                {
                    rsize = this.size;
                    label = meters >= 1000d ? string.Format("{0:0} km", meters / 1000d) : string.Format("{0:0} m", meters);
                }
                else
                {
                    // expanding round km lengths into miles
                    rsize = new Size((this.size.Width - WasteWidth) * 1.609 + WasteWidth, this.size.Height);
                    
                    double lengths = meters / 1000;

                    if (lengths < 0.1)
                        label = lengths.ToString("G");
                    if (lengths < 1)
                        label = lengths.ToString("0.0");
                    else
                        label = lengths.ToString("0");

                    label += " mile(s)";
                }
                Render(drawingContext, label, rsize);
            }
        }

        void Render(DrawingContext drawingContext, string label, Size bounds)
        {
            var x1 = Padding.Left + StrokeThickness / 2d;
            var x2 = bounds.Width - Padding.Right - StrokeThickness / 2d;
            // that means x2 - x1 = bounds.Width - WasteWidth
            var y1 = bounds.Height / 2d;
            var y2 = bounds.Height - Padding.Bottom - StrokeThickness / 2d;

            var text = new FormattedText(label,
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Foreground);

            drawingContext.DrawRectangle(Background ?? ParentMap.Background, null, new Rect(bounds));
            drawingContext.DrawLine(Pen, new Point(x1, y1), new Point(x1, y2));
            drawingContext.DrawLine(Pen, new Point(x2, y1), new Point(x2, y2));
            drawingContext.DrawLine(Pen, new Point(x1, y2), new Point(x2, y2)); // represents length
            drawingContext.DrawText(text, new Point((bounds.Width - text.Width) / 2d, 0d));
        }

        double WasteWidth
        {
            get
            {
                return Padding.Left + Padding.Right + StrokeThickness;
            }
        }
    }
}
