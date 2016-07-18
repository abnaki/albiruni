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
        public SourceRecordMessage(IEnumerable<Node> nodes)
        {
            this.SourceRecords = CreateSourceRecords(nodes);
        }

        public IEnumerable<SourceRecord> SourceRecords { get; private set; }

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
