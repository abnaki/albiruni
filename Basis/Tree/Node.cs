﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

//using Geo;
//using Geo.Abstractions.Interfaces;
using Abnaki.Albiruni.Providers;
using Abnaki.Albiruni.Tree.InputOutput;

namespace Abnaki.Albiruni.Tree
{

    public class Node
    {
        /// <summary>usually only for deserialization</summary>
        public Node()
        {

        }

        public static Node NewGlobalRoot()
        {
            return new Node() { Axis = Axis.NorthSouth, Degrees = -90, Delta = 180 };
        }

        public Axis Axis { get; private set; }

        public decimal Degrees { get; private set; }

        public decimal Delta { get; private set; }

        public System.Tuple<Node, Node> Children { get; private set; }

        //Lazy<MultiValueDictionary<Source, IPosition>> mapSourcePositions = new Lazy<MultiValueDictionary<Source, IPosition>>(); // see Microsoft.Experimental.Collections
        Lazy<SortedList<Source, SourceContentSummary>> mapSourceSummaries = new Lazy<SortedList<Source, SourceContentSummary>>();

        public int SourceCount
        {
            get
            {
                if (mapSourceSummaries.IsValueCreated)
                    return mapSourceSummaries.Value.Count;
                else
                    return 0;
            }
        }

        public IEnumerable<KeyValuePair<Source,SourceContentSummary>> GetSourcesSummaries()
        {
            if (mapSourceSummaries.IsValueCreated)
                return mapSourceSummaries.Value;

            return Enumerable.Empty<KeyValuePair<Source, SourceContentSummary>>();
        }

        // find/add descendants, given point(s)
        void Grow(Node grandparent, PointDump positions, Source source, decimal minDelta)
        {
            decimal newDelta = this.Delta / 2;
            if (grandparent != null)
                newDelta = grandparent.Delta / 2;

            PointDump validPoints = positions.SuchThat(this.ContainPosition);
            //var validPositions = source.GpxFile.AllPoints.Where(p => this.ContainPosition(p)).ToArray();
            if (false == validPoints.Any())
                return;

            if (newDelta < minDelta )
            {
                // leaf node
                //mapSourcePositions.Value.AddRange(source, validPositions);

                mapSourceSummaries.Value[source] = new SourceContentSummary(validPoints);

            }
            else
            // recurse, halving the applicable Delta
            {
                EnsureChildrenExist(grandparent);

                Children.Item1.Grow(this, validPoints, source, minDelta);
                Children.Item2.Grow(this, validPoints, source, minDelta);
            }
        }

        void EnsureChildrenExist(Node grandparent)
        {
            if ( Children == null )
            {
                Axis newAxis = this.Axis == Albiruni.Axis.EastWest ? Albiruni.Axis.NorthSouth : Albiruni.Axis.EastWest;
                // i.e. same as grandparent if that exists

                decimal xmin, xmax;
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

                Children = new Tuple<Node, Node>(lowChild, highChild);
            }
        }

        bool ContainPosition(IPoint p)
        {
            return ContainPosition(p.Latitude, p.Longitude);
        }

        bool ContainPosition(decimal latitude, decimal longitude)
        {
            decimal xc;
            switch (this.Axis)
            {
                case Albiruni.Axis.NorthSouth: xc = latitude; break;
                case Albiruni.Axis.EastWest: xc = longitude; break;
                default:
                    throw new NotSupportedException("No support for Axis " + this.Axis);
            }

            return ContainPosition(xc);
        }

        bool ContainPosition(decimal degrees)
        {
            return ((this.Degrees <= degrees) && (degrees < this.Degrees + this.Delta));
        }

        /// <summary>
        /// Span of Degrees and/or Degrees+Delta of nodes
        /// </summary>
        public class Span
        {
            public decimal? MinLat;
            public decimal? MaxLat;
            public decimal? MinLong;
            public decimal? MaxLong;

