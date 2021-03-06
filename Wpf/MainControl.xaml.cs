﻿using System;
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
using Abnaki.Windows.Software.Wpf.PreferredControls.Docking;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Albiruni.Message;
using System.Diagnostics;

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

            MessageTube.Subscribe<RootNodeMessage>(HandleRoot);


            // EventManager.RegisterClassHandler(typeof(Control), Control.KeyDownEvent, new KeyEventHandler(GlobalKeyUp));
        }

        Menu.FileMenuBus fmbus;
        Menu.OptionMenuBus optbus;
        Menu.HelpMenuBus helpbus;
        TileHost.Governor tileGovernor = new TileHost.Governor();
        ProcessLauncher procLauncher = new ProcessLauncher();

        IDockSystem IMainControl.DockingSystem
        {
            get { return new AvalonDockSystem(this.Docky, ver: 4); }
        }

        void IMainControl.ConfigureMenu(IMainMenu menu)
        {
            fmbus = new Menu.FileMenuBus(menu);
            optbus = new Menu.OptionMenuBus(menu);
            helpbus = new Menu.HelpMenuBus(menu);
        }

        void IMainControl.EmplacedInWindow()
        {
            UpdateRootDirectory(null);
        }

        private void HandleRoot(RootNodeMessage msg)
        {
            UpdateRootDirectory(msg.SourceDirectory.FullName);
        }

        void UpdateRootDirectory(string path)
        {
            MainTitle("Albiruni " + path);
        }

        public event Action<string> MainTitle;  // IMainControl

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tileGovernor.Complete();
        }

        Key m_lastKey = Key.None;

        void GlobalKeyUp(object sender, KeyEventArgs e)
        {
            if (m_lastKey != e.Key)
                Debug.WriteLine(e.Key + " from " + e.OriginalSource.GetType().FullName);

            m_lastKey = e.Key;
        }

    }
}
