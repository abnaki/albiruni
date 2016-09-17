using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Issued to draw Source graphically in detail
    /// </summary>
    class DrawSourceMessage
    {
        public DrawSourceMessage(SourceRecord r)
        {
            this.SourceRecord = r;
        }

        public SourceRecord SourceRecord { get; private set; }
    }
}
