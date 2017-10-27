using System;
using System.Threading;

namespace Utils
{
    public static class SynchUtils {
        public static int RemainingTimeout(int refTime, int timeout) {
            if (timeout == Timeout.Infinite) return timeout;
            return Math.Max(0, timeout - (Environment.TickCount - refTime));
        }


        private static void EnterUninterruptible(object obj, out bool interrupted) {
            interrupted = false;
            do {
                try {
                    Monitor.Enter(obj);
                    return;
                }
                catch (ThreadInterruptedException) {
                    interrupted = true;
                }
            }
            while (true);
        }

        public static bool Await(this object monitor, object cond, int timeout) {
            if (monitor == cond) {
                return Monitor.Wait(monitor, timeout);
            }
            Monitor.Enter(cond);
            Monitor.Exit(monitor);
            try {
                return Monitor.Wait(cond, timeout);
            }
            finally {
                bool interrupted;
                Monitor.Exit(cond);
                EnterUninterruptible(monitor, out interrupted);
                if (interrupted)
                    throw new ThreadInterruptedException();
            }
        }

        public static bool Await(this object monitor, object cond) {
            return monitor.Await(cond, Timeout.Infinite);
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
