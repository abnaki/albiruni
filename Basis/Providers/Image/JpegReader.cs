using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Abnaki.Albiruni.Providers.Image
{
    class JpegReader : FileReader
    {
        protected override IFile OpenFile(FileInfo fi)
        {
            JpegFile jf = new JpegFile();
            jf.Read(fi);

            return jf;
        }
    }
}
