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

        IDockSystem IMainControl.DockingSystem
        {
            get { return null; }
        }

        void IMainControl.ConfigureMenu(IMainMenu menu)
        {
            
        }

        void IMainControl.EmplacedInWindow()
        {
            MainTitle("Albiruni");
        }

        public event Action<string> MainTitle; // IMainControl
    }
}
