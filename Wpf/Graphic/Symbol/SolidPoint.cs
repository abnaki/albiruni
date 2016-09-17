using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Diagnostics;

using MapControl;

namespace Abnaki.Albiruni.Graphic.Symbol
{
    /// <summary>
    /// Acts like Pushpin; so it requires nontrivial xaml support
    /// to draw anything specific.
    /// </summary>
    class SolidPoint :  Pushpin // has Location
    {
        public SolidPoint()
        {

        }

        public SolidPoint(Location loc, Brush brush)
        {
            MapPanel.SetLocation(this, loc);
            this.Foreground = brush;
            this.Background = brush;
        }

        public override string ToString()
        {
            return GetType().Name + " at " + MapPanel.GetLocation(this) + ", Background " + this.Background;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Visibility == System.Windows.Visibility.Visible)
            {
                base.OnRender(drawingContext);
            }
        }

        static double? ideal_size;

        public static double IdealSize
        {
            get
            {
                if (false == ideal_size.HasValue)
                {
                    double min = 3, max = 16;
                    double overallWidth = 1600; // guess
                    double scale = 320; // denominator, chosen judiciously.
                    try
                    {
                        overallWidth = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    ideal_size = Math.Max(min, Math.Min(overallWidth / scale, max));
                }
                return ideal_size.Value;
            }
        }

        public static double MinusHalfIdealSize
        {
            get { return - IdealSize / 2;  }
        }
    }
}
