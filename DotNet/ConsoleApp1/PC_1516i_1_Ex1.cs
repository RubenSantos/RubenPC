using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Estudo
{
    /*
    * A implementação deste sincronizador, cuja semântica de sincronização é idêntica à do tipo 
    * Lazy<T> do . NET Framework, não é threadsafe. Sem utilizar locks, 
    * implemente uma versão threadsafe deste sincronizador.
    */
    public class UnsafeSpinLazy<T> where T : class
    {
        private const int UNCREATED = 0, BEING_CREATED = 1, CREATED = 2;

        private int state = UNCREATED;
        private Func<T> factory;
        private T value;
        public UnsafeSpinLazy(Func<T> factory) { this.factory = factory; }
        public bool IsValueCreated { get { return state == CREATED; } }
        public T Value
        {
            get
            {
                SpinWait sw = new SpinWait();
                do
                {
                    if (state == CREATED)
                    {
                        break;
                    }
                    else if (state == UNCREATED)
                    {
                        state = BEING_CREATED; value = factory(); state = CREATED; break;
                    }
                    sw.SpinOnce();
                } while (true);
                return value;
            }
        }
    }
    class PC_1516i_1_Ex1
    {
        public class SafeSpinLazy<T> where T : class
        {
            private const int UNCREATED = 0, BEING_CREATED = 1, CREATED = 2;

            private volatile int state = UNCREATED;
            private Func<T> factory;
            private T value;
            public SafeSpinLazy(Func<T> func)
            {
                factory = func;
            }
            public bool IsValueCreated
            {
                get { return state == CREATED; }
            }

            public T Value
            {
                get
                {
                    SpinWait sw = new SpinWait();
                    do
                    {
                        int obs = state;
                        if (state == CREATED)
                            return value;
                        if (obs == UNCREATED &&
                            Interlocked.CompareExchange(ref state, BEING_CREATED, obs) == obs)
                        {
                            value = factory();
                            state = CREATED;
                            return value;
                        }
                        sw.SpinOnce();
                    } while (true);
                }
            }

        }
    }
}
