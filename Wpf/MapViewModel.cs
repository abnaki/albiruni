using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

using Abnaki.Albiruni.Tree;
using Abnaki.Albiruni.Graphic;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.Menu;
using MapControl;
using PropertyChanged;

namespace Abnaki.Albiruni
{
    [ImplementPropertyChanged]
    public class MapViewModel 
    {
        public MapViewModel()
        {
            MinimumMesh = new Mesh();

            Rectangles = new ObservableCollection<MapRectangle>();
            Symbols = new ObservableCollection<MapPath>();
            Tracks = new ObservableCollection<MapPath>();
            EmphasizedPaths = new ObservableCollection<MapPath>();
            ViewPortRect = new MapRectangle(); // unspecified

            SetCellBrush(cellHueLevel, 0, 0);

            ClearLastNodesFound();

            MessageTube.SubscribeCostly<Message.RootNodeMessage>(HandleTree);
            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);
        }

        public Location MapCenter { get; set; }

        /// <summary>
        /// will be minimum limit on Delta of a Node to show individually as a MapRectangle
        /// </summary>
        public Mesh MinimumMesh { get; set; }

        // want to use BulkObservableCollection or similar
        public ObservableCollection<MapRectangle> Rectangles { get; private set; }
        public ObservableCollection<MapPath> Symbols { get; private set; }
        public ObservableCollection<MapPath> Tracks { get; private set; }
        public ObservableCollection<MapPath> EmphasizedPaths { get; private set; }

        /// <summary>Last known rectangle; not bound to control.  i.e. set{} does not affect the map.
        /// </summary>
        MapRectangle ViewPortRect { get; set; }

        /// <summary>1 by 1 display units translated onto map</summary>
        MapRectangle DisplayUnitRect { get; set; }

        public void SetViewPort(MapRectangle viewRect, MapRectangle unitRect)
        {
            DisplayUnitRect = unitRect;

            if (this.ViewPortRect.EqualCoordinates(viewRect))
            {
                // skip
            }
            else
            {
                this.ViewPortRect = viewRect;
                UpdateAdornments();
            }
        }

        public void SetMesh(int power)
        {
            ClearLastNodesFound();
            this.MinimumMesh = new Mesh(power);
            UpdateAdornments();
        }

        public void UpdateAdornments()
        {
            UpdateRectangles(newRoot: false);
        }

        Node RootNode { get; set; }

        public void HandleTree(Message.RootNodeMessage msg)
        {
            //root.DebugPrint();

            ClearAdornments();
            ClearLastNodesFound();

            bool newRoot = msg.Root != RootNode;
            RootNode = msg.Root;

            UpdateRectangles(newRoot);
        }

        /// <summary>
        /// depends on RootNode and ViewPortRect
        /// </summary>
        void UpdateRectangles(bool newRoot)
        {
            //Rectangles.Clear();
            ClearAdornments();

            if (RootNode == null)
                return; // OK

            Node.Statistic stat = RootNode.GetStatistic();
            Debug.WriteLine(stat);
            PointSummary pointSummary = stat.ContentSummary.FinalSummary();
            if (pointSummary.Points > 0)
            {
                if (newRoot)
                {
                    decimal midLat = (pointSummary.MinLatitude.Value + pointSummary.MaxLatitude.Value) / 2;
                    decimal midLon = (pointSummary.MinLongitude.Value + pointSummary.MaxLongitude.Value) / 2;
                    MapCenter = new Location((double)midLat, (double)midLon);
                    // note that the view may still remain zoomed in far enough to not intersect any points in the tree.  ergo, user must zoom out.
                }

                // predicting bounds from last known view
                double viewWidth = ViewPortRect.East - ViewPortRect.West;
                double viewHeight = ViewPortRect.North - ViewPortRect.South;

                // rather than MapRectangle, want an abstraction to allow for two disjoint shapes 
                // such as two rectangles abutting longitude +- 180; 
                // and easily check for its intersection with a Node.

                DescentLimits limits = new DescentLimits();
                limits.LogicalBound = MapExtensions.NewMapRectangle(

                    west: MapCenter.Longitude - viewWidth / 2,
                    east: MapCenter.Longitude + viewWidth / 2,
                    north: MapCenter.Latitude + viewHeight / 2,
                    south: MapCenter.Latitude - viewHeight / 2
                );

                limits.MinimumDelta = this.MinimumMesh.Delta;

                Debug.WriteLine("limits " + limits);

                AddDescendantRectangles(RootNode, null, limits);

                if (Rectangles.Count > 0)
                {
                    Debug.WriteLine("Westernmost rectangle " + Rectangles.Min(r => r.West));
                    Debug.WriteLine("Easternmost rectangle " + Rectangles.Max(r => r.East));
                    Debug.WriteLine("Southernmost rectangle " + Rectangles.Min(r => r.South));
                    Debug.WriteLine("Northernmost rectangle " + Rectangles.Max(r => r.North));
                }
            }
        }

