using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Abnaki.Albiruni.Tree;

namespace Abnaki.Albiruni.Message
{
    /// <summary>
    /// Root of all data to view
    /// </summary>
    public class RootNodeMessage
    {
        public RootNodeMessage(Node root, DirectoryInfo disource)
        {
            this.Root = root;
            this.SourceDirectory = disource;
        }

        public Node Root { get; private set; }

        public DirectoryInfo SourceDirectory { get; private set; }

        public readonly List<Source> Sources = new List<Source>();
    }
}
