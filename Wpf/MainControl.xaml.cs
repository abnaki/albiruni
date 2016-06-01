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

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl,
        Abnaki.Windows.GUI.IMainControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        Abnaki.Windows.GUI.IDockSystem Abnaki.Windows.GUI.IMainControl.DockingSystem
        {
            get { return null; }
        }

        void Abnaki.Windows.GUI.IMainControl.ConfigureMenu(Abnaki.Windows.GUI.IMainMenu menu)
        {
            
        }
    }
}
