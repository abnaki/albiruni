using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.PreferredControls.Grid;
using Abnaki.Windows;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// </summary>
    public partial class SourceDetailer : UserControl
    {
        public SourceDetailer()
        {
            InitializeComponent();

            MessageTube.SubscribeCostly<Message.SourceRecordMessage>(UpdateSources);
            MessageTube.Subscribe<Message.RootNodeMessage>(OnRoot);

            this.grid.DoubleClickedRecord += grid_DoubleClickedRecord;
        }

        DirectoryInfo sourceDirectory;

        void OnRoot(Message.RootNodeMessage msg)
        {
            sourceDirectory = msg.SourceDirectory;
        }

        void UpdateSources(Message.SourceRecordMessage msg)
        {
            this.grid.BindGrid(msg.SourceRecords);

            this.grid.ConfigureColumns(new Col[] {
                new Col(){ Field = "Path" },
                new Col(){ Field = "Waypoints" },
                new Col(){ Field = "Trackpoints" }
            });
        }

        void grid_DoubleClickedRecord(object weakRecord)
        {
            SourceRecord record = (SourceRecord)weakRecord;

            FileInfo fisource = AbnakiFile.CombinedFilePath(sourceDirectory, record.Path);
            if ( fisource.Exists )
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
