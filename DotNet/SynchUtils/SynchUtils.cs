using System;
using System.Threading;

namespace Utils
{
    public static class SynchUtils {
        public static int RemainingTimeout(int refTime, int timeout) {
            return (int)RemainingTimeout(refTime, (long)timeout);
        }

        public static long RemainingTimeout(int refTime, long timeout)
        {
            if (timeout == Timeout.Infinite) return timeout;
            return Math.Max(0, timeout - (Environment.TickCount - refTime));
        }


        private static bool EnterUninterruptible(object obj, out bool interrupted) {
            interrupted = false;
            do {
                try {
                    Monitor.Enter(obj);

                }
                catch (ThreadInterruptedException) {
                    interrupted = true;
                }
            }
            while (true);
        }

        public static void Wait(this object monitor, object cond, int timeout) {
            if (monitor == cond) {
                Monitor.Wait(monitor, timeout);
            }
            Monitor.Enter(cond);
            Monitor.Exit(monitor);
            try {
                Monitor.Wait(cond, timeout);
            }
            finally {
                bool interrupted;
                Monitor.Exit(cond);
                EnterUninterruptible(monitor, out interrupted);
                if (interrupted)
                    throw new ThreadInterruptedException();
            }
        }

        public static void Wait(this object monitor, object cond) {
            monitor.Wait(cond, Timeout.Infinite);
        }

        public static void Signal(this object monitor, object cond) {
            if (monitor == cond) {
                Monitor.Pulse(monitor);
                return;
            }
            bool interrupted;
            EnterUninterruptible(cond, out interrupted);
            Monitor.Pulse(cond);
            Monitor.Exit(cond);
            if (interrupted)
                Thread.CurrentThread.Interrupt();
        }

        public static void SignalAll(this object monitor, object cond) {
            if (monitor == cond) {
                Monitor.PulseAll(monitor);
                return;
            }
            bool interrupted;
            EnterUninterruptible(cond, out interrupted);
            Monitor.PulseAll(cond);
            Monitor.Exit(cond);
            if (interrupted)
                Thread.CurrentThread.Interrupt();
        }

    }

}
