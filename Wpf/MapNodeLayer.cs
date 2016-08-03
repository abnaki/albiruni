using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

using MapControl;
using System.Windows.Threading;
using Abnaki.Windows.Software.Wpf.Menu;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Draws MapViewModel Rectangles as fast as possible
    /// </summary>
    class MapNodeLayer : MapPanel
    {
        public MapNodeLayer()
        {
            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);
        }

        public new MapViewModel DataContext
        {
            get { return (MapViewModel)base.DataContext;  }
            set { base.DataContext = value; }
        }

        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            if (false == msg.Checked)
                return;

            switch (msg.Key)
            {
                case Menu.OptionMenuKey.MapCellColorRed:
                case Menu.OptionMenuKey.MapCellColorGreen:
                case Menu.OptionMenuKey.MapCellColorBlue:
                    InvalidateVisual();
                    break;
            }

        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (ParentMap == null || this.DataContext == null )
                return;

            //Debug.WriteLine(GetType().Name + " rendering " + this.DataContext.Rectangles.Count + " rectangles");

            foreach ( MapRectangle mrect in this.DataContext.Rectangles )
            {
                Render(dc, mrect);
            }
        }

        void Render(DrawingContext dc, MapRectangle mrect)
        {
            Rect r = RectFromMap(mrect);
            dc.DrawRectangle(mrect.Fill, pen: null, rectangle: r);
        }

        Rect RectFromMap(MapRectangle mrect)
        {
            return new Rect(ParentMap.LocationToViewportPoint(new Location(mrect.North, mrect.West)),
                ParentMap.LocationToViewportPoint(new Location(mrect.South, mrect.East)));

        }
    }
}
