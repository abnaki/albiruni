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
    /// Construct a LagTimer and add this.OnChanged() as a handler of the original event.
    /// </summary>
    /// <typeparam name="Targ">
    /// event passes Targ as in void delegate(object,Targ)
    /// </typeparam>
    class LagTimer<Targ>
        where Targ : class
    {
        public LagTimer()
        {
            vptimer.Tick += vptimer_Tick;
        }

        /// <summary>Watched.  Do not add costly handlers.
        /// </summary>
        public event Action<object,Targ> Changed;

        /// <summary>Timer expired.  Handlers can do significant work.
        /// </summary>
        public event Action<object, Targ> Settled;

        Targ LastArg { get; set; }

        DispatcherTimer vptimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        void vptimer_Tick(object sender, EventArgs e)
        {
            vptimer.Stop();

            var h = Settled;
            if (h != null)
                h(sender, LastArg);

            LastArg = null;
        }


        public void OnChanged(object sender, Targ e)
        {
            LastArg = e;
            vptimer.Stop(); vptimer.Start(); // MS idea of Reset

            var h = Changed;
            if (h != null)
                h(sender, e);
        }
    }
}
