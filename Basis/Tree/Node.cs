using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

using Geo;
using Geo.Abstractions.Interfaces;

using Abnaki.Albiruni.Tree.InputOutput;

namespace Abnaki.Albiruni.Tree
{

    public class Node
    {
        protected Node()
        {

        }

        public static Node NewGlobalRoot()
        {
            return new Node() { Axis = Axis.NorthSouth, Degrees = -90, Delta = 180 };
        }

        public Axis Axis { get; private set; }

        public double Degrees { get; private set; }

        public double Delta { get; private set; }

        //public Source Source { get; set; }

        System.Tuple<Node, Node> children = null;

        //Lazy<MultiValueDictionary<Source, IPosition>> mapSourcePositions = new Lazy<MultiValueDictionary<Source, IPosition>>(); // see Microsoft.Experimental.Collections
        Lazy<SortedList<Source, SourceContentSummary>> mapSourceSummaries = new Lazy<SortedList<Source, SourceContentSummary>>();

        // find/add descendants, given point(s)
        void Grow(Node grandparent, IEnumerable<IPosition> positions, Source source, double minDelta = 0.0001)
        {
            double newDelta = this.Delta / 2;
            if (grandparent != null)
                newDelta = grandparent.Delta / 2;

            var validPositions = positions.Where(p => this.ContainPosition(p)).ToArray();
            if ( false == validPositions.Any() )
                return;

            if (newDelta < minDelta + double.Epsilon) // <= 
            {
                // leaf node
                //mapSourcePositions.Value.AddRange(source, validPositions);

                mapSourceSummaries.Value[source] = new SourceContentSummary() { Points = validPositions.Length };

            }
            else
            // recurse, halving the applicable Delta
            {
                EnsureChildrenExist(grandparent);

                children.Item1.Grow(this, validPositions, source, minDelta);
                children.Item2.Grow(this, validPositions, source, minDelta);
            }
        }

        void EnsureChildrenExist(Node grandparent)
        {
            if ( children == null )
            {
                Axis newAxis = this.Axis == Albiruni.Axis.EastWest ? Albiruni.Axis.NorthSouth : Albiruni.Axis.EastWest;
                // i.e. same as grandparent if that exists

                double xmin, xmax;
                switch (newAxis)
                {
                    case Albiruni.Axis.NorthSouth:
                        xmin = -90;
                        xmax = 90;
                        break;
                    case Albiruni.Axis.EastWest:
                        xmin = -180;
                        xmax = 180;
                        break;
                    default:
                        throw new NotSupportedException("No support for Axis " + this.Axis);
                }
                if (grandparent != null)
                {
                    xmin = grandparent.Degrees;
                    xmax = xmin + grandparent.Delta;
                }

                Node lowChild = new Node() { Axis = newAxis, Degrees = xmin, Delta = (xmax - xmin) / 2 };

                Node highChild = new Node() { Axis = newAxis, Degrees = xmin + lowChild.Delta, Delta = lowChild.Delta };

                children = new Tuple<Node, Node>(lowChild, highChild);
            }
        }

        bool ContainPosition(IPosition pos)
        {
            Coordinate c = pos.GetCoordinate();
            double xc;
            switch (this.Axis)
            {
                case Albiruni.Axis.NorthSouth: xc = c.Latitude; break;
                case Albiruni.Axis.EastWest: xc = c.Longitude; break;
                default:
                    throw new NotSupportedException("No support for Axis " + this.Axis);
            }

            return ((this.Degrees - double.Epsilon < xc) && (xc < this.Degrees + this.Delta + double.Epsilon));
        }

        /// <summary>
        /// Grow tree to cover given data
        /// </summary>
        /// <param name="gpx"></param>
        public void Populate(Source source) // may want an interface
        {
            Geo.Gps.GpsData gdata = source.GpxFile.GeoGpsData;

            List<Coordinate> coordinates =
            gdata.Tracks
            .SelectMany(track => track.Segments)
            .SelectMany(seg => seg.Fixes)
            .Select(fix => fix.Coordinate)
            .ToList();

            coordinates.AddRange(
                gdata.Waypoints.Select(w => w.Coordinate));

            Grow(null, coordinates, source, minDelta: 1); // very coarse

        }

