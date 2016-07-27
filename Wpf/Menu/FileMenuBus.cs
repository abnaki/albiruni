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
using System.Diagnostics;


namespace Abnaki.Albiruni.Menu
{
    class FileMenuBus : ButtonBus<FileMenuKey>
    {
        public FileMenuBus(IMainMenu menu)
        {
            menu.AddCommand(new MenuSeed<FileMenuKey>(FileMenuKey.Open, "_Open...")
            {
                ParentKey = TopMenuKey.File,
                Tooltip = "Open a top-level folder containing files or other folders to search."
            });

            ButtonBus<Menu.OptionMenuKey>.HookupSubscriber(HandleOption);
        }

        Mesh minimumMesh = new Mesh(14);

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
            if (true == folderDialog.ShowDialog(Application.Current.MainWindow))
            {
                DirectoryInfo di = new DirectoryInfo(folderDialog.FileName);

                Nursery.Guidance guidance = new Nursery.Guidance();
                guidance.MinimumMesh = minimumMesh; // may eventually have UI

                List<FileInfo> potentialFiles = new List<FileInfo>();
                Nursery.SearchForFiles(di, guidance, potentialFiles);

                if (potentialFiles.Count == 0)
                {
                    Abnaki.Windows.Software.Wpf.Diplomat.Notifier.Error("No compatible files exist under " + di.FullName);
                }
                else
                {
                    string question = string.Format("Read {0} possible file(s) totaling {1:N0} bytes ?", potentialFiles.Count, potentialFiles.Sum(f => f.Length));

                    if (MessageBoxResult.OK ==
                        MessageBox.Show(Application.Current.MainWindow, question, "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                        )
                    {
                        // want to move to a worker thread, provide dialog for progress and interrupt button.
                        using (new WaitCursor())
                        {
                            DirectoryInfo ditarget = di.CreateSubdirectory(Nursery.CacheDir);

                            var root = Abnaki.Albiruni.Tree.Node.NewGlobalRoot();
                            Message.RootNodeMessage msg = new Message.RootNodeMessage(root, di);

                            Nursery.GrowTree(root, di, ditarget, guidance);

                            MessageTube.Publish(msg);

                            Node.Statistic rootStat = root.GetStatistic();
                            Debug.WriteLine("Tree of data in " + di.FullName + "  " + rootStat.ContentSummary.FinalSummary());
                        }

                        if (guidance.FilesExceptions.Count > 0)
                        {
                            foreach (var pair in guidance.FilesExceptions)
                            {
                                Abnaki.Windows.AbnakiLog.Exception(pair.Value, "Error due to " + pair.Key);
                            }
                            string msg = guidance.FilesExceptions.Count + " error(s)";
                            Abnaki.Windows.Software.Wpf.Diplomat.Notifier.Error(msg);

                        }
                    }
                }
            }
        }

        private void HandleOption(ButtonMessage<OptionMenuKey> msg)
        {
            Menu.OptionMenuBus.GetMeshFromOption(msg.Key, p => minimumMesh = new Mesh(p));

        }


    }

    enum FileMenuKey
    {
        Open
    }
}