        class DescentLimits
        {
            public MapRectangle LogicalBound { get; set; }
            public decimal MinimumDelta { get; set; }

            public override string ToString()
            {
                return LogicalBound.ToStringUseful() + ", MinimumDelta=" + MinimumDelta;
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
        DescentResult AddDescendantRectangles(Node node, Node parent, DescentLimits limits)
        {
            switch (node.Axis)
            {
                case Axis.NorthSouth:
                    if (node.Degrees > (decimal)limits.LogicalBound.North)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < (decimal)limits.LogicalBound.South)
                        return DescentResult.None;
                    break;
                case Axis.EastWest:
                    if (node.Degrees > (decimal)limits.LogicalBound.East)
                        return DescentResult.None;
                    if (node.Degrees + node.Delta < (decimal)limits.LogicalBound.West)
                        return DescentResult.None;
                    break;
            }

            List<DescentResult> results = new List<DescentResult>();
            //DescentResult rchild1 = DescentResult.None, rchild2 = DescentResult.None;
            if ( node.Children != null )
            {
                results.Add(AddDescendantRectangles(node.Children.Item1, node, limits));
                results.Add(AddDescendantRectangles(node.Children.Item2, node, limits));
            }

            if ( node.SourceCount > 0)
            {
                results.Add(DescentResult.PointsCovered);
            }

            if ( node.Delta >= limits.MinimumDelta
                && results.Any(r => r == DescentResult.PointsCovered) 
                && parent != null && parent.Delta >= limits.MinimumDelta )
            {
                // any descendants exist, and node has not been excluded for view or precision reasons (above)

                MapRectangle r = null;
                switch (node.Axis)
                {
                    case Axis.NorthSouth:
                        if ( (double)node.Delta < this.DisplayUnitRect.North - DisplayUnitRect.South)
                            break; //  not displayed

                        r = new MapRectangle()
                        {
                            South = (double)node.Degrees,
                            North = (double)(node.Degrees + node.Delta),
                            West = (double)parent.Degrees,
                            East = (double)(parent.Degrees + parent.Delta)
                        };
                        break;
                    case Axis.EastWest:
                        if ( (double)node.Delta < this.DisplayUnitRect.East - DisplayUnitRect.West)
                            break; //  not displayed

                        r = new MapRectangle()
                        {
                            South = (double)parent.Degrees,
                            North = (double)(parent.Degrees + parent.Delta),
                            West = (double)node.Degrees,
                            East = (double)(node.Degrees + node.Delta)
                        };
                        break;
                    default:
                        throw new NotSupportedException("No support for axis " + node.Axis);
                }

                if (r != null)
                {
                    CompleteRectangle(r);

                    this.Rectangles.Add(r); // efficient ?

                    results.Add(DescentResult.NewRectangles);
                }
            }

            if (results.Count > 0)
                return (DescentResult)results.Max();
            else
                return DescentResult.None;
        }

        void CompleteRectangle(MapRectangle r)
        {
            r.Fill = m_defaultFillBrush;

            //r.MouseLeftButtonUp += MapRectangle_MouseLeftButtonUp; // never gets  e.ClickCount == 2
            r.MouseLeftButtonDown += MapRectangle_MouseLeftButtonDown;

            // need to be able to pan ParentMap if mouse hit rectangle.
            r.AddHandler(UIElement.MouseDownEvent, new RoutedEventHandler(RouteToParentMap));
            r.AddHandler(UIElement.MouseMoveEvent, new RoutedEventHandler(RouteToParentMap));
            r.AddHandler(UIElement.MouseUpEvent, new RoutedEventHandler(RouteToParentMap));
            
        }

        static void RouteToParentMap(object sender, RoutedEventArgs e)
        {
            MapPath r = (MapPath)sender;
            r.ParentMap.RaiseEvent(e);
            e.Handled = true;
        }

        void MapRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MapRectangle r = (MapRectangle)sender;
                MapBase map = r.ParentMap;
                System.Windows.Point p = e.GetPosition(map);
                Location loc = map.ViewportPointToLocation(p);