        public void Graft(Node grandparent, Node branch)
        {
            if (this.Axis != branch.Axis)
                throw new InvalidOperationException("Graft mismatch of Axis " + this.Axis + " vs " + branch.Axis);

            if (Math.Abs(this.Delta - branch.Delta) > double.Epsilon)
                throw new InvalidOperationException("Graft mismatch of Delta");

            if (Math.Abs(this.Degrees - branch.Degrees) > double.Epsilon)
                throw new InvalidOperationException("Graft mismatch of Degrees");

            if (branch.children != null)
            {
                EnsureChildrenExist(grandparent);

                children.Item1.Graft(this, branch.children.Item1);
                children.Item2.Graft(this, branch.children.Item2);
            }

             // graft sources
            foreach ( var pair in branch.mapSourceSummaries.Value )
            {
                this.mapSourceSummaries.Value.Add(pair.Key, pair.Value);
            }

        }

        public void DebugPrint()
        {
            Debug.WriteLine(this);

            Debug.Indent();

            foreach ( var pair in mapSourceSummaries.Value )
            {
                Debug.WriteLine(pair.Key + " " + pair.Value);

            }

            if ( children != null )
            {
                children.Item1.DebugPrint();
                children.Item2.DebugPrint();
            }

            Debug.Unindent();
        }

        const int filever = 1;

        public void Write(IBinaryWrite ibw)
        {
            BinaryWriter bw = ibw.Writer;
            bw.Write(filever);
            bw.Write((int)this.Axis);
            bw.Write(this.Degrees);
            bw.Write(this.Delta);

            bw.Write(children != null);
            if ( children != null )
            {
                children.Item1.Write(ibw);
                children.Item2.Write(ibw);
            }

            bw.Write(mapSourceSummaries.IsValueCreated);
            if ( mapSourceSummaries.IsValueCreated )
            {
                bw.Write(mapSourceSummaries.Value.Count);
                foreach ( var pair in mapSourceSummaries.Value )
                {
                    ibw.ReferenceSource(pair.Key);
                    pair.Value.Write(bw);
                }
            }
        }

        public void Read(IBinaryRead ibr)
        {
            BinaryReader br = ibr.Reader;
            int v = br.ReadInt32();
            this.Axis = (Axis)br.ReadInt32();
            this.Degrees = br.ReadDouble();
            this.Delta = br.ReadDouble();

            bool exist = br.ReadBoolean();
            if ( exist )
            {
                Node left = new Node();
                left.Read(ibr);
                Node right = new Node();
                right.Read(ibr);
                children = new Tuple<Node, Node>(left, right);
            }

            exist = br.ReadBoolean();
            if ( exist )
            {
                int n = br.ReadInt32();
                for ( int i = 0; i < n; i++ )
                {
                    Source source = ibr.ReadSource();
                    SourceContentSummary summary = new SourceContentSummary();
                    summary.Read(br);
                    mapSourceSummaries.Value.Add(source, summary);
                }
            }
        }

        internal void GetSources(SortedList<string, Source> mapPathSources)
        {
            if (mapSourceSummaries.IsValueCreated)
            {
                foreach ( var pair in mapSourceSummaries.Value )
                {
                    Source s = pair.Key;
                    mapPathSources[s.Path] = s;
                }
            }

            if ( children != null )
            {
                children.Item1.GetSources(mapPathSources);
                children.Item2.GetSources(mapPathSources);
            }
        }

        public override string ToString()
        {
            return this.Axis + " " + this.Degrees + " +" + this.Delta + " " + (this.Degrees + this.Delta);
        }

    }
}
