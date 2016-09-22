using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.ObjectModel;

using MapControl;
using Abnaki.Albiruni.Providers;
using Abnaki.Albiruni.Tree;
using Abnaki.Windows.Software.Wpf;

namespace Abnaki.Albiruni.Graphic
{
    /// <summary>
    /// Creates graphics from raw data in IFiles of many Sources
    /// </summary>
    class SourceMapper
    {
        internal class LayerOption
        {
            public LayerOption()
            {
                TrackBrush = Brushes.Blue;
                RouteBrush = Brushes.Magenta;
                WaypointBrush = Brushes.Red;
            }

            //public Pen Pen { get; set; }
            public Brush WaypointBrush { get; set; }
            public Brush TrackBrush { get; set; }
            public Brush RouteBrush { get; set; }
        }

        internal DirectoryInfo SourceDirectory { get; set; }

        readonly Dictionary<Source, LayerOption> MapSourceOption
            = new Dictionary<Source, LayerOption>();

        public void Clear()
        {
            MapSourceOption.Clear();
        }

        //internal void UpdateSource(Source source, MapViewModel mapvm)
        //{
        //    UpdateSources(new[] { source }, mapvm);
        //}

        internal void UpdateSources(IEnumerable<Source> sources, MapViewModel mapvm)
        {
            foreach (Source source in sources)
            {
                if (MapSourceOption.ContainsKey(source))
                {
                    IEnumerable<FrameworkElement> things = mapvm.Tracks.Concat(mapvm.Symbols);

                    foreach (var item in things.Where(t => t.Tag == source))
                    {
                        item.Visibility = source.Draw ? Visibility.Visible : Visibility.Hidden;
                    }
                }
                else if ( source.Draw )
                {
                    LayerOption opt = new LayerOption();

                    MapSourceOption[source] = opt;

                    MakeMapDecorations(source, opt, mapvm);
                }
            }
        }

        static Location LocationFromIPoint(IPoint p)
        {
            return new Location((double)p.Latitude, (double)p.Longitude);
        }

        void MakeMapDecorations(Source source, LayerOption opt, MapViewModel mapvm)
        {
            IFile ifile = source.RefreshIFileFromSource(this.SourceDirectory);
            if (ifile == null)
            {
                Abnaki.Windows.AbnakiLog.Comment("Failed to find IFile of " + source);
            }
            else
            {
                foreach (IPoint p in ifile.WayPoints )
                {
                    var wpt = MakeWaypoint(p, opt.WaypointBrush, mapvm);
                    wpt.Tag = source;
                }

                foreach (ITrack itrack in ifile.Tracks)
                {
                    var track = MakeTrack(itrack, opt, mapvm);
                    track.Tag = source;
                }

                foreach ( IRoute iroute in ifile.Routes )
                {
                    var route = MakeRoute(iroute, opt, mapvm);
                    route.Tag = source;
                }
            }
        }

        FrameworkElement MakeWaypoint(IPoint p, Brush brush, MapViewModel mapvm)
        {
            Symbol.SolidPoint waypt = new Symbol.SolidPoint(LocationFromIPoint(p), brush);

            mapvm.Symbols.Add(waypt);
            
            return waypt;
        }

        FrameworkElement MakeGenericTrail(IPointCollection pc, Brush brush, MapViewModel mapvm)
        {
            if (pc.Points.Count() == 1)
            {
                return MakeWaypoint(pc.Points.First(), brush, mapvm);
            }

            Curve.Track track = new Curve.Track();
            track.Locations = pc.Points.Select(p => LocationFromIPoint(p)).ToArray();
            track.Stroke = brush;
            track.StrokeThickness = 2;

            mapvm.Tracks.Add(track);

            return track;
        }

        FrameworkElement MakeTrack(ITrack itrack, LayerOption opt, MapViewModel mapvm)
        {
            return MakeGenericTrail(itrack, opt.TrackBrush, mapvm);
        }

        FrameworkElement MakeRoute(IRoute iroute, LayerOption opt, MapViewModel mapvm)
        {
            return MakeGenericTrail(iroute, opt.RouteBrush, mapvm);
        }
    }
}