            public void NarrowTo(Node node)
            {
                // narrowing the span, not widening it, by nature of NullableExtreme signs
               switch ( node.Axis )
               {
                   case Axis.EastWest:
                       this.MinLong = Numerical.NullableExtreme(this.MinLong, node.Degrees, 1);
                       this.MaxLong = Numerical.NullableExtreme(this.MaxLong, node.Degrees + node.Delta, -1);
                       break;

                   case Axis.NorthSouth:
                       this.MinLat = Numerical.NullableExtreme(this.MinLat, node.Degrees, 1);
                       this.MaxLat = Numerical.NullableExtreme(this.MaxLat, node.Degrees + node.Delta, -1);
                       break;
               }
            }
        }

        public class FindResult
        {
            public readonly List<Node> List = new List<Node>();

            public readonly Span NodeSpan = new Span();

            public void Add(Node node)
            {
                this.List.Add(node);
            }

            public bool SameNodes(FindResult other)
            {
                return this.List.SequenceEqual(other.List);
            }
        }

        public void FindNodes(decimal latitude, decimal longitude, Mesh precision, FindResult nodefind)
        {
            if (Delta >= precision.Delta ) 
            {
                if (ContainPosition(latitude, longitude))
                {
                    nodefind.NodeSpan.NarrowTo(this);

                    AddNodesHavingSources(nodefind, recurse: false);

                    if (Children != null)
                    {
                        Children.Item1.FindNodes(latitude, longitude, precision, nodefind);
                        Children.Item2.FindNodes(latitude, longitude, precision, nodefind);
                    }
                }
            }
            else
            {
                // All descendant nodes are within precision.
                // It is sufficient for the last ancestor such that Delta>=precision.Delta to contain (latitude,longitude).
                // That is, FindResult can have sources from cousins of the node that contains the point.
                AddNodesHavingSources(nodefind, recurse: true);
            }
        }

        void AddNodesHavingSources(FindResult nodefind, bool recurse)
        {
            if (mapSourceSummaries.IsValueCreated && mapSourceSummaries.Value.Count > 0)
            {
                nodefind.Add(this);
            }

            if (recurse && Children != null)
            {
                Children.Item1.AddNodesHavingSources(nodefind, recurse);
                Children.Item2.AddNodesHavingSources(nodefind, recurse);
            }
        }

        /// <summary>
        /// Grow tree to cover given data
        /// </summary>
        public void Populate(Source source, Mesh minimumMesh) 
        {
            Grow(null, source.PointProvider.Points, source, minDelta: minimumMesh.Delta);
        }

        public void Graft(Node grandparent, Node branch)
        {
            if (this.Axis != branch.Axis)
                throw new InvalidOperationException("Graft mismatch of Axis " + this.Axis + " vs " + branch.Axis);

            if (this.Delta != branch.Delta)
                throw new InvalidOperationException("Graft mismatch of Delta");

            if (this.Degrees != branch.Degrees)
                throw new InvalidOperationException("Graft mismatch of Degrees");

            if (branch.Children != null)
            {
                EnsureChildrenExist(grandparent);

                Children.Item1.Graft(this, branch.Children.Item1);
                Children.Item2.Graft(this, branch.Children.Item2);
            }

            MergeSourceSummaries(branch);

        }

        void MergeSourceSummaries(Node other)
        {
            var map = this.mapSourceSummaries.Value;

            foreach (var pair in other.mapSourceSummaries.Value)
            {
                SourceContentSummary otherSummary = pair.Value;
                if (map.ContainsKey(pair.Key))
                {
                    SourceContentSummary existing = map[pair.Key];
                    existing.AggregateWith(otherSummary);
                }
                else
                {
                    map.Add(pair.Key, otherSummary);
                }
            }
        }

