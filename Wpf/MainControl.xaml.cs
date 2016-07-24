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

using Abnaki.Windows.GUI;
using Abnaki.Windows.Software.Wpf.Ultimate;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainControl : UserControl,
        Abnaki.Windows.GUI.IMainControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        Menu.FileMenuBus fmbus;
        Menu.OptionMenuBus optbus;
        ProcessLauncher procLauncher = new ProcessLauncher();

        IDockSystem IMainControl.DockingSystem
        {
            get { return new Abnaki.Windows.Software.Wpf.PreferredControls.Docking.AvalonDockSystem(this.Docky, 2); }
        }

        void IMainControl.ConfigureMenu(IMainMenu menu)
        {
            fmbus = new Menu.FileMenuBus(menu);
            optbus = new Menu.OptionMenuBus(menu);
        }

        void IMainControl.EmplacedInWindow()
        {
            MainTitle("Albiruni");
        }

        public event Action<string> MainTitle; // IMainControl

    }
}
