using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Externally launch or display a source natively.
    /// </summary>
    class InvokeSourceMessage
    {
        public InvokeSourceMessage(SourceRecord record)
        {
            this.SourceRecord = record;
        }

        public SourceRecord SourceRecord { get; private set; }
    }
}