        public void GetSources(SortedSet<Source> sources)
        {
            if (mapSourceSummaries.IsValueCreated)
            {
                foreach (Source key in mapSourceSummaries.Value.Keys)
                {
                    sources.Add(key);
                }
            }
            if ( Children != null )
            {
                Children.Item1.GetSources(sources);
                Children.Item2.GetSources(sources);
            }
        }

        public void DebugPrint()
        {
            // not interested in dead-end nodes in entire tree
            if (mapSourceSummaries.IsValueCreated || Children != null)
            {
                Debug.WriteLine(this);

                Debug.Indent();

                foreach (var pair in mapSourceSummaries.Value)
                {
                    Debug.WriteLine(pair.Key + " " + pair.Value);

                }

                if (Children != null)
                {
                    Children.Item1.DebugPrint();
                    Children.Item2.DebugPrint();
                }

                Debug.Unindent();
            }
        }

        const int filever = 3;

        public void Write(IBinaryWrite ibw)
        {
            BinaryWriter bw = ibw.Writer;
            bw.Write(filever);
            bw.Write((int)this.Axis);
            bw.Write(this.Degrees);
            bw.Write(this.Delta);

            bw.Write(Children != null);
            if ( Children != null )
            {
                Children.Item1.Write(ibw);
                Children.Item2.Write(ibw);
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
            if (v < 3)
                throw new NotSupportedException("Old version");

            this.Axis = (Axis)br.ReadInt32();
            this.Degrees = br.ReadDecimal();
            this.Delta = br.ReadDecimal();

            bool exist = br.ReadBoolean();
            if ( exist )
            {
                Node left = new Node();
                left.Read(ibr);
                Node right = new Node();
                right.Read(ibr);
                Children = new Tuple<Node, Node>(left, right);
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

        internal void GetSources(List<Source> sources)
        {
            if (mapSourceSummaries.IsValueCreated)
            {
                sources.AddRange(mapSourceSummaries.Value.Keys);
            }

            if ( Children != null )
            {
                Children.Item1.GetSources(sources);
                Children.Item2.GetSources(sources);
            }
        }

        public enum FathomResult { None, Descendants, This };

        public FathomResult Fathom(decimal minimumDelta)
        {
            FathomResult r = FathomResult.None;

            if ( Children != null )
            {
                FathomResult rc1 = Children.Item1.Fathom(minimumDelta);
                FathomResult rc2 = Children.Item2.Fathom(minimumDelta);

                r = (FathomResult)Math.Max((int)rc1, (int)rc2);
                
                if ( r == FathomResult.This )
                {
                    MergeSourceSummaries(Children.Item1);
                    MergeSourceSummaries(Children.Item2);
                    Children = null;
                    r = FathomResult.Descendants;
                }
            }

            if (Delta < minimumDelta)
                r = FathomResult.This;

            return r;
        }

        public override string ToString()
        {
            return this.Axis + " " + this.Degrees + " +" + this.Delta + " " + (this.Degrees + this.Delta);
        }

        public Statistic GetStatistic()
        {
            Statistic stat = new Statistic();
            AccumulateStatistic(stat);
            return stat;
        }

        void AccumulateStatistic(Statistic stat)
        {
            stat.Nodes ++;

            if ( mapSourceSummaries.IsValueCreated)
            {
                foreach ( var pair in mapSourceSummaries.Value )
                {
                    stat.ContentSummary.AggregateWith(pair.Value);
                }
            }
            if ( Children != null )
            {
                Children.Item1.AccumulateStatistic(stat);
                Children.Item2.AccumulateStatistic(stat);
            }
        }

        public class Statistic
        {
            public Statistic()
            {
                this.ContentSummary = new SourceContentSummary();
            }

            /// <summary>
            /// Count of self and descendants
            /// </summary>
            public int Nodes { get; set; }

            /// <summary>
            /// Totals and extremes of self and all descendants
            /// </summary>
            public SourceContentSummary ContentSummary { get; private set; }

            public override string ToString()
            {
                return Nodes + " nodes, " + ContentSummary;
            }
        }
    }
}
