using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Estudo
{
    class ExplicitMonitors
    {
        class AutoResetEvent
        {
            private object monitor = new object();
            private bool signaled;

            private LinkedList<bool> waiters;

            public AutoResetEvent(bool initialState)
            {
                signaled = initialState;
                waiters = new LinkedList<bool>();
            }

            public void Signal()
            {
                lock (monitor)
                {
                    if (waiters.Count > 0)
                    {
                        var wNode = waiters.First;
                        waiters.RemoveFirst();
                        wNode.Value = true;
                        Monitor.PulseAll(monitor);
                    }
                    else
                        signaled = true;
                }
            }

            public void SignalAll()
            {
                lock (monitor)
                {
                    for (var wNode = waiters.First; wNode != null; wNode = wNode.Next)
                        wNode.Value = true;
                    Monitor.PulseAll(monitor);
                }
            }

            public bool Wait(int timeout)
            {
                lock (monitor)
                {
                    if (signaled)
                    {
                        signaled = false;
                        return true;
                    }
                    var wNode = waiters.AddLast(false);
                    while (true)
                    {
                        try
                        {
                            int refTime = Environment.TickCount;
                            Monitor.Wait(monitor, timeout);
                            if (wNode.Value)
                                return true;
                            timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                            if(timeout == 0)
                            {
                                waiters.Remove(wNode);
                                return false;
                            }
                        }
                        catch(ThreadInterruptedException e)
                        {
                            if(wNode.Value == true)
                            {
                                Thread.CurrentThread.Interrupt();
                                return true;
                            }
                            waiters.Remove(wNode);
                            throw e;
                        }
                    }
                }
            }
        }
    }
}
