using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Abnaki.Windows.GUI;
using Abnaki.Windows.Software.Wpf;
using Abnaki.Windows.Software.Wpf.Menu;
using Abnaki.Windows.Software.Wpf.Ultimate;
using Abnaki.Albiruni.Tree;


namespace Abnaki.Albiruni
{
    class FileMenuBus : ButtonBus<FileMenuKey>
    {
        public FileMenuBus(IMainMenu menu)
        {
            menu.AddCommandChild(TopMenuKey.File, FileMenuKey.Open, "_Open...");
        }

        WPFFolderBrowser.WPFFolderBrowserDialog folderDialog = new WPFFolderBrowser.WPFFolderBrowserDialog("Open Folder");

        protected override void HandleButton(ButtonMessage<FileMenuKey> m)
        {
            base.HandleButton(m);

            switch ( m.Key )
            {
                case FileMenuKey.Open:
                    FileOpen();
                    break;
            }
        }

        void FileOpen()
        {
            if ( true == folderDialog.ShowDialog(Application.Current.MainWindow) )
            {
                using (new WaitCursor())
                {
                    // want to move to a worker thread, provide dialog for progress and interrupt button.

                    DirectoryInfo di = new DirectoryInfo(folderDialog.FileName);
                    DirectoryInfo ditarget = di.CreateSubdirectory("albiruni");

                    var root = Abnaki.Albiruni.Tree.Node.NewGlobalRoot();

                    Nursery.Guidance guidance = new Nursery.Guidance();
                    guidance.MinimumPrecision = 90 / System.Math.Pow(2, 12); // may eventually have UI

                    Nursery.GrowTree(root, di, ditarget, guidance);

                    MessageTube.Publish(root);
                }
            }
        }
    }

    enum FileMenuKey
    {
        Open
    }
}
