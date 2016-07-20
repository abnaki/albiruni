using System;
using System.Collections.Generic;
using System.Linq;

using Abnaki.Albiruni.Tree;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Subset of nodes and their descriptive SourceRecords
    /// </summary>
    class SourceRecordMessage
    {
        public SourceRecordMessage(Node.FindResult nodefind)
        {
            this.SourceRecords = CreateSourceRecords(nodefind.List);

            IEnumerable<SourceContentSummary> summaries =
                this.SourceRecords.Select(r => r.Summary);

            SourceContentSummary unionSummaries = new SourceContentSummary(summaries);
            this.FinalSummary = unionSummaries.FinalSummary();

            this.NodeExtremes = nodefind.NodeSpan;
        }

        public IEnumerable<SourceRecord> SourceRecords { get; private set; }

        public PointSummary FinalSummary { get; private set; }

        public Node.Span NodeExtremes { get; private set; }

        public static IEnumerable<SourceRecord> CreateSourceRecords(IEnumerable<Node> nodes)
        {
            var groups = from node in nodes
                         from pair in node.GetSourcesSummaries()
                         let source = pair.Key
                         let summary = pair.Value
                         group summary by source into g
                         select g;

            return (from g in groups
                             let aggregateSummary = new SourceContentSummary(g)
                             select new SourceRecord(g.Key, aggregateSummary)).ToList();

        }
    }
}
