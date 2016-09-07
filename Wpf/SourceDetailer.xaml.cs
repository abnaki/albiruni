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
using Abnaki.Windows.Software.Wpf.Ultimate;
using Abnaki.Windows.Software.Wpf.Menu;
using System.Globalization;

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
            MessageTube.SubscribeCostly<Message.RootNodeMessage>(HandleTree);
            MessageTube.Subscribe<FarewellMessage>(Farewell);

            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);

            Loaded += SourceDetailer_Loaded;
            this.grid.DoubleClickedRecord += grid_DoubleClickedRecord;
        }

        void SourceDetailer_Loaded(object sender, RoutedEventArgs e)
        {
            grid.RestorePreferences<SourceDetailer>();
        }

        private void HandleTree(Message.RootNodeMessage msg)
        {
            this.grid.ClearData();
            // no old data
        }

        void UpdateSources(Message.SourceRecordMessage msg)
        {
            Abnaki.Windows.Software.Wpf.PreferredControls.Docking.Paneling.ShowPanel(this);

            this.grid.BindGrid(msg.SourceRecords);

            ReconfigureColumns();
        }

        void ReconfigureColumns()
        {
            if (this.grid.DataContext == null)
                return;

            this.grid.ConfigureColumns(new Col[] {
                new Col("Path"){ Caption = "File"}, // Tooltip="Double-click to open externally"
                new Col("Waypoints"),
                new Col("Trackpoints"){ Caption = "Track points"},
                new Col("Routepoints"){ Caption = "Route points"},
                new Col("MinTime"){ Caption = "First UTC", Format = DateTimeFormat },
                new Col("MaxTime"){ Caption = "Last UTC", Format = DateTimeFormat}
            });

            // in the future, want a generalized optional way for grid to display non-null DateTime ToLocalTime.

        }

        void grid_DoubleClickedRecord(object weakRecord)
        {
            SourceRecord record = (SourceRecord)weakRecord;

            Message.InvokeSourceMessage msg = new Message.InvokeSourceMessage(record);
            MessageTube.Publish(msg);

        }

        void Farewell(FarewellMessage msg)
        {
            grid.SavePreferences<SourceDetailer>();
        }

        string DateTimeFormat { get; set; }

        void UpdateDateTimeFormat(string timeFmt)
        {
            DateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
                + " " + timeFmt;

            ReconfigureColumns();
        }


        private void HandleOption(ButtonMessage<Menu.OptionMenuKey> msg)
        {
            switch (msg.Key)
            {
                case Menu.OptionMenuKey.DetailTimeShort:
                    UpdateDateTimeFormat(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
                    break;

                case Menu.OptionMenuKey.DetailTimeLong:
                    UpdateDateTimeFormat(CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
                    break;
            }
        }

    }
}
