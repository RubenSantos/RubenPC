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
                    if (Value != null 
                        && SynchUtils.RemainingTimeout(startingTicks, timeToLive.Ticks) > 0)
                        return Value;
                    if (Value == null 
                        || SynchUtils.RemainingTimeout(startingTicks, timeToLive.Ticks) == 0)
                    {
                        while (inProcess)
                            Monitor.Wait(monitor);
                        inProcess = true;
                        try
                        {
                            Value = provider();
                        }
                        catch
                        {
                            inProcess = false;
                            Monitor.Pulse(monitor);
                            throw;
                        }
                        inProcess = false;
                        Monitor.PulseAll(monitor);
                        return Value;
                    }
                }
                return Value;
            }

            private set
            {
                Value = value;
                startingTicks = Environment.TickCount;
            }
        }
    }
}
