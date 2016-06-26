using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media;

using Abnaki.Albiruni.Tree;
using Abnaki.Albiruni.Graphic;
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
            MinimumPrecision = 0.2;

            MessageTube.Subscribe<Node>(HandleTree);
        }

        public Location MapCenter { get; set; }

        // want to use BulkObservableCollection or similar
        public ObservableCollection<MapRectangle> Rectangles { get; private set; }
        public ObservableCollection<MapPath> Symbols { get; private set; }
        public ObservableCollection<MapPath> Tracks { get; private set; }

        public double MinimumPrecision { get; set; }

        /// <summary>Last known rectangle; not bound to control.  i.e. set{} does not affect the map.
        /// </summary>
        MapRectangle ViewPortRect { get; set; }

        public void SetViewPort(MapRectangle rect)
        {
            if (this.ViewPortRect.EqualCoordinates(rect))
            {
                // skip
            }
            else
            {
                this.ViewPortRect = rect;
                UpdateRectangles(newRoot: false);
            }
        }

        Node RootNode { get; set; }

        System.Windows.Media.Brush m_defaultFillBrush = new SolidColorBrush(Color.FromArgb((byte)32, (byte)255, (byte)0, (byte)0));


        public void HandleTree(Node root)
        {
            //root.DebugPrint();

            bool newRoot = root != RootNode;
            RootNode = root;

            UpdateRectangles(newRoot);
        }

        /// <summary>
        /// depends on RootNode and ViewPortRect
        /// </summary>
        void UpdateRectangles(bool newRoot)
        {
            Rectangles.Clear();

            if (RootNode == null)
                return; // OK

            Node.Statistic stat = RootNode.GetStatistic();
            Debug.WriteLine(stat);
            if (stat.ContentSummary.Points > 0)
            {
                if (newRoot)
                {
                    double midLat = (stat.ContentSummary.MinLatitude.Value + stat.ContentSummary.MaxLatitude.Value) / 2;
                    double midLon = (stat.ContentSummary.MinLongitude.Value + stat.ContentSummary.MaxLongitude.Value) / 2;
                    MapCenter = new Location(midLat, midLon);
                    // note that the view may still remain zoomed in far enough to not intersect any points in the tree.  ergo, user must zoom out.
                }

                // predicting bounds from last known view
                double viewWidth = ViewPortRect.East - ViewPortRect.West;
                double viewHeight = ViewPortRect.North - ViewPortRect.South;

                // rather than MapRectangle, want an abstraction to allow for two disjoint shapes 
                // such as two rectangles abutting longitude +- 180; 
                // and easily check for its intersection with a Node.

                MapRectangle logicalBound = MapExtensions.NewMapRectangle(

                    west: MapCenter.Longitude - viewWidth / 2,
                    east: MapCenter.Longitude + viewWidth / 2,
                    north: MapCenter.Latitude + viewHeight / 2,
                    south: MapCenter.Latitude - viewHeight / 2
                );

                Debug.WriteLine("logicalBound " + logicalBound.ToStringUseful());

                AddDescendantRectangles(RootNode, null, logicalBound);

                if (Rectangles.Count > 0)
                {
                    Debug.WriteLine("Westernmost rectangle " + Rectangles.Min(r => r.West));
                    Debug.WriteLine("Easternmost rectangle " + Rectangles.Max(r => r.East));
                    Debug.WriteLine("Southernmost rectangle " + Rectangles.Min(r => r.South));
                    Debug.WriteLine("Northernmost rectangle " + Rectangles.Max(r => r.North));
                }
            }
        }

        enum DescentResult
        {
            /// <summary>no intersection of descendants with given region</summary>
            None = 0,
            /// <summary>descendants have any points in a given region</summary>
            PointsCovered = 1,
            /// <summary>implies PointsCovered and rectangles were added covering descendant points</summary>
            NewRectangles = 2
        };

        /// <summary>
        /// Here is the principal motivation for Albiruni.
        /// Add a Rectangle for every Node in tree that intersects ViewPortRect,
        /// not descending to any Nodes having Delta < MinimumPrecision.
        /// </summary>
        /// <returns>true if anything added</returns>
        DescentResult AddDescendantRectangles(Node node, Node parent, MapRectangle logicalBound)
        {
            switch (node.Axis)
            {
                case Axis.NorthSouth:
                    if (node.Degrees > logicalBound.North)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < logicalBound.South)
                        return DescentResult.None;
                    break;
                case Axis.EastWest:
                    if (node.Degrees > logicalBound.East)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < logicalBound.West)
                        return DescentResult.None;
                    break;
            }

            //if (node.Delta < MinimumPrecision)
            //{
            //    if (node.SourceCount > 0)
            //        return DescentResult.PointsCovered;
            //    else
            //        return DescentResult.None;
            //}

            List<DescentResult> results = new List<DescentResult>();
            //DescentResult rchild1 = DescentResult.None, rchild2 = DescentResult.None;
            if ( node.Children != null )
            {
                results.Add(AddDescendantRectangles(node.Children.Item1, node, logicalBound));
                results.Add(AddDescendantRectangles(node.Children.Item2, node, logicalBound));
            }

            if ( node.SourceCount > 0)
            {
                results.Add(DescentResult.PointsCovered);
            }

            if ( results.Any(r => r == DescentResult.PointsCovered) 
                && parent != null && parent.Delta >= MinimumPrecision )
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
                
                results.Add(DescentResult.NewRectangles);
            }

            if (results.Count > 0)
                return (DescentResult)results.Max();
            else
                return DescentResult.None;
        }

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
