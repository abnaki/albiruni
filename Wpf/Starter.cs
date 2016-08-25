using System;
using System.Collections.Generic;
using System.Linq;

namespace Abnaki.Albiruni
{
    class Starter
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Abnaki.Windows.Software.Wpf.Starter.CommonSettings(Properties.Settings.Default);

            return Abnaki.Windows.Software.Wpf.Ultimate.UltimateStarter<MainControl>.Start(args);
        }
    }
}
