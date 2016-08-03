using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Helps delay logic after interactive panning/zooming has stopped briefly.
    /// </summary>
    class ViewPortTimer
    {
        public ViewPortTimer(MapControl.MapBase map)
        {
            vptimer.Tick += vptimer_Tick;

            map.ViewportChanged += map_ViewportChanged;
        }

        /// <summary>Map ViewportChanged.  Do not add costly handlers.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>Timer expired.  Handlers can do significant work.
        /// </summary>
        public event EventHandler Settled;

        DispatcherTimer vptimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        void vptimer_Tick(object sender, EventArgs e)
        {
            vptimer.Stop();

            var h = Settled;
            if (h != null)
                h(sender, e);
        }


        void map_ViewportChanged(object sender, EventArgs e)
        {
            vptimer.Stop(); vptimer.Start(); // MS idea of Reset

            var h = Changed;
            if (h != null)
                h(sender, e);
        }
    }
}
