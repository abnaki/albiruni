using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;

using MapControl;
using Abnaki.Windows.Software.Wpf.Menu;
// also has System.Windows.Media.Imaging extension methods defined in WriteableBitmapEx.Wpf.dll

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
            get { return (MapViewModel)base.DataContext; }
            set { base.DataContext = value; }
        }

        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            if (false == msg.Checked)
                return;

            switch (msg.Key)
            {
                    // should set a color field

                case Menu.OptionMenuKey.MapCellColorRed:
                case Menu.OptionMenuKey.MapCellColorGreen:
                case Menu.OptionMenuKey.MapCellColorBlue:
                    InvalidateVisual();
                    break;
            }

        }

        public bool RectanglesValid { get; set; }

        void UpdateRectangles()
        {
            if (ParentMap.RenderSize.Width == 0 || ParentMap.RenderSize.Height == 0 )
                return;

            int color = 0x20ff0000;
            rectangles.Clear();
            geo.Children.Clear();

            double dpix = ParentMap.RenderTransform.Value.M11;
            double dpiy = ParentMap.RenderTransform.Value.M22;
            bitmap = new WriteableBitmap((int)ParentMap.RenderSize.Width, (int)ParentMap.RenderSize.Height, 96, 96, PixelFormats.Pbgra32, palette: null);

            foreach (MapRectangle mrect in this.DataContext.Rectangles)
            {
                if (false)
                {
                    Rect r = RectFromMap(mrect);
                    rectangles.Add(r);
                    RectangleGeometry rgeom = new RectangleGeometry(RectFromMap(mrect));
                    geo.Children.Add(rgeom);
                }
                else
                {
                    Point pnw = ParentMap.LocationToViewportPoint(new Location(mrect.North, mrect.West));
                    Point pse = ParentMap.LocationToViewportPoint(new Location(mrect.South, mrect.East));
                    bitmap.FillRectangle((int)pnw.X, (int)pnw.Y, (int)pse.X, (int)pse.Y, color, doAlphaBlend: true);
                }
            }

            Debug.WriteLine(GetType().Name + " UpdateRectangles " + this.DataContext.Rectangles.Count);

        }

        List<Rect> rectangles = new List<Rect>();
        GeometryGroup geo = new GeometryGroup();

        WriteableBitmap bitmap;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (ParentMap == null || this.DataContext == null)
                return;

            if (false == RectanglesValid)
            {
                UpdateRectangles();
                RectanglesValid = true;
            }

            //Debug.WriteLine(GetType().Name + " rendering " + this.DataContext.Rectangles.Count + " rectangles");

            //Stopwatch watch = new Stopwatch();
            //watch.Start();

            // anticipate need to shift
            double newxoff = this.ParentMap.ViewportTransform.Matrix.OffsetX;
            double oldxoff = this.DataContext.ViewportMatrixSettled.OffsetX;
            double newyoff = this.ParentMap.ViewportTransform.Matrix.OffsetY;
            double oldyoff = this.DataContext.ViewportMatrixSettled.OffsetY;
            //if ( oldxoff != newxoff )
            //    Debug.WriteLine("ViewportTransform.Matrix OffsetX changed " + oldxoff + " to " + newxoff);
            //if ( oldyoff != newyoff )
            //    Debug.WriteLine("ViewportTransform.Matrix OffsetY changed " + oldyoff + " to " + newyoff);
            // OffsetX increases while panning west.
            // OffsetY increases while panning north.
            //Matrix m = new Matrix { OffsetX = , OffsetY =  };
            //geo.Transform = new MatrixTransform(m);
            geo.Transform = new MatrixTransform(1, 0, 0, 1, newxoff - oldxoff, newyoff - oldyoff);

            if (false)
            {
                dc.DrawGeometry(this.DataContext.DefaultFillBrush, null, this.geo);
            }
            else
            {
                Rect r = new Rect(new Point(newxoff - oldxoff, newyoff - oldyoff), this.RenderSize);
                dc.DrawImage(this.bitmap, r);
            }

            //watch.Stop();
            //Debug.WriteLine("Rendered in " + watch.Elapsed);

            //Debug.WriteLine("Rendered " + this.geo.Children.Count);


        }

        //    foreach ( MapRectangle mrect in this.DataContext.Rectangles )
        //    {
        //        Render(dc, mrect);
        //    }
        //}

        Pen testPen = new Pen(Brushes.Purple, 1); // good for development

        //void Render(DrawingContext dc, MapRectangle mrect)
        //{
        //    Rect r = RectFromMap(mrect);

        //    dc.DrawRectangle(mrect.Fill, rectangle: r,
        //        pen: null // testPen
        //        );

        //}

        Rect RectFromMap(MapRectangle mrect)
        {
            return new Rect(ParentMap.LocationToViewportPoint(new Location(mrect.North, mrect.West)),
                ParentMap.LocationToViewportPoint(new Location(mrect.South, mrect.East)));

        }


    }
}
