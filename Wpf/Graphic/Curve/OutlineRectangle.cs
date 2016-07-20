using System;
using System.Collections.Generic;
using System.Linq;

using MapControl;

namespace Abnaki.Albiruni.Graphic.Curve
{
    class OutlineRectangle : MapPolyline
    {
        public OutlineRectangle(decimal west, decimal east, decimal south, decimal north)
            : this ((double)west, (double)east, (double)south, (double)north)
        {

        }

        public OutlineRectangle(double west, double east, double south, double north)
        {
            this.Locations = new Location[]
            {
                new Location(north, west),
                new Location(north, east),
                new Location(south, east),
                new Location(south, west)
            };

            IsClosed = true;
        }
    }
}
