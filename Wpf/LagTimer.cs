using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Abnaki.Albiruni
{
    /// <summary>
    /// Provides a delayed or lagging event after the originally handled event stopped for a short time.
    /// The original event is typically too rapid-firing, 
    /// and handlers of the Settled event are designed to be expensive/slow.
    /// </summary>
    class LagTimer
    {
        public LagTimer(Action<EventHandler> addHandler)
        {
            vptimer.Tick += vptimer_Tick;

            addHandler(ThingChanged);
        }

        /// <summary>Watched.  Do not add costly handlers.
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


        void ThingChanged(object sender, EventArgs e)
        {
            vptimer.Stop(); vptimer.Start(); // MS idea of Reset

            var h = Changed;
            if (h != null)
                h(sender, e);
        }
    }
}
