using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Graphic
{
    class Graticule : MapControl.MapGraticule
    {
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (IsEnabled)
            {
                base.OnRender(drawingContext);
            }
        }
    }
}
