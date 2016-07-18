using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using Abnaki.Windows;
using Abnaki.Windows.Software.Wpf;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Launch an external Process
    /// </summary>
    class ProcessLauncher
    {
        public ProcessLauncher()
        {
            MessageTube.Subscribe<Message.RootNodeMessage>(OnRoot);
            MessageTube.SubscribeCostly<Message.InvokeSourceMessage>(InvokeSource);
        }

        DirectoryInfo sourceDirectory;

        void OnRoot(Message.RootNodeMessage msg)
        {
            sourceDirectory = msg.SourceDirectory;
        }

        void InvokeSource(Message.InvokeSourceMessage msg)
        {
            FileInfo fisource = AbnakiFile.CombinedFilePath(sourceDirectory, msg.SourceRecord.Path);
            if (fisource.Exists)
            {
                //Debug.WriteLine("Want to open " + fisource.FullName);
                using (new WaitCursor())
                {
                    ProcessStartInfo psi = new ProcessStartInfo(fisource.FullName);
                    using (Process.Start(psi))
                    {
                        // indeed
                    }
                }
            }
            else
            {
                Abnaki.Windows.Software.Wpf.Diplomat.Notifier.Error("Nonexistent " + fisource.FullName);
            }

        }
    }
}
