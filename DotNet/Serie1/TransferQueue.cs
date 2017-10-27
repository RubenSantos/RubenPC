using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Serie1
{
    public class TransferQueue<T>
    {
        private object monitor = new object();
        private LinkedList<T> queue = new LinkedList<T>();

        public void Put(T msg)
        {
            lock (monitor)
            {
                queue.AddLast(msg);
                monitor.Signal(queue);
            }
        }

        public bool Transfer(T msg, int timeout)
        {
            lock (monitor)
            {
                queue.AddLast(msg);
                monitor.Signal(queue);
                try
                {
                    do
                    {
                        timeout = SynchUtils.RemainingTimeout(Environment.TickCount, timeout);
                        if (!monitor.Await(msg, timeout) && queue.Contains(msg))
                        {
                            queue.Remove(msg);
                            return false;
                        }
                    } while (queue.Contains(msg));
                }
                catch(ThreadInterruptedException)
                {
                    if (queue.Contains(msg))
                    {
                        queue.Remove(msg);
                        throw;
                    }
                    Thread.CurrentThread.Interrupt();
                    return true;
                    
                }
                return true;
            }
        }

        public bool Take(int timeout, out T rmsg)
        {
            lock (monitor)
            {
                if(queue.Count > 0)
                {
                    rmsg = queue.First.Value;
                    queue.RemoveFirst();
                    monitor.Signal(rmsg);
                    return true;
                }
                do
                {
                    timeout = SynchUtils.RemainingTimeout(Environment.TickCount, timeout);
                    if(!monitor.Await(queue, timeout))
                    {
                        rmsg = default(T);
                        return false;
                    }
                } while (queue.Count == 0);
                rmsg = queue.First.Value;
                queue.RemoveFirst();
                monitor.Signal(rmsg);
                return true;
            }
        }
    }
}
