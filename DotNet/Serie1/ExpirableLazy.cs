using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Serie1
{
    public class ExpirableLazy<T> where T: class
    {
        object monitor = new object();
        Func<T> provider;
        TimeSpan timeToLive;
        int startingTicks;
        bool inProcess;
        private T tValue;

        public ExpirableLazy(Func<T> provider, TimeSpan timeToLive)
        {
            this.provider = provider;
            this.timeToLive = timeToLive;
        }

        public T Value
        {
            get {
                lock (monitor) 
                {
                    if (tValue != null 
                        && SynchUtils.RemainingTimeout(startingTicks, timeToLive.Ticks) > 0)
                        return tValue;
                    if (tValue == null 
                        || SynchUtils.RemainingTimeout(startingTicks, timeToLive.Ticks) == 0)
                    {
                        while (inProcess)
                            Monitor.Wait(monitor);
                        inProcess = true;
                        try
                        {
                            tValue = provider();
                        }
                        catch
                        {
                            inProcess = false;
                            Monitor.Pulse(monitor);
                            throw;
                        }
                        inProcess = false;
                        Monitor.PulseAll(monitor);
                        return tValue;
                    }
                }
                return tValue;
            }

            private set
            {
                tValue = value;
                startingTicks = Environment.TickCount;
            }
        }
    }
}
