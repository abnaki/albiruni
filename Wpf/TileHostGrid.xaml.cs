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
using Abnaki.Windows.Software.Wpf.Ultimate;

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

            Griddy.GridEditCommitted += GridEditCommitted;

            MessageTube.Subscribe<TileHostMessage>(HandleTileHost);
            MessageTube.Subscribe<FarewellMessage>(Farewell);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if (Parent is FrameworkElement)
                ((FrameworkElement)Parent).Loaded += Griddy_Loaded; // seemingly magical requirement for Xceed DataGrid columns to exist
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
            Griddy.RestorePreferences<TileHostGrid>();

            IEnumerable<TiRecord> records = LocatorTemplate.Predefined().Select(t => new TiRecord(t)).ToList();

            Griddy.BindGrid(records);

            Griddy.ConfigureColumns(new Col[]
            {
                new Col("Select"),
                new Col("Host"),
                new Col("Style"),
                new Col("PartialKey"){ Caption= "User Key", Tooltip = "access token, abbreviated, if defined in " + TileHostSupply.ConfigFilename }
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

        private void HyperlinkNavigate(object sender, RequestNavigateEventArgs e)
        {
            using ( var p = System.Diagnostics.Process.Start(e.Uri.ToString()))
            {
                // right
            }
        }
        
        void Farewell(FarewellMessage msg)
        {
            Griddy.SavePreferences<TileHostGrid>();
        }
    }
}
