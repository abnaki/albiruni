﻿using System;
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

            this.grid.DoubleClickedRecord += grid_DoubleClickedRecord;
        }

        void UpdateSources(Message.SourceRecordMessage msg)
        {
            this.grid.BindGrid(msg.SourceRecords);

            this.grid.ConfigureColumns(new Col[] {
                new Col("Path"),
                new Col("Waypoints"),
                new Col("Trackpoints"),
                new Col("MinTime"){ Caption = "First UTC" },
                new Col("MaxTime"){ Caption = "Last UTC"}
            });
            
            // in the future, want a generalized optional way for grid to display non-null DateTime ToLocalTime.
        }

        void grid_DoubleClickedRecord(object weakRecord)
        {
            SourceRecord record = (SourceRecord)weakRecord;

            Message.InvokeSourceMessage msg = new Message.InvokeSourceMessage(record);
            MessageTube.Publish(msg);

        }

    }
}
