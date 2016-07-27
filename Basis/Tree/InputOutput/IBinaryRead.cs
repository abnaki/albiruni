using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni.Tree.InputOutput
{
    public interface IBinaryRead : IDisposable
    {
        System.IO.BinaryReader Reader { get; }

        void Init(System.IO.Stream stream);

        /// <summary>Exists after Init()</summary>
        int? MeshPower { get; }

        void ReadSources();

        Source ReadSource();
    }
}
