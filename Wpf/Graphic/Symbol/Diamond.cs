using System;
using System.Collections.Generic;
using System.Linq;

using MapControl;

namespace Abnaki.Albiruni.Graphic.Symbol
{
    class Diamond : MapControl.MapPolyline
    {
        public Diamond(Location center, double width, double height)
        {
            this.Locations = new Location[]{
                new Location(center.Latitude + height / 2, center.Longitude),
                new Location(center.Latitude, center.Longitude - width/2), 
                new Location(center.Latitude - height/2, center.Longitude),
                new Location(center.Latitude, center.Longitude + width/2)
            };

            IsClosed = true;
        }
    }
}
