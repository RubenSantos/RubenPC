using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Estudo
{

    /*  
           Esta implementação reflete a semântica de sincronização do semáforo 
           no sistema operativo W indows, mas não é threadsafe.
           Implemente em Java , sem utilizar locks , uma versão threadsafe
           deste semáforo.
       */
    class UnsafeSpinWindowsSemaphore
    {
        private int limit, count;
        public UnsafeSpinWindowsSemaphore(int count, int limit)
        {
            if (count < 0 || count > limit) throw new InvalidOperationException("bad count/limit");
            this.count = count; this.limit = limit;
        }
        public void await()
        {
            while (count == 0) Thread.Yield();
            --count;
        }
        public void release(int rcount)
        {
            if (count + rcount < count || count + rcount > limit)
                throw new InvalidOperationException("limit exceeded");
            count += rcount;
        }
    }

    class PC_1516i_2
    {

        class SafeSpinWindowsSemaphore
        {
            private int count;
            private int limit;

            public SafeSpinWindowsSemaphore(int count, int limit)
            {
                if (count < 0 || count > limit) throw new InvalidOperationException("bad count/limit");
                this.count = count; this.limit = limit;
            }

            public void Await()
            {
                do
                {
                    int obs = count;
                    if (count == 0)
                        Thread.Yield();
                    if (Interlocked.CompareExchange(ref count, obs - 1, obs) == obs)
                        return;
                } while (true);
            }

            public void Release(int rcount)
            {
                while (true)
                {
                    int obs = count;
                    if (obs + rcount < count || obs + rcount > limit)
                        throw new InvalidOperationException("limit exceeded");
                    if (Interlocked.CompareExchange(ref count, obs + rcount, obs) == obs)
                        return;
                }
            }
        }
    }
}
