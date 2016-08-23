using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using Abnaki.Albiruni.TileHost;
using Abnaki.Albiruni.Message;
using Abnaki.Windows.Software.Wpf.PreferredControls.Grid;
using Abnaki.Windows.Software.Wpf;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Interaction logic for TileHostGrid.xaml
    /// </summary>
    public partial class TileHostGrid : UserControl
    {
        public TileHostGrid()
        {
            InitializeComponent();

            //this.LayoutUpdated += TileHostGrid_LayoutUpdated;
            Griddy.Loaded += Griddy_Loaded;

            Griddy.GridEditCommitted += GridEditCommitted;

            MessageTube.Subscribe<TileHostMessage>(HandleTileHost);

        }

        bool init = false;

        void Griddy_Loaded(object sender, RoutedEventArgs e)
        {
            if ( ! init )
            {
                init = true;
                Bind();
            }
        }

        void Bind()
        {
            IEnumerable<TiRecord> records = LocatorTemplate.Predefined().Select(t => new TiRecord(t)).ToList();

            Griddy.BindGrid(records);

            Griddy.ConfigureColumns(new Col[]
            {
                new Col("Select"),
                new Col("Host"),
                new Col("Style"),
            });

            // initial selected record
            var rec = records.FirstOrDefault(r => r.LocatorTemplate == defaultLocatorTemplate);
            if (rec != null)
            {
                rec.Select = true;
                ExclusiveSelection(rec);
            }

        }

        void GridEditCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e, string field)
        {
            if (field == "Select")
            {
                TiRecord cur = (TiRecord)e.Item;

                var records = e.CollectionView.Cast<TiRecord>();

                if ( cur.Select )
                {
                    TileHostMessage msg = new TileHostMessage(cur.LocatorTemplate);
                    MessageTube.Publish(msg);
                }

                ExclusiveSelection(cur);
            }
        }

        LocatorTemplate defaultLocatorTemplate;

        void HandleTileHost(TileHostMessage msg)
        {
            Debug.WriteLine(GetType().Name + " handles " + msg.LocatorTemplate);

            defaultLocatorTemplate = msg.LocatorTemplate;
        }

        void ExclusiveSelection(TiRecord current)
        {
            foreach ( var other in Griddy.DataContext.Data.Cast<TiRecord>() )
            {
                if (other != current)
                    other.Select = false;
            }
        }
        
    }
}
