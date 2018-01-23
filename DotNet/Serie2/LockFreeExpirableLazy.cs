using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serie2
{
    class LockFreeExpirableLazy<T> where T: class
    {

        Func<T> provider;
        TimeSpan timeToLive;
        T value;

        Stopwatch stopwatch;
        volatile bool inProgress;

        public LockFreeExpirableLazy(Func<T> provider, TimeSpan timeToLive)
        {
            this.provider = provider;
            this.timeToLive = timeToLive;
        }

        public T Value
        {
            get
            {
                if (value != null && stopwatch.Elapsed < timeToLive)
                    return value;
                while (true)
                {
                    if(!inProgress)
                        try
                        {
                            value = provider();
                            stopwatch = Stopwatch.StartNew();
                        }
                        catch
                        {
                            inProgress = false;
                            throw;
                        }

                }
            }
        }
    }
}
