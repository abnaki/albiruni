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
using Abnaki.Windows.Software.Wpf.Diplomat;
using System.ComponentModel;
using System.Windows.Input;
using Abnaki.Windows;
using System.Text;


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

                Nursery.SearchForFiles(di, guidance);

                if ( 0 == (guidance.PotentialSourceFileCount ?? 0  ) )
                {
                    Abnaki.Windows.Software.Wpf.Diplomat.Notifier.Error("No compatible files exist under " + di.FullName);
                }
                else
                {
                    string question = string.Format("Read {0} possible file(s) totaling {1:N0} bytes ?", 
                        guidance.PotentialSourceFileCount, guidance.PotentialSourceFileBytes);

                    if ( MessageBox.Show(Application.Current.MainWindow, question, "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question)
                        == MessageBoxResult.OK)
                    {
                        AbnakiLog.Comment(question, di.FullName);

                        OpenTreeState state = new OpenTreeState() { RootDirectory = di, Guidance = guidance };
                        GuiOpenDirectory(state);
                    }
                }
            }
        }

        class OpenTreeState
        {
            public Message.RootNodeMessage RootMessage { get; set; }
            public DirectoryInfo RootDirectory { get; set; }
            public Nursery.Guidance Guidance { get; set; }
            public string FinalError { get; set; }
        }

        //void GuiOpenDirectory(OpenTreeState state)
        //{
        //    MessageTube.Publish(new Message.InvalidateMessage()); // now dialog is out of the map's way.
        //    WaitCursor.Progressing();
        //    OpenDirectoryTree(state);
        //    OpenTreeResponse(state);
        //}

        void GuiOpenDirectory(OpenTreeState state) // BackgroundWorker.  no big payoff yet but user could Exit.
        {
            // want to provide dialog for progress and cancel button.
            // also see Xceed BusyIndicator.

            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += (sender, e) =>
            {
                BackgroundWorker th = (BackgroundWorker)sender;
                OpenTreeState st = (OpenTreeState)e.Argument;
                //st.Guidance.SourceFileCounted += n => th.ReportProgress((int)((double)n / (double)st.Guidance.PotentialSourceFileCount));
                OpenDirectoryTree(st);
                e.Result = e.Argument;
            };
            thread.RunWorkerCompleted += (sender, e) =>
            {
                OpenTreeResponse((OpenTreeState)e.Result);
                WaitCursor.Default();
            };
            thread.WorkerSupportsCancellation = true;
            //thread.WorkerReportsProgress = true;
            //thread.ProgressChanged += thread_ProgressChanged;
            WaitCursor.Progressing();
            thread.RunWorkerAsync(state);

            MessageTube.Publish(new Message.InvalidateMessage()); // now dialog is out of the map's way.

            //    Notifier.Error("Failed to completely read " + di.FullName + " within " + tsLimit);
        }

        void OpenDirectoryTree(OpenTreeState state)
        {
            DirectoryInfo ditarget = null;
            try
            {
                ditarget = state.RootDirectory.CreateSubdirectory(Nursery.CacheDir);
            }
            catch ( Exception ex ) // permissions?
            {
                state.FinalError = "Unable to create " + Nursery.CacheDir + " folder for future speedup.";
                AbnakiLog.Exception(ex, "Creating " + Nursery.CacheDir);
            }

            var root = Abnaki.Albiruni.Tree.Node.NewGlobalRoot();

            state.RootMessage = new Message.RootNodeMessage(root, state.RootDirectory);
            state.Guidance.SourceCounted += (source, count) => state.RootMessage.Sources.Add(source);

            Nursery.GrowTree(root, state.RootDirectory, ditarget, state.Guidance);

            Node.Statistic rootStat = root.GetStatistic();
            Debug.WriteLine("Tree of data in " + state.RootDirectory.FullName + "  " + rootStat.ContentSummary.FinalSummary());
        }

        void thread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        void OpenTreeResponse(OpenTreeState state)
        {
            OpenTreeCompletion();

            MessageTube.Publish(state.RootMessage);

            StringBuilder sbErr = new StringBuilder();

            if (state.Guidance.FilesExceptions.Count > 0)
            {
                foreach (var pair in state.Guidance.FilesExceptions)
                {
                    Abnaki.Windows.AbnakiLog.Exception(pair.Value, "Error due to " + pair.Key);
                }
                sbErr.Append(state.Guidance.FilesExceptions.Count + " error(s).  ");
            }

            if ( false == string.IsNullOrEmpty(state.FinalError) )
            {
                sbErr.Append(state.FinalError);
            }

            if (sbErr.Length > 0)
            {
                Abnaki.Windows.Software.Wpf.Diplomat.Notifier.Error(sbErr.ToString());
                MessageTube.Publish(new Message.InvalidateMessage()); // now dialog is out of the map's way.
            }
        }

        void OpenTreeCompletion()
        {
            WaitCursor.Default();
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
