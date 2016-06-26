using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

using Abnaki.Albiruni.Tree;
using Abnaki.Windows.Software.Wpf;
using MapControl;
using PropertyChanged;

namespace Abnaki.Albiruni
{
    [ImplementPropertyChanged]
    public class MapViewModel 
    {
        public MapViewModel()
        {
            Rectangles = new ObservableCollection<MapRectangle>();
            Symbols = new ObservableCollection<MapPath>();
            Tracks = new ObservableCollection<MapPath>();
            ViewPortRect = new MapRectangle(); // unspecified
            MinimumPrecision = 1;

            MessageTube.Subscribe<Node>(HandleTree);
        }

        public Location MapCenter { get; set; }

        public ObservableCollection<MapRectangle> Rectangles { get; private set; }
        public ObservableCollection<MapPath> Symbols { get; private set; }
        public ObservableCollection<MapPath> Tracks { get; private set; }

        public double MinimumPrecision { get; set; }

        /// <summary>Last known rectangle; not bound to control.  i.e. set{} does not affect the map.
        /// </summary>
        public MapRectangle ViewPortRect { get; set; }

        //public ObservableCollection<Point> Points { get; set; }
        //public ObservableCollection<Polyline> Polylines { get; set; }

        public void HandleTree(Node root)
        {
            ClearAdornments();

            //root.DebugPrint();

            Node.Statistic stat = root.GetStatistic();
            Debug.WriteLine(stat);
            if (stat.ContentSummary.Points > 0)
            {
                double midLat = (stat.ContentSummary.MinLatitude.Value + stat.ContentSummary.MaxLatitude.Value) / 2;
                double midLon = (stat.ContentSummary.MinLongitude.Value + stat.ContentSummary.MaxLongitude.Value) / 2;

                // predicting bounds from last known view
                double viewWidth = ViewPortRect.East - ViewPortRect.West;
                double viewHeight = ViewPortRect.North - ViewPortRect.South;

                MapCenter = new Location(midLat, midLon);

                MapRectangle logicalBound = new MapRectangle()
                {
                    West = midLon - viewWidth / 2,
                    East = midLon + viewWidth / 2,
                    North = midLat + viewHeight / 2,
                    South = midLat - viewHeight / 2
                };

                AddDescendantRectangles(root, null, logicalBound);
            }
        }

        /// <summary>
        /// Here is the principal motivation for Albiruni.
        /// Add a Rectangle for every Node in tree that intersects ViewPortRect,
        /// not descending to any Nodes having Delta < MinimumPrecision.
        /// </summary>
        /// <returns>true if anything added</returns>
        bool AddDescendantRectangles(Node node, Node parent, MapRectangle logicalBound)
        {
            if (node.Delta < MinimumPrecision)
                return false;

            switch (node.Axis)
            {
                case Axis.NorthSouth:
                    if (node.Degrees > logicalBound.North)
                        return false;
                    if (node.Degrees + node.Delta < logicalBound.South)
                        return false;
                    break;
                case Axis.EastWest:
                    if (node.Degrees > logicalBound.East)
                        return false;
                    if (node.Degrees + node.Delta < logicalBound.West)
                        return false;
                    break;
            }

            bool childRectanglesAdded = false;
            if ( node.Children != null )
            {
                childRectanglesAdded |= AddDescendantRectangles(node.Children.Item1, node, logicalBound);
                childRectanglesAdded |= AddDescendantRectangles(node.Children.Item2, node, logicalBound);
            }

            if (childRectanglesAdded)
                return true;

            if ( node.Children != null && parent != null && parent.Delta >= MinimumPrecision )
            {
                // any descendants exist, and node has not been excluded for view or precision reasons (above)
                MapRectangle r;
                switch ( node.Axis )
                {
                    case Axis.NorthSouth:
                        r = new MapRectangle() { 
                            South = node.Degrees, North = node.Degrees + node.Delta, 
                            West = parent.Degrees, East = parent.Degrees + parent.Delta
                        };
                        break;
                    case Axis.EastWest:
                        r = new MapRectangle()
                        {
                            South = parent.Degrees, North = parent.Degrees + parent.Delta,
                            West = node.Degrees, East = node.Degrees + node.Delta
                        };
                        break;
                    default:
                        throw new NotSupportedException("No support for axis " + node.Axis);
                }
                r.Fill = m_defaultFillBrush;
                this.Rectangles.Add(r); // efficient ?
                return true;
            }
            return false;
        }

        System.Windows.Media.Brush m_defaultFillBrush = new SolidColorBrush(Color.FromArgb((byte)32, (byte)255, (byte)0, (byte)0));


        void ClearAdornments()
        {
            Rectangles.Clear();
            Symbols.Clear();
            Tracks.Clear();
        }

        public void Testing()
        {
            MapCenter = new Location(30, -100); // Texas

            MapRectangle r = new MapRectangle();
            r.South = MapCenter.Latitude;
            r.West = MapCenter.Longitude;
            r.North = r.South + 0.1;
            r.East = r.West + 0.1;
            r.Fill = m_defaultFillBrush;
            Rectangles.Add(r);

            var diamond = new Abnaki.Albiruni.Graphic.Symbol.Diamond(MapCenter, 0.01, 0.015);
            diamond.Fill = new SolidColorBrush(Color.FromArgb((byte)64, (byte)0, (byte)0, (byte)255));
            this.Symbols.Add(diamond);

            var track = new Abnaki.Albiruni.Graphic.Curve.Track();
            track.Locations = new Location[]
            {
                MapCenter,
                new Location(MapCenter.Latitude + 0.1, MapCenter.Longitude - 0.2)
            };
            track.Stroke = Brushes.Blue;
            track.StrokeThickness = 2;
            this.Tracks.Add(track);
        }

    }

}
