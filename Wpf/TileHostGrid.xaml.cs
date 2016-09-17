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
            {  // seemingly magical requirement for Xceed DataGrid columns to exist
                // and yet a second Loaded may be necessary for the columns to exist
                FrameworkElement feparent = (FrameworkElement)Parent;

                if (feparent.IsLoaded)
                    DelayBind();
                else
                    feparent.Loaded += (s, e) => DelayBind();
            }
        }

        //bool init = false;

        void DelayBind()
        {
            Dispatcher.InvokeAsync(Bind, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        void Bind()
        {
            //if (init)
            //    return;

            //init = true;

            using (new WaitCursor())
            {
                Griddy.RestorePreferences<TileHostGrid>();

                IEnumerable<TiRecord> records = LocatorTemplate.Predefined().Select(t => new TiRecord(t)).ToList();

                Griddy.BindGrid(records);

                // this implies that, when 1 key is involved, 
                // Organization.UriDelimitUserKey can be "?paramname=" and UserKey is simply data.
                //   That requires fewer user inputs/tricks.
                // But when more than 1 key is involved, UriDelimitUserKey is "?" 
                //    and various paramnames must be lumped into Organization.UserKey.
                string keyTips = string.Join(Environment.NewLine,
                    "If your account with server",
                    "has a single key (or access token)",
                    "paste here.  For example, pk.geus783s37",
                    "If there are multiple keys,",
                    "paste as formatted http GET parameters.",
                    "For example, app_id=puWHx&app_code=luNuE31");

                Griddy.ConfigureColumns(new Col[]
                {
                new Col("Select"),
                new Col("Host"),
                new Col("Style"),
                new Col("PartialKey"){ Caption= "User Key", Tooltip = keyTips }
                });

                // initial selected record
                var rec = records.FirstOrDefault(r => r.LocatorTemplate == defaultLocatorTemplate);
                if (rec != null)
                {
                    rec.Select = true;
                    ExclusiveSelection(rec);
                }
            }
        }

        void GridEditCommitted(object sender, Abnaki.Windows.Software.Wpf.PreferredControls.Grid.Event.RecordCellEventArgs e)
        {
            TiRecord cur = (TiRecord)e.Record;

            switch (e.Field)
            {
                case "Select":
                    ApplySelection(cur);
                    ExclusiveSelection(cur);
                    break;

                case "PartialKey":
                    // multiple records may need change event but there is no logic in TiRecord to raise them,
                    // unless there were transitive PropertyChanged handlers
                    Griddy.Refresh();

                    if (cur.Select && false == cur.LocatorTemplate.Org.Public)
                    {
                        ApplySelection(cur);
                    }

                    break;
            }
        }

        void ApplySelection(TiRecord cur)
        {
            if (cur.Select)
            {
                if (cur.LocatorTemplate.Valid)
                {
                    TileHostMessage msg = new TileHostMessage(cur.LocatorTemplate);
                    
                    Dispatcher.InvokeAsync(() => MessageTube.Publish(msg), // want grid checkbox to take priority
                        System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    Abnaki.Windows.AbnakiLog.Comment("Selected invalid or underspecified server", cur.LocatorTemplate);
                }
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