                OnLeftDoubleClick(loc);
            }
        }

        void ClearAdornments()
        {
            Rectangles.Clear();
            Symbols.Clear();
            Tracks.Clear();
            EmphasizedPaths.Clear();
        }

        Location lastLocation;
        Node.FindResult lastNodeSpan;

        void ClearLastNodesFound()
        {
            lastLocation = null;
            lastNodeSpan = null;
        }

        public void OnHover(Location loc)
        {
            bool hoverChanged;

            Message.SourceRecordMessage msg = SourceRecordOfLocation(loc, out hoverChanged); // depends on MinimumMesh

            if (msg != null && hoverChanged
                && msg.FinalSummary.Points > 0) // implies msg.NodeExtremes must contain non-nulls
            {
                MessageTube.Publish(msg);

                EmphasizedPaths.Clear();

                //MapRectangle r = MapExtensions.NewMapRectangle(ps.MinLongitude.Value, ps.MaxLongitude.Value, ps.MinLatitude.Value, ps.MaxLatitude.Value);
                // ps=msg.FinalSummary is inadequate for visibility; could be a single point or small/thin cluster.
                // Also MapControl 2.9 did not draw any MapRectangle Stroke.

                Graphic.Curve.OutlineRectangle r = new Graphic.Curve.OutlineRectangle(
                    west: msg.NodeExtremes.MinLong.Value,
                    east: msg.NodeExtremes.MaxLong.Value,
                    south: msg.NodeExtremes.MinLat.Value,
                    north: msg.NodeExtremes.MaxLat.Value);

                r.Stroke = Brushes.Yellow;
                r.StrokeThickness = 2;

                EmphasizedPaths.Add(r);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>non null only if Nodes exist at location
        /// </returns>
        Message.SourceRecordMessage SourceRecordOfLocation(Location loc, out bool changed)
        {
            changed = false;

            if (RootNode == null)
                return null;

            if (loc.EqualCoordinates(lastLocation))
            {
                // lastNodes are correct
            }
            else
            {
                Node.FindResult nodes = new Node.FindResult();
                RootNode.FindNodes((decimal)loc.Latitude, (decimal)loc.Longitude, this.MinimumMesh, nodes);
                
                changed = (lastNodeSpan == null || false == lastNodeSpan.SameNodes(nodes));

                lastLocation = loc;
                lastNodeSpan = nodes;

            }

            Message.SourceRecordMessage msg = null;

            if (lastNodeSpan != null)
            {
                msg = new Message.SourceRecordMessage(lastNodeSpan);
            }

            return msg;

            //if (nodes.Count > 0)
            //{
            //    Debug.WriteLine("Hovered on ");
            //    Debug.Indent();
            //    foreach (Node n in nodes)
            //    {
            //        Debug.WriteLine(n);
            //    }
            //    Debug.Unindent();
            //}
        }

        void OnLeftDoubleClick(Location loc)
        {
            bool locationChanged; // unused.  user can invoke a source multiple times, not required to move mouse.
            Message.SourceRecordMessage recsMsg = SourceRecordOfLocation(loc, out locationChanged);
            if (recsMsg != null)
            {
                int n = recsMsg.SourceRecords.Count();
                if (n == 1)
                {
                    Message.InvokeSourceMessage msg = new Message.InvokeSourceMessage(recsMsg.SourceRecords.Single());
                    MessageTube.Publish(msg);
                }
                else
                {
                    Debug.WriteLine("Not invoking " + n + " sources.  Expect uniqueness.");
                }
            }
        }

        System.Windows.Media.Brush m_defaultFillBrush;
        const int cellHueLevel = 255;
        const int cellAlpha = 48;

        void SetCellBrush(int red, int green, int blue)
        {
            m_defaultFillBrush = new SolidColorBrush(Color.FromArgb((byte)cellAlpha, (byte)red, (byte)green, (byte)blue));
            UpdateRectangles(newRoot: false);
        }

        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            if (false == msg.Checked)
                return;

            switch (msg.Key)
            {
                case Menu.OptionMenuKey.MapCellColorRed:
                    SetCellBrush(cellHueLevel, 0, 0);
                    break;
                case Menu.OptionMenuKey.MapCellColorGreen:
                    SetCellBrush(0, cellHueLevel, 0);
                    break;
                case Menu.OptionMenuKey.MapCellColorBlue:
                    SetCellBrush(0, 0, cellHueLevel);
                    break;
            }
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
