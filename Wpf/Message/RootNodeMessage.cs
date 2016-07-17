using System;
using System.Collections.Generic;
using System.Linq;
using Abnaki.Albiruni.Tree;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Root of all data to view
    /// </summary>
    public class RootNodeMessage
    {
        public RootNodeMessage(Node root)
        {
            this.Root = root;
        }

        public Node Root { get; private set; }
    }
}
