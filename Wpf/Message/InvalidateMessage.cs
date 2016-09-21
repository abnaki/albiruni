using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Message
{
    class InvalidateMessage
    {
        public InvalidateMessage(bool nodeLayerAffected)
        {
            this.NodeLayerAffected = nodeLayerAffected;
        }

        public bool NodeLayerAffected { get; private set; }

    }
}
