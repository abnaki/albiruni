using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Tree.InputOutput
{
    public interface IBinaryWrite : IDisposable
    {
        void Init(System.IO.Stream stream, Mesh minimumMesh);

        System.IO.BinaryWriter Writer { get; }

        void WriteSources(Node root);

        void ReferenceSource(Source source);
    }
}
